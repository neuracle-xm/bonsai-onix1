using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Bonsai;
using oni;
using System.Drawing.Design;


namespace OpenEphys.Onix1
{
    [Combinator]
    [Description("")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class StimulatorTrigger
    {
        readonly BehaviorSubject<bool> enable = new(true);
        readonly BehaviorSubject<double> phaseOneCurrent = new(0);
        readonly BehaviorSubject<double> interPhaseCurrent = new(0);
        readonly BehaviorSubject<double> phaseTwoCurrent = new(0);
        readonly BehaviorSubject<uint> phaseOneDuration = new(0);
        readonly BehaviorSubject<uint> interPhaseInterval = new(0);
        readonly BehaviorSubject<uint> phaseTwoDuration = new(0);
        readonly BehaviorSubject<uint> interPulseInterval = new(0);
        readonly BehaviorSubject<uint> burstPulseCount = new(0);
        readonly BehaviorSubject<uint> interBurstInterval = new(0);
        readonly BehaviorSubject<uint> trainBurstCount = new(0);
        readonly BehaviorSubject<uint> triggerDelay = new(0);
        readonly BehaviorSubject<bool> powerEnable = new(false);

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
        /// </remarks>
        [Description("Specifies whether the electrical stimulator will respect triggers.")]
        [Category(DeviceFactory.AcquisitionCategory)]
        public bool Enable
        {
            get => enable.Value;
            set => enable.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the electrical stimulator's power state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator's ±15V power supplies will be turned on. If set to false,
        /// they will be turned off. It may be desirable to power down the electrical stimulator's power supplies outside
        /// of stimulation windows to reduce power consumption and electrical noise. This property must be set to true
        /// in order for electrical stimuli to be delivered properly. It takes ~10 milliseconds for these supplies to stabilize.
        /// </remarks>
        [Description("Stimulator power on/off.")]
        [Category(DeviceFactory.AcquisitionCategory)]
        public bool PowerEnable
        {
            get => powerEnable.Value;
            set => powerEnable.OnNext(value);
        }

        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint TriggerDelay
        {
            get => triggerDelay.Value;
            set => triggerDelay.OnNext(value);
        }


        /// <summary>
        /// Gets or sets the amplitude of the first phase of each pulse in μA.
        /// </summary>
        [Description("Amplitude of the first phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double PhaseOneCurrent
        {
            get => phaseOneCurrent.Value;
            set => phaseOneCurrent.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double InterPhaseCurrent
        {
            get => interPhaseCurrent.Value;
            set => interPhaseCurrent.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double PhaseTwoCurrent
        {
            get => phaseTwoCurrent.Value;
            set => phaseTwoCurrent.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the first phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the first phase of each pulse in μsec.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint PhaseOneDuration
        {
            get => phaseOneDuration.Value;
            set => phaseOneDuration.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the interphase interval of each pulse in μsec.
        /// </summary>
        [Description("The duration of the interphase interval of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint InterPhaseInterval
        {
            get => interPhaseInterval.Value;
            set => interPhaseInterval.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the second phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the second phase of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint PhaseTwoDuration
        {
            get => phaseTwoDuration.Value;
            set => phaseTwoDuration.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
        /// </summary>
        [Description("The duration of the inter-pulse interval within a single burst (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint InterPulseInterval
        {
            get => interPulseInterval.Value;
            set => interPulseInterval.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint InterBurstInterval
        {
            get => interBurstInterval.Value;
            set => interBurstInterval.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("The number of pulses per burst.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint BurstPulseCount
        {
            get => burstPulseCount.Value;
            set => burstPulseCount.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of bursts in each train.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint TrainBurstCount
        {
            get => trainBurstCount.Value;
            set => trainBurstCount.OnNext(value);
        }

        public IObservable<bool> Process(IObservable<bool> source)
        {
            //return source.Do(value => {
            //    DeviceManager.GetDevice(DeviceName).SelectMany(
            //    deviceInfo => Observable.Create<bool>(observer =>
            //    {
            //        var device = deviceInfo.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
            //        var triggerObserver = Observer.Create<bool>(
            //            value =>
            //            {
            //                device.WriteRegister(Headstage64ElectricalStimulator.TRIGGER, value ? 1u : 0u);
            //                observer.OnNext(value);
            //            },
            //            observer.OnError,
            //            observer.OnCompleted);

            //        static uint uAToCode(double currentuA)
            //        {
            //            var k = 1 / (2 * Headstage64ElectricalStimulator.AbsMaxMicroAmps / (Math.Pow(2, Headstage64ElectricalStimulator.DacBitDepth) - 1)); // static
            //            return (uint)(k * (currentuA + Headstage64ElectricalStimulator.AbsMaxMicroAmps));
            //        }

            //        return new CompositeDisposable(
            //            enable.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.ENABLE, value ? 1u : 0u)),
            //            phaseOneCurrent.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.CURRENT1, uAToCode(value))),
            //            interPhaseCurrent.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.RESTCURR, uAToCode(value))),
            //            phaseTwoCurrent.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.CURRENT2, uAToCode(value))),
            //            triggerDelay.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.TRAINDELAY, value)),
            //            phaseOneDuration.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.PULSEDUR1, value)),
            //            interPhaseInterval.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.INTERPHASEINTERVAL, value)),
            //            phaseTwoDuration.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.PULSEDUR2, value)),
            //            interPulseInterval.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.INTERPULSEINTERVAL, value)),
            //            interBurstInterval.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.INTERBURSTINTERVAL, value)),
            //            burstPulseCount.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.BURSTCOUNT, value)),
            //            trainBurstCount.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.TRAINCOUNT, value)),
            //            powerEnable.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.POWERON, value ? 1u : 0u)),
            //            source.SubscribeSafe(triggerObserver)
            //        );
            //    }));
            //});
            return Observable.Create<bool>(observer =>
            {

                var triggerObserver = Observer.Create<bool>(
                        value =>
                        {
                            Console.WriteLine("write register");
                            observer.OnNext(value);
                        },
                        observer.OnError,
                        observer.OnCompleted);
                return new CompositeDisposable(
                   enable.SubscribeSafe(observer, value => Console.WriteLine("write register1")),
                    phaseOneCurrent.SubscribeSafe(observer, value => Console.WriteLine("write register2")),
                    interPhaseCurrent.SubscribeSafe(observer, value => Console.WriteLine("write register3")),
                    phaseTwoCurrent.SubscribeSafe(observer, value => Console.WriteLine("write register4")),
                    triggerDelay.SubscribeSafe(observer, value => Console.WriteLine("write register5")),
                    phaseOneDuration.SubscribeSafe(observer, value => Console.WriteLine("write register6")),
                    interPhaseInterval.SubscribeSafe(observer, value => Console.WriteLine("write register7")),
                    phaseTwoDuration.SubscribeSafe(observer, value => Console.WriteLine("write register8")),
                    interPulseInterval.SubscribeSafe(observer, value => Console.WriteLine("write register9")),
                    interBurstInterval.SubscribeSafe(observer, value => Console.WriteLine("write register10")),
                    burstPulseCount.SubscribeSafe(observer, value => Console.WriteLine("write register11")),
                    trainBurstCount.SubscribeSafe(observer, value =>
                    {
                        Console.WriteLine("write register12");
                        }
                    ),
                    powerEnable.SubscribeSafe(observer, value => Console.WriteLine("write register13")),
                    source.SubscribeSafe(triggerObserver)
                );
            });
        }
    }
}
