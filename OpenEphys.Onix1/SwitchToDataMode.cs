using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1;

[Description("切换成采集模式")]
public class SwitchToData : Sink<bool>
{
    /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
    [TypeConverter(typeof(SwitchDevice.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string DeviceName { get; set; }

    /// <summary>
    /// Start an electrical stimulus sequence.
    /// </summary>
    /// <param name="source">A sequence of boolean values indicating the start of a stimulus sequence when true.</param>
    /// <returns>A sequence of boolean values that is identical to <paramref name="source"/></returns>
    public override IObservable<bool> Process(IObservable<bool> source)
    {
        return DeviceManager.GetDevice(DeviceName).SelectMany(
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
                        GlobalState.HubState = HubState.Data;
                        device.WriteRegister(1, value ? 1u : 0u);
                        observer.OnNext(value);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(triggerObserver);
            }));
    }
}

