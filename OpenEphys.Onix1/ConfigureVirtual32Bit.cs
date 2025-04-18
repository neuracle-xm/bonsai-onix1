// filepath: d:\Bonsai-onix1\OpenEphys.Onix1\ConfigureVirtual32Bit.cs
using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a virtual 32-bit bioamplifier device.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="Virtual32BitData"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a Virtual32Bit device.")]
    public class ConfigureVirtual32Bit : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureVirtual32Bit"/> class.
        /// </summary>
        public ConfigureVirtual32Bit()
            : base(typeof(Virtual32Bit))
        {
            // Set default configurations specific to the virtual device if needed
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, a <see cref="Virtual32BitData"/> instance that is linked to this configuration will produce data.
        /// If set to false, it will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Virtual32Bit device is enabled.")]
        public bool Enable { get; set; } = true;

        // Note: Filter properties from Rhd2164 are kept for structural similarity.
        // Their effect might be virtual or ignored depending on the simulation logic (if any).

        /// <summary>
        /// Gets or sets the cutoff frequency for the virtual digital high-pass filter.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Specifies the cutoff frequency for the virtual digital high-pass filter.")]
        public Rhd2164DspCutoff DspCutoff { get; set; } = Rhd2164DspCutoff.Dsp146mHz; // Using existing enum for structure

        /// <summary>
        /// Gets or sets the low cutoff frequency of the virtual analog bandpass filter.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Specifies the low cutoff frequency of the virtual analog bandpass filter.")]
        public Rhd2164AnalogLowCutoff AnalogLowCutoff { get; set; } = Rhd2164AnalogLowCutoff.Low100mHz; // Using existing enum for structure

        /// <summary>
        /// Gets or sets the high cutoff frequency of the virtual analog bandpass filter.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Specifies the high cutoff frequency of the virtual analog bandpass filter.")]
        public Rhd2164AnalogHighCutoff AnalogHighCutoff { get; set; } = Rhd2164AnalogHighCutoff.High10000Hz; // Using existing enum for structure


        /// <summary>
        /// Configures a Virtual32Bit device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition. For a virtual device, this might involve setting simulation parameters.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to configure a Virtual32Bit device.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                // In a real virtual device, you might configure simulation parameters here.
                // For now, we'll just get the context and register it.
                // Register reads/writes are omitted as their meaning is undefined for this virtual device.
                var device = context.GetDeviceContext(deviceAddress, DeviceType);

                // Example: Set the enable state (might control data generation in Virtual32BitData)
                // This uses a convention similar to Rhd2164's ENABLE register.
                device.WriteRegister(Virtual32Bit.ENABLE, enable ? 1u : 0);

                // Configure other virtual parameters if needed using device.WriteRegister or custom methods

                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    // Static class to hold constants and potentially helper methods for the virtual device
    internal static class Virtual32Bit
    {
        // Assign a unique ID, different from existing devices (e.g., Rhd2164 is 3)
        public const int ID = 100; // Example ID

        // Define constants - adjust channel counts if needed for the virtual device
        public const int AmplifierChannelCount = 64;
        public const int AuxChannelCount = 3;

        // Define virtual registers (optional, but maintains structure)
        public const uint ENABLE = 0x8000; // Enable/disable stream
        public const uint CONFIG_REG1 = 0x00; // Example virtual config register
        // Add other virtual registers as needed

        // Name converter for Bonsai UI dropdowns
        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Virtual32Bit)) // Use the static class type here
            {
            }
        }
    }
}