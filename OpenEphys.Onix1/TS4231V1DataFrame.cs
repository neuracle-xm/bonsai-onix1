using System;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A single synchronization pulse or light sweep from a SteamVR V1 base station.
    /// </summary>
    public class TS4231V1DataFrame : DataFrame
    {
        /// <summary>
        /// 硬件的时钟周期，事先约定好，单位ns
        /// </summary>
        public const double HubClockPeriod = 20;

        /// <summary>
        /// Initializes a new instance of the <see cref="TS4231V1DataFrame"/> class.
        /// </summary>
        /// <param name="frame">An <see cref="oni.Frame"/> produced by a TS4231 device</param>
        /// <param name="hubClockPeriod">The period of the TS4231 devices local clock in Hz</param>
        public unsafe TS4231V1DataFrame(oni.Frame frame, double hubClockPeriod)
            : base(frame.Clock)
        {
            var payload = (TS4231V1Payload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            SensorIndex = payload->SensorIndex;
            EnvelopeWidth = 1e6 * hubClockPeriod * payload->EnvelopeWidth;
            EnvelopeType = payload->EnvelopeType;
        }

        public TS4231V1DataFrame(ulong clock, ulong hubClock, int sensorIndex, Span<Int16> ts4231Buffer) : base(clock)
        {
            HubClock = hubClock;
            SensorIndex = sensorIndex;
            // 高16位和低16位拼起来组成EnvelopeWidth
            var high16Bytes = BitConverter.GetBytes(ts4231Buffer[3]);
            var low16Bytes = BitConverter.GetBytes(ts4231Buffer[4]);
            var bytes = new byte[] { low16Bytes[0], low16Bytes[1], high16Bytes[0], high16Bytes[1] };
            var envelopeWidth = BitConverter.ToInt32(bytes, 0);
            // EnvelopeWidth单位是μs
            EnvelopeWidth = envelopeWidth * HubClockPeriod / 1000.0;
            EnvelopeType = EnvelopeWidthToEnvelopeType(EnvelopeWidth);
        }

        /// <summary>
        /// Gets the index of the TS4231 sensor that produced this data.
        /// </summary>
        public int SensorIndex { get; }

        /// <summary>
        /// Gets the width of the envelope of the modulated optical pulse or sweep in microseconds.
        /// </summary>
        public double EnvelopeWidth { get; }

        /// <summary>
        /// Gets the pulse or sweep classification.
        /// </summary>
        public TS4231V1Envelope EnvelopeType { get; }

        /// <summary>
        /// 根据EnvelopeWidth转换成TS4231V1Envelope这个枚举
        /// </summary>
        /// <param name="envelopeWidth">单位μs</param>
        /// <returns></returns>
        private static TS4231V1Envelope EnvelopeWidthToEnvelopeType(double envelopeWidth)
        {
            TS4231V1Envelope result;
            if (envelopeWidth <= 50.0)
            {
                result = TS4231V1Envelope.Sweep;
            }
            else if (envelopeWidth <= 62.5)
            {
                result = TS4231V1Envelope.J0;
            }
            else if (envelopeWidth <= 72.9)
            {
                result = TS4231V1Envelope.K0;
            }
            else if (envelopeWidth <= 83.3)
            {
                result = TS4231V1Envelope.J1;
            }
            else if (envelopeWidth <= 93.8)
            {
                result = TS4231V1Envelope.K1;
            }
            else if (envelopeWidth <= 104.0)
            {
                result = TS4231V1Envelope.J2;
            }
            else if (envelopeWidth <= 115.0)
            {
                result = TS4231V1Envelope.K2;
            }
            else if (envelopeWidth <= 125.0)
            {
                result = TS4231V1Envelope.J3;
            }
            else if (envelopeWidth <= 135.0)
            {
                result = TS4231V1Envelope.K3;
            }
            else
            {
                result = TS4231V1Envelope.Bad;
            }
            return result;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TS4231V1Payload
    {
        public ulong HubClock;
        public ushort SensorIndex;
        public uint EnvelopeWidth;
        public TS4231V1Envelope EnvelopeType;
    }

    /// <summary>
    /// Specifies the SteamVR V1 base station optical signal classification.
    /// </summary>
    public enum TS4231V1Envelope : short
    {
        /// <summary>
        /// Specifies and invalid optical signal.
        /// </summary>
        Bad = -1,
        /// <summary>
        /// Specifies a synchronization pulse with 50.0 μS &lt; width ≤ 62.5 μS
        /// </summary>
        J0,
        /// <summary>
        /// Specifies a synchronization pulse with 62.5 μS &lt; width ≤ 72.9 μS
        /// </summary>
        K0,
        /// <summary>
        /// Specifies a synchronization pulse with 72.9 μS &lt; width ≤ 83.3 μS
        /// </summary>
        J1,
        /// <summary>
        /// Specifies a synchronization pulse with 83.3 μS &lt; width ≤ 93.8 μS
        /// </summary>
        K1,
        /// <summary>
        /// Specifies a synchronization pulse with 93.8 μS &lt; width ≤ 104 μS
        /// </summary>
        J2,
        /// <summary>
        /// Specifies a synchronization pulse with 104 μS &lt; width ≤ 115 μS
        /// </summary>
        K2,
        /// <summary>
        /// Specifies a synchronization pulse with 115 μS &lt; width ≤ 125 μS
        /// </summary>
        J3,
        /// <summary>
        /// Specifies a synchronization pulse with 125 μS &lt; width ≤ 135 μS
        /// </summary>
        K3,
        /// <summary>
        /// Specifies a light sheet sweep (width ≤ 50  μS)
        /// </summary>
        Sweep,
    }
}
