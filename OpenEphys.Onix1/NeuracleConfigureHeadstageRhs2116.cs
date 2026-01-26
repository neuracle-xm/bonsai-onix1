using System.Collections.Generic;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("配置Neuracle的HeadstageRhs2116")]
public class NeuracleConfigureHeadstageRhs2116 : MultiDeviceFactory
{
    [Category(DevicesCategory)]
    [TypeConverter(typeof(SingleDeviceFactoryConverter))]
    [Description("NeuracleHeadstageDataRhs2116的配置")]
    public NeuracleConfigureHeadstageDataRhs2116 HeadstageDataRhs2116 { get; set; } = new();

    [Category(DevicesCategory)]
    [TypeConverter(typeof(SingleDeviceFactoryConverter))]
    [Description("NeuracleHeadstageAdcRhs2116的配置")]
    public NeuracleConfigureHeadstageAdcRhs2116 HeadstageAdcRhs2116 { get; set; } = new();

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
            HeadstageDataRhs2116.DeviceAddress = (uint)value;
            HeadstageAdcRhs2116.DeviceAddress = (uint)value + 1;
        }
    }

    internal override IEnumerable<IDeviceConfiguration> GetDevices()
    {
        //这个遍历顺序不要改动
        yield return HeadstageDataRhs2116;
        yield return HeadstageAdcRhs2116;
    }
}

