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
    // 下位机固定地址
    //DeviceAddress = 39168
    public NeuracleConfigureHeadstageData HeadstageData { get; set; } = new();

    [Category(DevicesCategory)]
    [TypeConverter(typeof(SingleDeviceFactoryConverter))]
    [Description("Neuracle Headstage刺激的配置")]
    // 下位机固定地址
    //DeviceAddress = 39169
    public NeuracleConfigureHeadstageStimulator HeadstageStimulator { get; set; } = new();

    internal override IEnumerable<IDeviceConfiguration> GetDevices()
    {
        yield return HeadstageData;
        yield return HeadstageStimulator;
    }
}
