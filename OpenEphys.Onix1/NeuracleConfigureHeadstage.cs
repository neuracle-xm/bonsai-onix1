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
    public NeuracleConfigureHeadstageData HeadstageData { get; set; } = new()
    {
        // 下位机固定地址，先写死
        DeviceAddress = 39168
    };

    internal override IEnumerable<IDeviceConfiguration> GetDevices()
    {
        yield return HeadstageData;
    }
}
