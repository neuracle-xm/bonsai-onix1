// 条件编译，在Release下会跳过
#if DEBUG
using System;
using System.ComponentModel;
using System.Threading;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Neuracle电压控制配置,内部使用")]
public class NeuracleConfigureVoltageControl : SingleDeviceFactory
{

    /// <summary>
    /// 最终下发的数值,这样可以直接在UI上修改
    /// </summary>
    [Description("Data寄存器具体下发的数值")]
    public uint DataValue { get; set; } = 0x81F6;

    /// <summary>
    /// 写寄存器之间的延迟，单位ms
    /// </summary>
    private const int _delay = 100;

    public NeuracleConfigureVoltageControl() : base(typeof(NeuracleVoltageControl))
    {
        DeviceAddress = 0x1100;
        //这个名字无所谓
        DeviceName = "Internal Voltage Control";
    }

    public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
    {
        var deviceName = DeviceName;
        var deviceAddress = DeviceAddress;
        return source.ConfigureDevice(context =>
        {
            var device = context.GetDeviceContext(deviceAddress, DeviceType);
            //直接在这就写寄存器
            device.WriteRegister(NeuracleVoltageControl.Data, DataValue);
            Thread.Sleep(_delay);
            device.WriteRegister(NeuracleVoltageControl.Update, 0);
            Thread.Sleep(_delay);
            device.WriteRegister(NeuracleVoltageControl.Update, 1);
            Thread.Sleep(_delay);
            device.WriteRegister(NeuracleVoltageControl.Update, 0);
            return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
        });
    }
}

static class NeuracleVoltageControl
{
    public const int ID = 0;

    /// <summary>
    /// 写入数据的寄存器
    /// </summary>
    public const uint Data = 0;

    /// <summary>
    /// 更新上面Data中的数值到硬件中
    /// 操作时0-1-0，数据写到硬件中
    /// </summary>
    public const uint Update = 1;

    //下面这些是可以写到Data中的具体数值
    //写在这里只是为了方便查阅
    /// <summary>
    /// 掉电
    /// </summary>
    public const uint ShutdownMode = 0xC801;

    /// <summary>
    /// 上电
    /// </summary>
    public const uint NormalMode = 0xC800;

    /// <summary>
    /// 复位
    /// </summary>
    public const uint Reset = 0xB800;

    /// <summary>
    /// Top Scale
    /// </summary>
    public const uint TopScale = 0x9880;

    /// <summary>
    /// copy serial register data to control register
    /// </summary>
    public const uint CopySerialToControl = 0xD003;

    /// <summary>
    /// RDAC1_voltage 0x81XX
    /// 对于不同的硬件，需要通过把XX换成具体数值
    /// 过程是换一个XX的数值，用电压表量一下实际电压，
    /// 几次尝试后应该就能确定最终数值
    /// </summary>
    public const uint RDac1Voltage = 0x8100;

    /// <summary>
    /// RDAC2_voltage 0x83XX
    /// 对于不同的硬件，需要通过把XX换成具体数值
    /// 过程是换一个XX的数值，用电压表量一下实际电压，
    /// 几次尝试后应该就能确定最终数值
    /// </summary>
    public const uint RDac2Voltage = 0x8300;

    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(NeuracleVoltageControl))
        {
        }
    }
}
#endif
