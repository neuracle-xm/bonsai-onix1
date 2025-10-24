using System;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Neuracle Headstage刺激配置")]
public class NeuracleConfigureHeadstageStimulator : SingleDeviceFactory
{
    public NeuracleConfigureHeadstageStimulator() : base(typeof(HeadstageStimulator))
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

static class HeadstageStimulator
{
    public const int ID = 3;

    // NB: could be read from REZ but these are constant
    public const double DacBitDepth = 16;
    public const double AbsMaxMicroAmps = 2500;

    // managed registers
    /// <summary>
    /// 刺激通道1参数配置
    /// </summary>
    public const uint CH1PULSEDUR1 = 0;  // 单位us, 比如UI上填了10，那就相当于下发了10us(物理有个最小限制1us)
    public const uint CH1PULSEDUR2 = 1;
    public const uint CH1PHASEINTERVAL = 2;
    public const uint CH1PULSEINTERVAL = 3;
    public const uint CH1BURSTCNT = 4;
    public const uint CH1BURSTINTERVAL = 5;
    public const uint CH1CURRENT1 = 6;
    public const uint CH1CURRENT2 = 7;
    public const uint CH1RESTCURRENT = 8;
    public const uint CH1TRAINCNT = 9;
    public const uint CH1TRAINDELAY = 10;
    //模拟开关控制，0断开，1闭合
    public const uint STIM_NULL = 11;
    //上电使能，0断电，1上电
    public const uint POWER_EN = 12;
    //开始刺激，每次开始刺激时先将此值清0，再置1，再置0
    public const uint STIM_START = 15;

    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(HeadstageStimulator))
        {
        }
    }
}

