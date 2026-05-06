using System;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Neuracle模拟开关配置")]
public class NeuracleSwitchDeviceConfigure : SingleDeviceFactory
{
    public NeuracleSwitchDeviceConfigure() : base(typeof(SwitchDevice))
    {
    }

    public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
    {
        var deviceName = DeviceName;
        var deviceAddress = DeviceAddress;
        return source.ConfigureDevice(context =>
        {
            var device = context.GetDeviceContext(deviceAddress, DeviceType);
            //一开始就默认开始采集
            device.WriteRegister(SwitchDevice.SwitchCref, 1028);
            device.OpenAllAdc();
            device.CloseAllDac();
            device.StartSwitch();
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

    /// <summary>
    /// 新加的四路用于控制DAC的开关，每8位控制一路
    /// 31~24 对应stimd 打开设置为 0b01000000,关闭设置为 0b00010000
    /// 23~16 对应stimc 打开设置为 0b01000000,关闭设置为 0b00010000
    /// 15~8 对应stimb  打开设置为 0b01000000,关闭设置为 0b00010000
    /// 7~0 对应stima   打开设置为 0b01000000,关闭设置为 0b00010000
    /// </summary>
    public const int SwitchNewDac = 22;

    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(SwitchDevice))
        {
        }
    }
}

