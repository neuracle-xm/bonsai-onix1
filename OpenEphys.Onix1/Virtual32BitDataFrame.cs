// filepath: d:\Bonsai-onix1\OpenEphys.Onix1\Virtual32BitDataFrame.cs
using System.Runtime.InteropServices;
using OpenCV.Net; // Assuming OpenCV Mat is still desired for data structure

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Represents a frame of data produced by a virtual 32-bit bioamplifier device.
    /// </summary>
    public class Virtual32BitDataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Virtual32BitDataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of device clock counter values.</param>
        /// <param name="hubClock"> An array of hub clock counter values.</param>
        /// <param name="amplifierData">A matrix of virtual 32-bit multi-channel amplifier data.</param>
        /// <param name="auxData">A matrix of virtual 32-bit auxiliary channel data.</param>
        public Virtual32BitDataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData, Mat auxData)
            : base(clock, hubClock)
        {
            // Ensure the input Mats have the correct depth (e.g., S32 for signed 32-bit int)
            // Add checks here if necessary, e.g.:
            // if (amplifierData.Depth != Depth.S32) throw new ArgumentException(...);
            // if (auxData.Depth != Depth.S32) throw new ArgumentException(...);

            AmplifierData = amplifierData;
            AuxData = auxData;
        }

        /// <summary>
        /// Gets the buffered virtual amplifier data array.
        /// </summary>
        /// <remarks>
        /// Data is organized in a Channels x Samples matrix (e.g., 64xN).
        /// Each sample is a signed 32-bit integer (<see cref="int"/>, OpenCV <see cref="Depth.S32"/>).
        /// Define conversion to physical units (e.g., microvolts) if applicable for the virtual device.
        /// Example: Virtual Voltage (µV) = ScaleFactor * (ADC Sample)
        /// </remarks>
        public Mat AmplifierData { get; }

        /// <summary>
        /// Gets the buffered virtual auxiliary data array.
        /// </summary>
        /// <remarks>
        /// Data is organized in a Channels x Samples matrix (e.g., 3xN).
        /// Each sample is a signed 32-bit integer (<see cref="int"/>, OpenCV <see cref="Depth.S32"/>).
        /// Define conversion to physical units (e.g., volts) if applicable.
        /// </remarks>
        public Mat AuxData { get; }
    }

    // Payload structure matching the expected data layout from the hardware/simulation
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct Virtual32BitPayload
    {
        public ulong HubClock;
        // Use 'int' for signed 32-bit data. Use 'float' if 32-bit floating point is needed.
        public fixed int AmplifierData[Virtual32Bit.AmplifierChannelCount];
        public fixed int AuxData[Virtual32Bit.AuxChannelCount];
        // Ensure total size matches the device frame data size
    }
}