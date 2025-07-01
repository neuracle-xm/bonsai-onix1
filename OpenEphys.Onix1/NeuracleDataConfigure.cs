using System;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Neuracle数据采集配置")]
public class NeuracleDataConfigure : SingleDeviceFactory
{
    public NeuracleDataConfigure() : base(typeof(NeuracleData))
    {
    }

    //[Category(ConfigurationCategory)]
    //[Description("Specifies the cutoff frequency for the digital (post-ADC) high-pass filter used for amplifier offset removal.")]
    //public Rhd2164DspCutoff DspCutoff { get; set; } = Rhd2164DspCutoff.Dsp146mHz;

    //[Category(ConfigurationCategory)]
    //[Description("Specifies the low cutoff frequency of the analog (pre-ADC) bandpass filter.")]
    //public Rhd2164AnalogLowCutoff AnalogLowCutoff { get; set; } = Rhd2164AnalogLowCutoff.Low100mHz;

    //[Category(ConfigurationCategory)]
    //[Description("Specifies the high cutoff frequency of the analog (pre-ADC) bandpass filter.")]
    //public Rhd2164AnalogHighCutoff AnalogHighCutoff { get; set; } = Rhd2164AnalogHighCutoff.High10000Hz;

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

static class NeuracleData
{
    public const int ID = 2;
    public const int AmplifierChannelCount = 64;
    public const int ImpedanceChannelCount = 8;

    public const uint ENABLE = 0x8000;

    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(NeuracleData))
        {
        }
    }
}

