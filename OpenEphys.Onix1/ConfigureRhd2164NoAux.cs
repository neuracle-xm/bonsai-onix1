using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// 配置不包含Aux通道的Intan Rhd2164生物放大器芯片。
    /// </summary>
    [Description("Configures a Rhd2164 device (no Aux channels).")]
    public class ConfigureRhd2164NoAux : SingleDeviceFactory
    {
        public ConfigureRhd2164NoAux()
            : base(typeof(Rhd2164NoAux))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Rhd2164 device is enabled.")]
        public bool Enable { get; set; } = true;

        [Category(ConfigurationCategory)]
        [Description("Specifies the cutoff frequency for the digital (post-ADC) high-pass filter used for amplifier offset removal.")]
        public Rhd2164DspCutoff DspCutoff { get; set; } = Rhd2164DspCutoff.Dsp146mHz;

        [Category(ConfigurationCategory)]
        [Description("Specifies the low cutoff frequency of the analog (pre-ADC) bandpass filter.")]
        public Rhd2164AnalogLowCutoff AnalogLowCutoff { get; set; } = Rhd2164AnalogLowCutoff.Low100mHz;

        [Category(ConfigurationCategory)]
        [Description("Specifies the high cutoff frequency of the analog (pre-ADC) bandpass filter.")]
        public Rhd2164AnalogHighCutoff AnalogHighCutoff { get; set; } = Rhd2164AnalogHighCutoff.High10000Hz;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);

                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class Rhd2164NoAux
    {
        public const int ID = 2;
        public const int AmplifierChannelCount = 64;
        public const int ImpedanceChannelCount = 8;

        public const uint ENABLE = 0x8000;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhd2164NoAux))
            {
            }
        }
    }
}
