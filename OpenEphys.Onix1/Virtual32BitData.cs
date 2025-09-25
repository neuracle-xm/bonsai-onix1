using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net; // Using Mat for data structure

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="Virtual32BitDataFrame"/> objects with data from a virtual 32-bit device.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureVirtual32Bit"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of Virtual32BitDataFrame objects from a virtual 32-bit device.")]
    public class Virtual32BitData : Source<Virtual32BitDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Virtual32Bit.NameConverter))] // Use the new NameConverter
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the number of samples collected for each channel used to create a single <see cref="Virtual32BitDataFrame"/>.
        /// </summary>
        [Description("The number of samples per channel per data frame.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize { get; set; } = 30; // Default buffer size

        /// <summary>
        /// Generates a sequence of <see cref="Virtual32BitDataFrame"/> objects.
        /// </summary>
        /// <returns>A sequence of <see cref="Virtual32BitDataFrame"/> objects.</returns>
        public unsafe override IObservable<Virtual32BitDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<Virtual32BitDataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var device = deviceInfo.GetDeviceContext(typeof(Virtual32Bit)); // Use the virtual device type

                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    // Buffers using 'int' for 32-bit signed data
                    var amplifierBuffer = new int[Virtual32Bit.AmplifierChannelCount * bufferSize];
                    var auxBuffer = new int[Virtual32Bit.AuxChannelCount * bufferSize];
                    var bno055Buffer = new int[Virtual32Bit.Bno055ChannelCount * bufferSize];
                    var ts4231Buffer = new int[Virtual32Bit.TS4231ChannelCount * bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            // Cast payload to the 32-bit structure
                            var payload = (Virtual32BitPayload*)frame.Data.ToPointer();

                            // Marshal data from the payload to the buffers
                            // Ensure the source IntPtr points correctly within the payload struct
                            Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, sampleIndex * Virtual32Bit.AmplifierChannelCount, Virtual32Bit.AmplifierChannelCount);
                            Marshal.Copy(new IntPtr(payload->AuxData), auxBuffer, sampleIndex * Virtual32Bit.AuxChannelCount, Virtual32Bit.AuxChannelCount);
                            Marshal.Copy(new IntPtr(payload->Bno055Data), bno055Buffer, sampleIndex * Virtual32Bit.Bno055ChannelCount, Virtual32Bit.Bno055ChannelCount);
                            Marshal.Copy(new IntPtr(payload->TS4231Data), ts4231Buffer, sampleIndex * Virtual32Bit.TS4231ChannelCount, Virtual32Bit.TS4231ChannelCount);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;

                            if (++sampleIndex >= bufferSize)
                            {
                                // Copy and transpose buffers into Mat objects with Depth.S32
                                var amplifierData = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, Virtual32Bit.AmplifierChannelCount, Depth.S32);
                                var auxData = BufferHelper.CopyTranspose(auxBuffer, bufferSize, Virtual32Bit.AuxChannelCount, Depth.S32);
                                var bno055Data = BufferHelper.CopyTranspose(bno055Buffer, bufferSize, Virtual32Bit.Bno055ChannelCount, Depth.S32);
                                var ts4231Data = BufferHelper.CopyTranspose(ts4231Buffer, bufferSize, Virtual32Bit.TS4231ChannelCount, Depth.S32);

                                // Create and publish the 32-bit data frame
                                observer.OnNext(new Virtual32BitDataFrame(clockBuffer, hubClockBuffer, amplifierData, auxData, bno055Data, ts4231Data));

                                // Reset buffers for the next frame
                                hubClockBuffer = new ulong[bufferSize];
                                clockBuffer = new ulong[bufferSize];
                                // amplifierBuffer and auxBuffer are overwritten by Marshal.Copy, no need to reallocate unless clearing is desired.
                                sampleIndex = 0;
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);

                    // Subscribe to frames from the specified device address
                    return deviceInfo.Context
                        .GetDeviceFrames(device.Address)
                        .SubscribeSafe(frameObserver);
                }));
        }
    }
}
