using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1;

[Description("停止刺激")]
public class StopStimulate : Sink<bool>
{
    /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
    [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
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
                var device = deviceInfo.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                var triggerObserver = Observer.Create<bool>(
                    value =>
                    {
                        if (!value)
                        {
                            return;
                        }
                        device.WriteRegister(1, value ? 1u : 0u);
                        observer.OnNext(value);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(triggerObserver);
            }));
    }
}
