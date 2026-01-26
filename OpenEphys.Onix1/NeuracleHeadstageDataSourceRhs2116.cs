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

[Description("输出NeuracleHeadstageDataRhs2116的数据")]
public class NeuracleHeadstageDataSourceRhs2116 : Source<NeuracleHeadstageDataFrame>
{
    [TypeConverter(typeof(NeuracleHeadstageDataRhs2116.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string DeviceName { get; set; }

    [TypeConverter(typeof(NeuracleHeadstageAdcRhs2116.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string AdDeviceName { get; set; }

    /// <summary>
    /// 采集模式电压转换系数，原始值乘这个值
    /// </summary>
    public const float VoltageCoefficient = 0.0006f;

    [Description("缓存的帧大小")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public int BufferSize { get; set; } = 3000;

    public unsafe override IObservable<NeuracleHeadstageDataFrame> Generate()
    {
        var bufferSize = BufferSize;
        return DeviceManager.GetDevice(DeviceName).SelectMany(
            deviceInfo => Observable.Create<NeuracleHeadstageDataFrame>(observer =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(NeuracleHeadstageDataRhs2116));
                //采集模式要累计满一定的帧
                var dataSampleIndex = 0;
                var rawAmplifierBuffer = new int[NeuracleHeadstageDataRhs2116.AmplifierChannelCount * bufferSize];
                var amplifierBuffer = new float[NeuracleHeadstageDataRhs2116.AmplifierChannelCount * bufferSize];
                var auxBuffer = new int[NeuracleHeadstageDataRhs2116.AuxChannelCount];
                //这个是原始32位的数组
                var rawBno055Buffer = new int[NeuracleHeadstageDataRhs2116.Bno055ChannelCount];
                var rawTS4231Buffer = new int[NeuracleHeadstageDataRhs2116.TS4231ChannelCount];
                //按照取低16位复制到最终使用的数组中
                var bno055Buffer = new Int16[NeuracleHeadstageDataRhs2116.Bno055ChannelCount];
                var ts4231Buffer = new Int16[NeuracleHeadstageDataRhs2116.TS4231ChannelCount];
                //阻抗模式下要累计一定点数后算一次阻抗
                var impedanceSampleIndex = 0;
                var rawImpedanceBuffer = new int[NeuracleHeadstageDataRhs2116.AmplifierChannelCount];
                var impedanceBuffer = new int[NeuracleHeadstageGlobalState.ImpedanceBufferSize];
                var frameObserver = Observer.Create<oni.Frame>(
                    frame =>
                    {
                        var payload = (NeuracleHeadstageDataPayload*)frame.Data.ToPointer();
                        ulong clock = frame.Clock;
                        ulong hubClock = payload->HubClock;
                        Marshal.Copy(new IntPtr(payload->AuxData), auxBuffer, 0, NeuracleHeadstageDataRhs2116.AuxChannelCount);
                        Marshal.Copy(new IntPtr(payload->Bno055Data), rawBno055Buffer, 0, NeuracleHeadstageDataRhs2116.Bno055ChannelCount);
                        Marshal.Copy(new IntPtr(payload->TS4231Data), rawTS4231Buffer, 0, NeuracleHeadstageDataRhs2116.TS4231ChannelCount);
                        for (int i = 0; i < NeuracleHeadstageDataRhs2116.Bno055ChannelCount; i++)
                        {
                            bno055Buffer[i] = NeuracleUtils.GetInt32LowInt16(rawBno055Buffer[i]);
                        }
                        var bno055DataFrame = new Bno055DataFrame(clock, hubClock, bno055Buffer);
                        for (int i = 0; i < NeuracleHeadstageDataRhs2116.TS4231ChannelCount; i++)
                        {
                            ts4231Buffer[i] = NeuracleUtils.GetInt32LowInt16(rawTS4231Buffer[i]);
                        }
                        // 从所有的TS4231通道中分成4个组
                        Span<Int16> group1 = ts4231Buffer.AsSpan(1, 5);
                        var ts4231V1DataFrame1 = new TS4231V1DataFrame(clock, hubClock, 1, group1);
                        Span<Int16> group2 = ts4231Buffer.AsSpan(6, 5);
                        var ts4231V1DataFrame2 = new TS4231V1DataFrame(clock, hubClock, 2, group2);
                        Span<Int16> group3 = ts4231Buffer.AsSpan(11, 5);
                        var ts4231V1DataFrame3 = new TS4231V1DataFrame(clock, hubClock, 3, group3);
                        Span<Int16> group4 = ts4231Buffer.AsSpan(16, 5);
                        var ts4231V1DataFrame4 = new TS4231V1DataFrame(clock, hubClock, 4, group4);
                        var auxData = BufferHelper.CopyTranspose(auxBuffer, 1, NeuracleHeadstageDataRhs2116.AuxChannelCount, Depth.S32);
                        if (NeuracleHeadstageGlobalState.HeadstageState[AdDeviceName] == HeadstageState.Data)
                        {
                            //换成采集模式时清掉阻抗数组累计的值
                            impedanceSampleIndex = 0;
                            Marshal.Copy(new IntPtr(payload->AmplifierData), rawAmplifierBuffer, dataSampleIndex * NeuracleHeadstageDataRhs2116.AmplifierChannelCount, NeuracleHeadstageDataRhs2116.AmplifierChannelCount);
                            if (++dataSampleIndex >= bufferSize)
                            {
                                //转换成实际的电压值，单位mV
                                for (int i = 0; i < rawAmplifierBuffer.Length; i++)
                                {
                                    amplifierBuffer[i] = rawAmplifierBuffer[i] * VoltageCoefficient;
                                }
                                var amplifierData = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, NeuracleHeadstageDataRhs2116.AmplifierChannelCount, Depth.F32);
                                observer.OnNext(new NeuracleHeadstageDataFrame(DeviceName, clock, hubClock, amplifierData, NeuracleHeadstageGlobalState.ImpedanceChannelIndex, float.PositiveInfinity, auxData, bno055DataFrame, ts4231V1DataFrame1, ts4231V1DataFrame2, ts4231V1DataFrame3, ts4231V1DataFrame4));
                                dataSampleIndex = 0;
                            }
                        }
                        else if (NeuracleHeadstageGlobalState.HeadstageState[AdDeviceName] == HeadstageState.Impedance)
                        {
                            dataSampleIndex = 0;
                            //找到当前测的通道返回的电压值
                            Marshal.Copy(new IntPtr(payload->AmplifierData), rawImpedanceBuffer, 0, NeuracleHeadstageDataRhs2116.AmplifierChannelCount);
                            var currentImpedanceChannelVoltage = rawImpedanceBuffer[NeuracleHeadstageGlobalState.ImpedanceChannelIndex];
                            impedanceBuffer[impedanceSampleIndex] = currentImpedanceChannelVoltage;
                            if (++impedanceSampleIndex >= NeuracleHeadstageGlobalState.ImpedanceBufferSize)
                            {
                                var bandPassFilterdArray = NeuracleHeadstageGlobalState.FirFilt(impedanceBuffer, NeuracleHeadstageGlobalState.bandPassCoefficient);
                                var impedanceValue = NeuracleHeadstageGlobalState.ComputeImpedanceRhs2116(bandPassFilterdArray);
                                observer.OnNext(new NeuracleHeadstageDataFrame(DeviceName, clock, hubClock, Mat.Zeros(NeuracleHeadstageDataRhs2116.AmplifierChannelCount, 1, Depth.F32, 1), NeuracleHeadstageGlobalState.ImpedanceChannelIndex, impedanceValue, auxData, bno055DataFrame, ts4231V1DataFrame1, ts4231V1DataFrame2, ts4231V1DataFrame3, ts4231V1DataFrame4));
                                impedanceSampleIndex = 0;
                            }
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
