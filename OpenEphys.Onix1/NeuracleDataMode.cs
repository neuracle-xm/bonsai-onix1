using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("切换成采集模式")]
public class NeuracleDataMode : Sink<bool>
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
            if (NeuracleGlobalState.HubNameToDeviceName.TryGetValue(_hubName, out var deviceTuple))
            {
                _switchDeviceName = deviceTuple.Item3;
            }
        }
    }

    /// <summary>
    /// 模拟开关的DeviceName
    /// </summary>
    private string _switchDeviceName;

    /// <summary>
    /// 切换到采集模式
    /// </summary>
    /// <param name="switchDeviceName"></param>
    public static void DataProcedure(string switchDeviceName)
    {
        NeuracleGlobalState.HubStates[NeuracleGlobalState.DeviceNameToHubName[switchDeviceName]] = HubState.Data;
        DeviceManager.GetDevice(switchDeviceName).Subscribe(deviceInfo =>
        {
            var switchDevice = deviceInfo.GetDeviceContext(typeof(SwitchDevice));
            switchDevice.WriteRegister(SwitchDevice.SwitchCref, 1028);
            switchDevice.OpenAllAdc();
            switchDevice.CloseAllDac();
            switchDevice.StartSwitch();
        });
    }

    /// <summary>
    /// Start an electrical stimulus sequence.
    /// </summary>
    /// <param name="source">A sequence of boolean values indicating the start of a stimulus sequence when true.</param>
    /// <returns>A sequence of boolean values that is identical to <paramref name="source"/></returns>
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
                        DataProcedure(_switchDeviceName);
                        var messageBox = new NeuracleMessageBox("切换到采集模式");
                        messageBox.Show();
                        observer.OnNext(value);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(triggerObserver);
            });
    }
}

