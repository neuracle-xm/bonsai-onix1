using System;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("NeuracleHeadstageData的配置")]
public class NeuracleConfigureHeadstageData : SingleDeviceFactory
{
    public NeuracleConfigureHeadstageData() : base(typeof(NeuracleHeadstageData))
    {

    }

    public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
    {
        var deviceName = DeviceName;
        return source.ConfigureDevice(context =>
        {
            uint? deviceAddress = context.GetAddressByID(NeuracleHeadstageData.ID) ?? throw new Exception("没有找到NeuracleHeadstageData对应的地址");
            DeviceAddress = deviceAddress.Value;
            var device = context.GetDeviceContext(deviceAddress.Value, DeviceType);
            return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
        });
    }
}

static class NeuracleHeadstageData
{
    public const int ID = 2;
    public const int AmplifierChannelCount = 32;
    public const int AuxChannelCount = 3;
    public const int Bno055ChannelCount = 14;
    public const int TS4231ChannelCount = 21;

    public const uint ENABLE = 0x8000;

    // managed registers
    //操作时0-1-0
    public const uint SOFT_RST = 20;
    //写0时为正常采样模式，写1切换到阻抗模式
    public const uint ZCHECK_MODE = 21;
    //阻抗检测通道选择，当前支持阻抗检测通道为0-31
    public const uint ZCHECK_CH = 22;

    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(NeuracleHeadstageData))
        {
        }
    }
}

