using System.Runtime.InteropServices;
using OpenCV.Net;
using OpenEphys.Onix1;

namespace NeuracleExtension;

public class NeuracleHeadstageDataFrame : DataFrame
{
    public NeuracleHeadstageDataFrame(ulong clock, ulong hubClock, Mat amplifierData, Mat auxData, Bno055DataFrame bno055DataFrame,
                                      TS4231V1DataFrame ts4231V1DataFrame1, TS4231V1DataFrame ts4231V1DataFrame2,
                                      TS4231V1DataFrame ts4231V1DataFrame3, TS4231V1DataFrame ts4231V1DataFrame4) : base(clock, hubClock)
    {
        AmplifierData = amplifierData;
        AuxData = auxData;
        Bno055DataFrame = bno055DataFrame;
        TS4231V1DataFrame1 = ts4231V1DataFrame1;
        TS4231V1DataFrame2 = ts4231V1DataFrame2;
        TS4231V1DataFrame3 = ts4231V1DataFrame3;
        TS4231V1DataFrame4 = ts4231V1DataFrame4;
    }

    public Mat AmplifierData { get; set; }
    public Mat AuxData { get; set; }

    public Bno055DataFrame Bno055DataFrame { get; set; }
    // 有4个TS4231传感器
    public TS4231V1DataFrame TS4231V1DataFrame1 { get; set; }
    public TS4231V1DataFrame TS4231V1DataFrame2 { get; set; }
    public TS4231V1DataFrame TS4231V1DataFrame3 { get; set; }
    public TS4231V1DataFrame TS4231V1DataFrame4 { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
unsafe struct NeuracleHeadstageDataPayload
{
    public ulong HubClock;
    public fixed int AmplifierData[NeuracleHeadstageData.AmplifierChannelCount];
    public fixed int AuxData[NeuracleHeadstageData.AuxChannelCount];
    public fixed int Bno055Data[NeuracleHeadstageData.Bno055ChannelCount];
    public fixed int TS4231Data[NeuracleHeadstageData.TS4231ChannelCount];
}
