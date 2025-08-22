using System;
using System.Collections.Generic;

namespace NeuracleExtension;

public class GlobalState
{
    /// <summary>
    /// 每个头盒的通道数
    /// </summary>
    public const int ChannelNumberPerHub = 64;

    /// <summary>
    /// 默认的帧BufferSize
    /// </summary>
    public const int BufferSize = 3200;

    /// <summary>
    /// 头盒的状态，初始为采集状态
    /// </summary>
    public static Dictionary<HubName, HubState> HubStates { get; set; } = new();

    /// <summary>
    /// 记录每个DeviceName对应的头盒
    /// </summary>
    public static Dictionary<string, HubName> DeviceNameToHubName { get; set; } = new();

    /// <summary>
    /// 记录每个头盒下有哪些Device
    /// 第一个是Rhd2164NoAux
    /// 第二个是ElectricalStimulator
    /// 第三个是SwitchDevice
    /// </summary>
    public static Dictionary<HubName, Tuple<string, string, string>> HubNameToDeviceName { get; set; } = new();

    /// <summary>
    /// 当前查看哪个阻抗通道
    /// </summary>
    public static uint ImpedanceChannelIndex { get; set; } = 0;

    /// <summary>
    /// 是否已经计算完了配对通道的阻抗
    /// </summary>
    public static bool IsPairImpedanceComplete { get; set; }
}
