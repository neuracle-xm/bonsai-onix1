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

[Description("输出Neuracle头盒的数据")]
public class NeuracleHubData : Source<NeuracleHubDataFrame>
{
    [TypeConverter(typeof(NeuracleData.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string DeviceName { get; set; }

    [Description("缓存的帧大小")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public int BufferSize { get; set; } = NeuracleGlobalState.BufferSize;

    /// <summary>
    /// 生成测试用方波
    /// </summary>
    /// <returns></returns>
    private float[,] GenerateSquareWave()
    {
        var middleIndex = BufferSize / 2;
        var result = new float[Rhd2164.AmplifierChannelCount, BufferSize];
        for (int row = 0; row < Rhd2164.AmplifierChannelCount; row++)
        {
            for (int col = 0; col < middleIndex; col++)
            {
                result[row, col] = row * 5;
            }
            for (int col = middleIndex; col < BufferSize; col++)
            {
                result[row, col] = -row * 5;
            }
        }
        return result;
    }

    /// <summary>
    /// 采集数据的增益,单位mV
    /// </summary>
    public static float DataScale = (float)(4096 / Math.Pow(2, 23) / 12);

    /// <summary>
    /// 阻抗检测中的电压增益
    /// </summary>
    public static float VoltageScale = (float)(4096 / Math.Pow(2, 23) * 11);

    /// <summary>
    /// 配对通道的r1阻抗
    /// </summary>
    private float _pairR1 = float.PositiveInfinity;

    /// <summary>
    /// 配对通道的r2阻抗
    /// </summary>
    private float _pairR2 = float.PositiveInfinity;

    public unsafe override IObservable<NeuracleHubDataFrame> Generate()
    {
        var bufferSize = BufferSize;
        return DeviceManager.GetDevice(DeviceName).SelectMany(
            deviceInfo => Observable.Create<NeuracleHubDataFrame>(observer =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(NeuracleData));
                //这个就相当于时间戳
                var hubClockBuffer = new ulong[bufferSize];
                var clockBuffer = new ulong[bufferSize];
                var sampleIndex = 0;
                var amplifierBuffer = new int[NeuracleData.AmplifierChannelCount * bufferSize];
                var impedanceBuffer = new int[NeuracleData.ImpedanceChannelCount * bufferSize];
                var frameObserver = Observer.Create<oni.Frame>(
                    frame =>
                    {
                        var payload = (NeuracleHubDataPayload*)frame.Data.ToPointer();
                        hubClockBuffer[sampleIndex] = NeuracleUtils.HubClockToTime(payload->HubClock);
                        clockBuffer[sampleIndex] = frame.Clock;
                        Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, sampleIndex * NeuracleData.AmplifierChannelCount, NeuracleData.AmplifierChannelCount);
                        Marshal.Copy(new IntPtr(payload->ImpedanceData), impedanceBuffer, sampleIndex * NeuracleData.ImpedanceChannelCount, NeuracleData.ImpedanceChannelCount);
                        //采集模式取前64个通道数据
                        if (NeuracleGlobalState.HubStates[NeuracleGlobalState.DeviceNameToHubName[DeviceName]] == HubState.Data)
                        {
                            if (++sampleIndex >= bufferSize)
                            {
                                var digitalMat = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, NeuracleData.AmplifierChannelCount, Depth.S32);
                                Mat analogMat = new(digitalMat.Rows, digitalMat.Cols, Depth.F32, digitalMat.Channels);
                                CV.ConvertScale(digitalMat, analogMat, DataScale);
                                observer.OnNext(new NeuracleHubDataFrame(DeviceName, clockBuffer, hubClockBuffer, analogMat, NeuracleGlobalState.ImpedanceChannelIndex, float.PositiveInfinity, float.PositiveInfinity));
                                sampleIndex = 0;
                            }
                        }
                        //阻抗取最后8个通道数据用于计算阻抗
                        else if (NeuracleGlobalState.HubStates[NeuracleGlobalState.DeviceNameToHubName[DeviceName]] == HubState.Impedance)
                        {
                            if (++sampleIndex >= bufferSize)
                            {
                                long i1_sum = 0;
                                long v1_sum = 0;
                                long v2_sum = 0;
                                for (var i = 0; i < impedanceBuffer.Length; i++)
                                {
                                    var channelIndex = i % NeuracleData.ImpedanceChannelCount;
                                    int value = impedanceBuffer[i];
                                    //通道1输出电流l1
                                    if (channelIndex == 0)
                                    {
                                        i1_sum += value;
                                    }
                                    //通道2输出电压V1
                                    else if (channelIndex == 1)
                                    {
                                        v1_sum += value;
                                    }
                                    //通道4输出电压V2
                                    else if (channelIndex == 3)
                                    {
                                        v2_sum += value;
                                    }
                                }
                                // 最终交付时用这一行
                                var i1_mean = (float)(i1_sum / bufferSize * 4096 / Math.Pow(2, 23) - 1024) / 200;
                                // 测试第一版硬件时用
                                //var i1_mean = (float)(i1_sum / bufferSize * 4096 / Math.Pow(2, 23)) / 200;
                                float r1 = float.PositiveInfinity;
                                float r2 = float.PositiveInfinity;
                                //现在阻抗模式下发的电流是0.61mA，只有接收到的电流值大于30%才认为是有阻抗的
                                //if (i1_mean > 0.183)
                                //{
                                var v1_mean = v1_sum * VoltageScale / bufferSize;
                                var v2_mean = v2_sum * VoltageScale / bufferSize;
                                r1 = (v1_mean - v2_mean) / i1_mean;
                                r2 = v2_mean / i1_mean;
                                //}
                                //还没计算过配对阻抗
                                if (!NeuracleGlobalState.IsPairImpedanceComplete)
                                {
                                    _pairR1 = r1;
                                    _pairR2 = r2;
                                    NeuracleGlobalState.IsPairImpedanceComplete = true;
                                    //这时候还没测真正的阻抗，就直接显示为无穷
                                    observer.OnNext(new NeuracleHubDataFrame(DeviceName, clockBuffer, hubClockBuffer, Mat.Zeros(NeuracleData.AmplifierChannelCount, bufferSize, Depth.F32, 1), NeuracleGlobalState.ImpedanceChannelIndex, float.PositiveInfinity, float.PositiveInfinity));
                                }
                                //已经算过配对通道的阻抗了，那就先看这些阻抗是不是无穷大
                                else
                                {
                                    //如果之前算出来的配对通道的阻抗是无穷大，那真正需要计算的通道的阻抗就直接视为无穷大
                                    //PairR1和PairR2只可能同时为无穷
                                    if (_pairR1 == float.PositiveInfinity)
                                    {
                                        r1 = float.PositiveInfinity;
                                        r2 = float.PositiveInfinity;
                                    }
                                    observer.OnNext(new NeuracleHubDataFrame(DeviceName, clockBuffer, hubClockBuffer, Mat.Zeros(NeuracleData.AmplifierChannelCount, bufferSize, Depth.F32, 1), NeuracleGlobalState.ImpedanceChannelIndex, r1, r2));
                                }
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
