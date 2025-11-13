using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Headstage切换成阻抗模式")]
public class NeuracleHeadstageImpedanceMode : Sink<bool>
{
    /// <summary>
    /// 采集和阻抗设备的DeviceName
    /// </summary>
    [TypeConverter(typeof(NeuracleHeadstageData.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string DeviceName { get; set; }

    private uint _channelIndex = 0;
    /// <summary>
    /// 选择哪个通道进行阻抗测量
    /// </summary>
    [Description("选择哪个通道查看阻抗")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public uint ChannelIndex
    {
        get { return _channelIndex; }
        set
        {
            //只能选0-31通道
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 31)
            {
                value = 31;
            }
            _channelIndex = value;
            NeuracleHeadstageGlobalState.ImpedanceChannelIndex = value;
        }
    }

    /// <summary>
    /// 写寄存器之间的延迟
    /// </summary>
    private readonly int _delay = 100;

    public override IObservable<bool> Process(IObservable<bool> source)
    {
        return Observable.Create<bool>(observer =>
        {
            var triggerObserver = Observer.Create<bool>(
                value =>
                {
                    if (!value)
                    {
                        return;
                    }
                    NeuracleHeadstageGlobalState.HeadstageState = HeadstageState.Impedance;
                    Task task = Task.Run(() =>
                    {
                        DeviceManager.GetDevice(DeviceName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(NeuracleHeadstageData));
                            //切换到阻抗模式时，先写zcheck_ch和zcheck_mode寄存器，切换到阻抗模式并选择阻抗检测通道，
                            //最后下发soft_rst寄存器，FPGA开始配置阻抗模式并获取对应通道电压值
                            device.WriteRegister(NeuracleHeadstageData.ZCHECK_CH, NeuracleHeadstageGlobalState.ImpedanceChannelIndex);
                            Thread.Sleep(_delay);
                            device.WriteRegister(NeuracleHeadstageData.ZCHECK_MODE, 1);
                            Thread.Sleep(_delay);
                            device.WriteRegister(NeuracleHeadstageData.SOFT_RST, 0);
                            Thread.Sleep(_delay);
                            device.WriteRegister(NeuracleHeadstageData.SOFT_RST, 1);
                            Thread.Sleep(_delay);
                            device.WriteRegister(NeuracleHeadstageData.SOFT_RST, 0);
                        });
                    });
                    task.Wait();
                    var messageBox = new NeuracleMessageBox("Headstage切换到阻抗模式");
                    messageBox.Show();
                    observer.OnNext(value);
                },
                observer.OnError,
                observer.OnCompleted);
            return source.SubscribeSafe(triggerObserver);
        });
    }
}

