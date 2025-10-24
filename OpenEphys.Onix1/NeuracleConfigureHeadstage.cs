using System.Collections.Generic;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("配置Neuracle的Headstage")]
public class NeuracleConfigureHeadstage : MultiDeviceFactory
{
    [Category(DevicesCategory)]
    [TypeConverter(typeof(SingleDeviceFactoryConverter))]
    [Description("NeuracleHeadstageData的配置")]
    public NeuracleConfigureHeadstageData HeadstageData { get; set; } = new();

    [Category(DevicesCategory)]
    [TypeConverter(typeof(SingleDeviceFactoryConverter))]
    [Description("Neuracle Headstage刺激的配置")]
    public NeuracleConfigureHeadstageStimulator HeadstageStimulator { get; set; } = new();

    private HeadstageName _headstageName = HeadstageName.HeadstageA;
    [Description("这个Headstage的名称")]
    [Category(ConfigurationCategory)]
    public HeadstageName HeadstageName
    {
        get
        {
            return _headstageName;
        }
        set
        {
            _headstageName = value;
            Name = _headstageName.ToString();
            HeadstageData.DeviceAddress = (uint)value;
            HeadstageStimulator.DeviceAddress = (uint)value + 1;
        }
    }

    internal override IEnumerable<IDeviceConfiguration> GetDevices()
    {
        //这个遍历顺序不要改动
        yield return HeadstageData;
        yield return HeadstageStimulator;
    }
}

/// <summary>
/// 区分不同Headstage的枚举
/// </summary>
public enum HeadstageName
{
    [Description("Headstage A")]
    HeadstageA = 0x9900,

    [Description("Headstage B")]
    HeadstageB = 0xAA00,
}
