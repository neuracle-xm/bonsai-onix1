using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a headstage-64 onboard electrical stimulator.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="SwitchToStimulate"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a headstage-64 onboard electrical stimulator.")]
    public class ConfigureSwitchDevice : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstage64ElectricalStimulator"/> class.
        /// </summary>
        public ConfigureSwitchDevice()
            : base(typeof(SwitchDevice))
        {
        }

        /// <summary>
        /// Configure a headstage-64 onboard electrical stimulator.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/>
        /// instance prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration
        /// actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to
        /// configure a headstage-64 onboard electrical stimulator.</returns>f
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                //device.WriteRegister(Headstage64ElectricalStimulator.ENABLE, 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    public static class SwitchDevice
    {
        public const int ID = 4;

        /// <summary>
        /// 下面是模拟开关控制寄存器中的各个地址
        /// 参考模拟开关
        /// </summary>
        public const int SwitchCref = 0;

        /// <summary>
        /// ADC(采样用)
        /// </summary>
        public const int SwitchAdc0_31 = 1;
        public const int SwitchAdc32_63 = 2;
        public const int SwitchAdc64_95 = 3;
        public const int SwitchAdc96_127 = 4;

        /// <summary>
        /// DAC(刺激用)
        /// </summary>
        public const int SwitchDac0_31 = 5;
        public const int SwitchDac32_63 = 6;
        public const int SwitchDac64_95 = 7;
        public const int SwitchDac96_127 = 8;
        public const int SwitchDac128_159 = 9;
        public const int SwitchDac160_191 = 10;
        public const int SwitchDac192_223 = 11;
        public const int SwitchDac224_255 = 12;
        public const int SwitchDac256_287 = 13;
        public const int SwitchDac288_319 = 14;
        public const int SwitchDac320_351 = 15;
        public const int SwitchDac352_383 = 16;
        public const int SwitchDac384_415 = 17;
        public const int SwitchDac416_447 = 18;
        public const int SwitchDac448_479 = 19;
        public const int SwitchDac480_511 = 20;

        /// <summary>
        /// 开始切换
        /// </summary>
        public const int SwitchStart = 21;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(SwitchDevice))
            {
            }
        }
    }
}
