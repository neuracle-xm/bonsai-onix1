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

    static class SwitchDevice
    {
        public const int ID = 4;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(SwitchDevice))
            {
            }
        }
    }
}
