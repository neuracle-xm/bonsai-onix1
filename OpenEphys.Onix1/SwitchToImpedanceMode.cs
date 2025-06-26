using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1;

[Description("切换成阻抗模式")]
public class SwitchToImpedanceMode : Sink<bool>
{
    /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
    [TypeConverter(typeof(SwitchDevice.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string SwitchDevice { get; set; }

    /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
    [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string StimulationDevice { get; set; }

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
                    GlobalState.HubState = HubState.Impedance;
                    var d1 = DeviceManager.GetDevice(SwitchDevice).Subscribe(x =>
                    {
                        var device = x.GetDeviceContext(typeof(SwitchDevice));
                        device.WriteRegister(123, 0);
                    });
                    var d2 = DeviceManager.GetDevice(StimulationDevice).Subscribe(x =>
                    {
                        var device = x.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                        device.WriteRegister(123, 0);
                    });
                    observer.OnNext(value);
                },
                observer.OnError,
                observer.OnCompleted);
            return source.SubscribeSafe(triggerObserver);
        });
    }
}

