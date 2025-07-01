using System;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Neuracle刺激配置")]
public class NeuracleElectricalStimulatorConfigure : SingleDeviceFactory
{
    public NeuracleElectricalStimulatorConfigure() : base(typeof(ElectricalStimulator))
    {
    }

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

static class ElectricalStimulator
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

    /// <summary>
    /// 刺激通道2参数配置
    /// </summary>
    public const uint CH2PULSEDUR1 = 11;
    public const uint CH2PULSEDUR2 = 12;
    public const uint CH2PHASEINTERVAL = 13;
    public const uint CH2PULSEINTERVAL = 14;
    public const uint CH2BURSTCNT = 15;
    public const uint CH2BURSTINTERVAL = 16;
    public const uint CH2CURRENT1 = 17;
    public const uint CH2CURRENT2 = 18;
    public const uint CH2RESTCURRENT = 19;
    public const uint CH2TRAINCNT = 20;
    public const uint CH2TRAINDELAY = 21;

    /// <summary>
    /// 刺激通道3参数配置
    /// </summary>
    public const uint CH3PULSEDUR1 = 22;
    public const uint CH3PULSEDUR2 = 23;
    public const uint CH3PHASEINTERVAL = 24;
    public const uint CH3PULSEINTERVAL = 25;
    public const uint CH3BURSTCNT = 26;
    public const uint CH3BURSTINTERVAL = 27;
    public const uint CH3CURRENT1 = 28;
    public const uint CH3CURRENT2 = 29;
    public const uint CH3RESTCURRENT = 30;
    public const uint CH3TRAINCNT = 31;
    public const uint CH3TRAINDELAY = 32;

    /// <summary>
    /// 刺激通道4参数配置
    /// </summary>
    public const uint CH4PULSEDUR1 = 33;
    public const uint CH4PULSEDUR2 = 34;
    public const uint CH4PHASEINTERVAL = 35;
    public const uint CH4PULSEINTERVAL = 36;
    public const uint CH4BURSTCNT = 37;
    public const uint CH4BURSTINTERVAL = 38;
    public const uint CH4CURRENT1 = 39;
    public const uint CH4CURRENT2 = 40;
    public const uint CH4RESTCURRENT = 41;
    public const uint CH4TRAINCNT = 42;
    public const uint CH4TRAINDELAY = 43;

    public const uint CHANNEL_ENABLE = 44; // 通道使能  0011代表使能了前2通道，从低位开始
    public const uint STIM_START = 45; // 开始刺激，需要先置0再置1 再置0
    public const uint RESISTOR_MODE = 46; //阻抗模式寄存器，执行阻抗检测时，此值置1，刺激时此值置0 查看阻抗时不需要管上面那个Stim_start，永远写0；也不用管Channel_enable，永远写0

    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(ElectricalStimulator))
        {
        }
    }
}

