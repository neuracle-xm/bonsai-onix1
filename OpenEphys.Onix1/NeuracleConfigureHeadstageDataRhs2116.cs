using System;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("NeuracleHeadstageDataRhs2116的配置")]
public class NeuracleConfigureHeadstageDataRhs2116 : SingleDeviceFactory
{
    public NeuracleConfigureHeadstageDataRhs2116() : base(typeof(NeuracleHeadstageDataRhs2116))
    {

    }

    public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
    {
        var deviceName = DeviceName;
        var deviceAddress = DeviceAddress;
        return source.ConfigureDevice(context =>
        {
            var device = context.GetDeviceContext(deviceAddress, DeviceType);
            return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
        });
    }
}

static class NeuracleHeadstageDataRhs2116
{
    public const int ID = 2;
    public const int AmplifierChannelCount = 32;
    public const int AuxChannelCount = 3;
    public const int Bno055ChannelCount = 14;
    public const int TS4231ChannelCount = 21;

    public const uint ENABLE = 0x8000;

    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(NeuracleHeadstageDataRhs2116))
        {
        }
    }
}

