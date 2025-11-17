using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("配置Neuracle的头盒")]
public class NeuracleConfigureHub : MultiDeviceFactory
{
    [Category(DevicesCategory)]
    [TypeConverter(typeof(SingleDeviceFactoryConverter))]
    [Description("NeuracleData的配置")]
    public NeuracleDataConfigure Data { get; set; } = new();

    [Category(DevicesCategory)]
    [TypeConverter(typeof(SingleDeviceFactoryConverter))]
    [Description("Neuracle刺激器的配置")]
    public NeuracleElectricalStimulatorConfigure ElectricalStimulator { get; set; } = new();

    [Category(DevicesCategory)]
    [TypeConverter(typeof(SingleDeviceFactoryConverter))]
    [Description("Neuracle模拟开关的配置")]
    public NeuracleSwitchDeviceConfigure SwitchDevice { get; set; } = new();

    /// <summary>
    /// 闭环反馈模式专用，不要放到UI上
    /// </summary>
    private readonly NeuracleFeedbackDeviceConfigure _feedbackDeviceConfigure = new();

    private HubName _hub = HubName.HubA;
    [Description("这个头盒的名称")]
    [Category(ConfigurationCategory)]
    public HubName Hub
    {
        get
        {
            return _hub;
        }
        set
        {
            _hub = value;
            Name = Hub.ToString();
            Data.DeviceAddress = (uint)value;
            ElectricalStimulator.DeviceAddress = (uint)value + 1;
            SwitchDevice.DeviceAddress = (uint)value + 2;
            _feedbackDeviceConfigure.DeviceAddress = (uint)value + 3;
            //每个头盒初始都是采集模式
            if (!NeuracleGlobalState.HubStates.ContainsKey(_hub))
            {
                NeuracleGlobalState.HubStates[_hub] = HubState.Data;
            }
            //把每个设备的DeviceName关联到HubName
            if (!NeuracleGlobalState.DeviceNameToHubName.ContainsKey(Data.DeviceName))
            {
                NeuracleGlobalState.DeviceNameToHubName[Data.DeviceName] = _hub;
            }
            if (!NeuracleGlobalState.DeviceNameToHubName.ContainsKey(ElectricalStimulator.DeviceName))
            {
                NeuracleGlobalState.DeviceNameToHubName[ElectricalStimulator.DeviceName] = _hub;
            }
            if (!NeuracleGlobalState.DeviceNameToHubName.ContainsKey(SwitchDevice.DeviceName))
            {
                NeuracleGlobalState.DeviceNameToHubName[SwitchDevice.DeviceName] = _hub;
            }
            //记录当前hub下有哪些Device
            if (!NeuracleGlobalState.HubNameToDeviceName.ContainsKey(_hub))
            {
                NeuracleGlobalState.HubNameToDeviceName[_hub] = Tuple.Create(Data.DeviceName, ElectricalStimulator.DeviceName, SwitchDevice.DeviceName);
            }
        }
    }

    internal override IEnumerable<IDeviceConfiguration> GetDevices()
    {
        yield return Data;
        yield return ElectricalStimulator;
        yield return SwitchDevice;
        yield return _feedbackDeviceConfigure;
    }
}

/// <summary>
/// 区分不同头盒的枚举
/// </summary>
public enum HubName
{
    /// <summary>
    /// Specifies Hub A.
    /// </summary>
    [Description("Hub A")]
    HubA = 0x1100,

    /// <summary>
    /// Specifies Hub B.
    /// </summary>
    [Description("Hub B")]
    HubB = 0x2200,

    /// <summary>
    /// Specifies Hub B.
    /// </summary>
    [Description("Hub C")]
    HubC = 0x3300,

    /// <summary>
    /// Specifies Hub B.
    /// </summary>
    [Description("Hub D")]
    HubD = 0x4400,

    /// <summary>
    /// Specifies Hub B.
    /// </summary>
    [Description("Hub E")]
    HubE = 0x5500,

    /// <summary>
    /// Specifies Hub B.
    /// </summary>
    [Description("Hub F")]
    HubF = 0x6600,

    /// <summary>
    /// Specifies Hub B.
    /// </summary>
    [Description("Hub G")]
    HubG = 0x7700,

    /// <summary>
    /// Specifies Hub B.
    /// </summary>
    [Description("Hub H")]
    HubH = 0x8800,
}
