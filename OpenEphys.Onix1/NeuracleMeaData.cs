using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("只用于测试Neuracle头盒相关指标")]
public class NeuracleMeaData : Source<NeuracleMeaDataFrame>
{
    /// <summary>
    /// 采集设备名称
    /// </summary>
    private string _dataDeviceName;

    /// <summary>
    /// 模拟开关设备名称
    /// </summary>
    private string _switchDeviceName;

    private HubName _hubName;
    [Description("选择的头盒")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public HubName HubName
    {
        get
        {
            return _hubName;
        }
        set
        {
            _hubName = value;
            if (NeuracleGlobalState.HubNameToDeviceName.TryGetValue(_hubName, out var deviceTuple))
            {
                _dataDeviceName = deviceTuple.Item1;
                _switchDeviceName = deviceTuple.Item3;
            }
        }
    }

    [Description("缓存的帧大小")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public int BufferSize { get; set; } = NeuracleGlobalState.BufferSize;

    /// <summary>
    /// 采集数据的增益
    /// </summary>
    public static float DataScale = (float)(4096 / Math.Pow(2, 23) / 12);

    /// <summary>
    /// HubClock首位置1时记录采集刺激开始切换时间
    /// </summary>
    private ulong? _switchTimeStart = null;

    /// <summary>
    /// 采集波形大于阈值时记录采集刺激结束切换时间
    /// </summary>
    private ulong? _switchTimeEnd = null;

    /// <summary>
    /// 阈值
    /// </summary>
    private const int _threshold = -8;

    public unsafe override IObservable<NeuracleMeaDataFrame> Generate()
    {
        var bufferSize = BufferSize;
        _switchTimeStart = null;
        _switchTimeEnd = null;
        return DeviceManager.GetDevice(_dataDeviceName).SelectMany(
            deviceInfo => Observable.Create<NeuracleMeaDataFrame>(observer =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(NeuracleData));
                //这个就相当于时间戳
                var hubClockBuffer = new ulong[bufferSize];
                var clockBuffer = new ulong[bufferSize];
                var sampleIndex = 0;
                var amplifierBuffer = new int[NeuracleData.AmplifierChannelCount * bufferSize];
                var frameObserver = Observer.Create<oni.Frame>(
                    frame =>
                    {
                        var payload = (NeuracleHubDataPayload*)frame.Data.ToPointer();
                        //这个hubClock是先低8字节，再高8字节
                        var hubClock = payload->HubClock;
                        if (NeuracleUtils.IsHubClockFirstBitOne(hubClock))
                        {
                            _switchTimeStart ??= NeuracleUtils.HubClockToTime(hubClock);
                        }
                        hubClockBuffer[sampleIndex] = NeuracleUtils.HubClockToTime(hubClock);
                        clockBuffer[sampleIndex] = frame.Clock;
                        Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, sampleIndex * NeuracleData.AmplifierChannelCount, NeuracleData.AmplifierChannelCount);
                        //取出通道0的幅度，和阈值比较
                        var channel0Amp = DataScale * amplifierBuffer[sampleIndex * NeuracleData.AmplifierChannelCount];
                        if (channel0Amp < _threshold)
                        {
                            if (_switchTimeStart is not null)
                            {
                                //这里只要走一次
                                if (_switchTimeEnd is null)
                                {
                                    _switchTimeEnd ??= NeuracleUtils.HubClockToTime(hubClock);
                                    //Console.WriteLine($"_switchTimeStart:{_switchTimeStart},_switchTimeEnd:{_switchTimeEnd},时间:{(_switchTimeEnd - _switchTimeStart) / 1000 * 32}");
                                    //退出这个模式
                                    DeviceManager.GetDevice(_switchDeviceName).Subscribe(x =>
                                    {
                                        var device = x.GetDeviceContext(typeof(SwitchDevice));
                                        device.WriteRegister(SwitchDevice.SwitchStart, 0);
                                        //Console.WriteLine($"退出采集刺激切换模式");
                                    });
                                }
                            }
                        }
                        //采集模式取前64个通道数据
                        if (NeuracleGlobalState.HubStates[NeuracleGlobalState.DeviceNameToHubName[_dataDeviceName]] == HubState.Data)
                        {
                            if (++sampleIndex >= bufferSize)
                            {
                                var digitalMat = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, NeuracleData.AmplifierChannelCount, Depth.S32);
                                Mat analogMat = new(digitalMat.Rows, digitalMat.Cols, Depth.F32, digitalMat.Channels);
                                CV.ConvertScale(digitalMat, analogMat, DataScale);
                                observer.OnNext(new NeuracleMeaDataFrame(_dataDeviceName, clockBuffer, hubClockBuffer, analogMat,
                                                                         NeuracleGlobalState.ImpedanceChannelIndex, float.PositiveInfinity, float.PositiveInfinity,
                                                                         _switchTimeStart, _switchTimeEnd));
                                sampleIndex = 0;
                            }
                        }
                        //刺激模式不用做什么
                        else
                        {
                            sampleIndex = 0;
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .SubscribeSafe(frameObserver);
            }));
    }
}
