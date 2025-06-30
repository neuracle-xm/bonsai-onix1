using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// 只包含amplifier数据的Rhd2164Data版本，不包含aux数据。
    /// </summary>
    [Description("只输出amplifier数据的Rhd2164DataFrame，不包含aux数据。")]
    public class Rhd2164DataNoAux : Source<Rhd2164DataFrameNoAux>
    {
        [TypeConverter(typeof(Rhd2164NoAux.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        [Description("每个通道用于创建单个Rhd2164DataFrame的采样数。")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize { get; set; } = 30;

        //[Description("增益。")]
        //[Category(DeviceFactory.ConfigurationCategory)]
        //public int Scale { get; set; } = 1;

        public static (byte low, byte middle, byte high, byte moreHigh) Low24OfInt32(Int32 x)
        {
            //低8位
            var low8 = Convert.ToByte(x & 0xFF);
            //中间8位
            var middle8 = Convert.ToByte(x >> 8 & 0xFF);
            //高8位
            var high8 = Convert.ToByte(x >> 16 & 0xFF);

            var moreHigh8 = Convert.ToByte(x >> 24 & 0xFF);
            return (low8, middle8, high8, moreHigh8);
        }

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
        /// 阻抗检测中的电压增益
        /// </summary>
        public static float VoltageScale = (float)(4096 / Math.Pow(2, 23) * 10 * 11);

        /// <summary>
        /// 阻抗检测中的电流增益
        /// </summary>
        public static float CurrentScale = (float)(4096 / Math.Pow(2, 23) / 200 * 10);

        public unsafe override IObservable<Rhd2164DataFrameNoAux> Generate()
        {
            var bufferSize = BufferSize;
            //var scale = Scale;
            if (DeviceName == "test")
            {
                return Observable.Create<Rhd2164DataFrameNoAux>(observer =>
                {
                    return Observable.Interval(TimeSpan.FromSeconds(1.0 / 200))
                        .Subscribe(_ =>
                        {
                            var ampliferArray = GenerateSquareWave();
                            var hubClockBuffer = new ulong[bufferSize];
                            var clockBuffer = new ulong[bufferSize];
                            observer.OnNext(new Rhd2164DataFrameNoAux(
                                (ulong[])clockBuffer.Clone(),
                                (ulong[])hubClockBuffer.Clone(),
                                Mat.FromArray(ampliferArray),
                                10.3f,
                                12.5f));
                        }, observer.OnError, observer.OnCompleted);
                });
            }
            else
            {
                return DeviceManager.GetDevice(DeviceName).SelectMany(
                    deviceInfo => Observable.Create<Rhd2164DataFrameNoAux>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(Rhd2164NoAux));
                        var hubClockBuffer = new ulong[bufferSize];
                        var clockBuffer = new ulong[bufferSize];
                        var sampleIndex = 0;
                        var amplifierBuffer = new int[Rhd2164NoAux.AmplifierChannelCount * bufferSize];
                        var impedanceBuffer = new int[Rhd2164NoAux.ImpedanceChannelCount * bufferSize];
                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var dataSize = frame.DataSize;
                                var payload = (Rhd2164NoAuxPayload*)frame.Data.ToPointer();
                                hubClockBuffer[sampleIndex] = payload->HubClock;
                                clockBuffer[sampleIndex] = frame.Clock;
                                Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, sampleIndex * Rhd2164NoAux.AmplifierChannelCount, Rhd2164NoAux.AmplifierChannelCount);
                                Marshal.Copy(new IntPtr(payload->ImpedanceData), impedanceBuffer, sampleIndex * Rhd2164NoAux.ImpedanceChannelCount, Rhd2164NoAux.ImpedanceChannelCount);
                                //采集模式取前64个通道数据
                                if (GlobalState.HubStates[GlobalState.DeviceNameToHubName[DeviceName]] == HubState.Data)
                                {
                                    if (++sampleIndex >= bufferSize)
                                    {
                                        var amplifierData = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, Rhd2164NoAux.AmplifierChannelCount, Depth.S32);
                                        observer.OnNext(new Rhd2164DataFrameNoAux(clockBuffer, hubClockBuffer, amplifierData, 0, 0));
                                        sampleIndex = 0;
                                    }
                                }
                                //阻抗取最后8个通道数据用于计算阻抗
                                else if (GlobalState.HubStates[GlobalState.DeviceNameToHubName[DeviceName]] == HubState.Impedance)
                                {
                                    if (++sampleIndex >= bufferSize)
                                    {
                                        long i1_sum = 0;
                                        long v1_sum = 0;
                                        long v2_sum = 0;
                                        for (var i = 0; i < impedanceBuffer.Length; i++)
                                        {
                                            var channelIndex = i % Rhd2164NoAux.ImpedanceChannelCount;
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
                                        var i1_mean = i1_sum * CurrentScale / bufferSize;
                                        var v1_mean = v1_sum * VoltageScale / bufferSize;
                                        var v2_mean = v2_sum * VoltageScale / bufferSize;
                                        float r1 = 0;
                                        float r2 = 0;
                                        if (i1_mean != 0)
                                        {
                                            r1 = (v1_mean - v2_mean) / i1_mean;
                                            r2 = v2_mean / i1_mean;
                                        }
                                        observer.OnNext(new Rhd2164DataFrameNoAux(clockBuffer, hubClockBuffer, Mat.Zeros(Rhd2164NoAux.AmplifierChannelCount, bufferSize, Depth.S32, 1), r1, r2));
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
    }
}
