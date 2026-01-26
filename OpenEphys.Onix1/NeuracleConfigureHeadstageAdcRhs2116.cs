using System;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Neuracle Headstage Adc Rhs2116配置")]
public class NeuracleConfigureHeadstageAdcRhs2116 : SingleDeviceFactory
{
    public NeuracleConfigureHeadstageAdcRhs2116() : base(typeof(NeuracleHeadstageAdcRhs2116))
    {
    }

    public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
    {
        var deviceName = DeviceName;
        var deviceAddress = DeviceAddress;
        return source.ConfigureDevice(context =>
        {
            var device = context.GetDeviceContext(deviceAddress, DeviceType);
            //初始是采集模式
            NeuracleHeadstageGlobalState.HeadstageState[deviceName] = HeadstageState.Data;
            return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
        });
    }
}

static class NeuracleHeadstageAdcRhs2116
{
    public const int ID = 3;

    // NB: could be read from REZ but these are constant
    public const double DacBitDepth = 16;
    public const double AbsMaxMicroAmps = 2500;

    // managed registers
    //操作时0-1-0
    public const uint SOFT_RST = 0;
    //写0时为正常采样模式，写1切换到阻抗模式
    public const uint ZCHECK_MODE = 1;
    //阻抗检测通道选择，当前支持阻抗检测通道为0-31
    public const uint ZCHECK_CH = 2;


    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(NeuracleHeadstageAdcRhs2116))
        {
        }
    }
}

