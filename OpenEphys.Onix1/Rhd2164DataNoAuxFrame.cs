using System;
using System.Runtime.InteropServices;
using OpenCV.Net;
using OpenEphys.Onix1;

namespace NeuracleExtension;

/// <summary>
/// 只包含amplifier数据的Rhd2164DataFrame，不包含aux数据。
/// </summary>
public class NeuracleHubDataFrame : BufferedDataFrame
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="deviceName">采集数据的DeviceName。</param>
    /// <param name="clock">时钟数组。</param>
    /// <param name="hubClock">hub时钟计数值数组。</param>
    /// <param name="amplifierData">Rhd2164多通道电生理数据。</param>
    /// <param name="r1">阻抗R1</param>
    /// <param name="r2">阻抗R2</param>
    public NeuracleHubDataFrame(string deviceName, ulong[] clock, ulong[] hubClock, Mat amplifierData, float r1, float r2) : base(clock, hubClock)
    {
        DeviceName = deviceName;
        AmplifierData = amplifierData;
        ImpedanceValue = new Tuple<float, float>(r1, r2);
    }

    /// <summary>
    /// 采集数据的DeviceName
    /// </summary>
    public string DeviceName { get; }

    /// <summary>
    /// 获取缓冲的电生理数据数组。
    /// </summary>
    /// <remarks>
    /// 电生理样本以64xN矩阵组织，行表示通道号，N列表示采样点。
    /// </remarks>
    public Mat AmplifierData { get; }

    /// <summary>
    /// 阻抗
    /// </summary>
    public Tuple<float, float> ImpedanceValue { get; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
unsafe struct NeuracleHubDataPayload
{
    public ulong HubClock;
    public fixed int AmplifierData[NeuracleData.AmplifierChannelCount];
    public fixed int ImpedanceData[NeuracleData.ImpedanceChannelCount];
}
