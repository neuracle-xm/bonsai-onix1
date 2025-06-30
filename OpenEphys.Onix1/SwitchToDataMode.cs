using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using static OpenEphys.Onix1.ConfigureHeadstage64NoAux;

namespace OpenEphys.Onix1;

[Description("切换成采集模式")]
public class SwitchToData : Sink<bool>
{
    private HubName _hubName;
    [Description("选择的头盒")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public HubName HubName
    {
        get
        {
            return _hubName;
        }
        set
        {
            _hubName = value;
            _switchDeviceName = GlobalState.HubNameToDeviceName[_hubName].Item3;
        }
    }

    /// <summary>
    /// 模拟开关的DeviceName
    /// </summary>
    private string _switchDeviceName;

    /// <summary>
    /// Start an electrical stimulus sequence.
    /// </summary>
    /// <param name="source">A sequence of boolean values indicating the start of a stimulus sequence when true.</param>
    /// <returns>A sequence of boolean values that is identical to <paramref name="source"/></returns>
    public override IObservable<bool> Process(IObservable<bool> source)
    {
        return DeviceManager.GetDevice(_switchDeviceName).SelectMany(
            deviceInfo => Observable.Create<bool>(observer =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(SwitchDevice));
                var triggerObserver = Observer.Create<bool>(
                    value =>
                    {
                        if (!value)
                        {
                            return;
                        }
                        GlobalState.HubStates[GlobalState.DeviceNameToHubName[_switchDeviceName]] = HubState.Data;
                        //这些是测试用的
                        //device.TestCref1();
                        //device.TestCref2();
                        //device.TestStima();
                        //device.TestCref1_Stima();
                        //device.TestCref2_Stima();
                        //device.TestChannel0();
                        //device.TestChannel1();
                        //device.TestCloseAll();
                        //device.TestOpenAll();
                        //device.TestChannel2Stima();
                        //device.TestChannel2Stimb();
                        //device.TestChannel2Stimc();
                        //device.TestChannel2Stimd();
                        //device.TestChannel9Stima();
                        //device.TestChannel9Stimb();
                        //device.TestChannel9Stimc();
                        //device.TestChannel9Stimd();
                        //下面的是正式切换到采集流程
                        device.WriteRegister(SwitchDevice.SwitchCref, 4);
                        device.OpenAllAdc();
                        device.CloseAllDac();
                        device.StartSwitch();
                        observer.OnNext(value);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(triggerObserver);
            }));
    }
}

