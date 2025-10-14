using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Headstage切换成采集模式")]
public class NeuracleHeadstageDataMode : Sink<bool>
{
    /// <summary>
    /// 采集和阻抗设备的DeviceName
    /// </summary>
    [TypeConverter(typeof(NeuracleHeadstageData.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string DeviceName { get; set; }

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
                        NeuracleHeadstageGlobalState.HeadstageState = HeadstageState.Data;
                        DeviceManager.GetDevice(DeviceName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(NeuracleHeadstageData));
                            //由阻抗模式切换到采样模式时，将zcheck_mode寄存器写0，zcheck_ch清空，并下发soft_rst寄存器
                            device.WriteRegister(NeuracleHeadstageData.ZCHECK_MODE, 0);
                            device.WriteRegister(NeuracleHeadstageData.ZCHECK_CH, 0);
                            device.WriteRegister(NeuracleHeadstageData.SOFT_RST, 0);
                            device.WriteRegister(NeuracleHeadstageData.SOFT_RST, 1);
                            device.WriteRegister(NeuracleHeadstageData.SOFT_RST, 0);
                            var messageBox = new NeuracleMessageBox("Headstage切换到采集模式");
                            messageBox.Show();
                        });
                        observer.OnNext(value);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(triggerObserver);
            });
    }
}

