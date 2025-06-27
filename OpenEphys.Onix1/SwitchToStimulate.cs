using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Controls a headstage-64 onboard electrical stimulus sequencer.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureHeadstage64ElectricalStimulator"/>, using a shared <c>DeviceName</c>.
    /// Headstage-64's onboard electrical stimulator can be used to deliver current controlled
    /// micro-stimulation through a contact on the probe connector on the bottom of the headstage or the
    /// corresponding contact on a compatible electrode interface board.
    /// </remarks>
    [Description("Controls a headstage-64 onboard electrical stimulus sequencer V2.")]
    public class SwitchToStimulate : Sink<bool>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(SwitchDevice.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string SwitchDeviceName { get; set; }

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string StimulationDevice { get; set; }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
        /// </remarks>
        [Description("Specifies whether the electrical stimulator will respect triggers.")]
        [Category(DeviceFactory.StimulatorCh1)]
        public bool Ch1Enable { get; set; }

        /// <summary>
        /// 设置刺激通道1 需要进行刺激的通道.
        /// </summary>
        [Description("Ch1刺激通道.")]
        [Range(0, 63)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1StimulateChannel { get; set; }

        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1TriggerDelay { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the first phase of each pulse in μA.
        /// </summary>
        [Description("Amplitude of the first phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1PhaseTwoCurrent { get; set; }

        /// <summary>
        /// Gets or sets the duration of the first phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the first phase of each pulse in μsec.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1PhaseOneDuration { get; set; }

        /// <summary>
        /// Gets or sets the duration of the interphase interval of each pulse in μsec.
        /// </summary>
        [Description("The duration of the interphase interval of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1InterPhaseInterval { get; set; }

        /// <summary>
        /// Gets or sets the duration of the second phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the second phase of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1PhaseTwoDuration { get; set; }

        /// <summary>
        /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
        /// </summary>
        [Description("The duration of the inter-pulse interval within a single burst (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1InterPulseInterval { get; set; }

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1InterBurstInterval { get; set; }

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("The number of pulses per burst.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1BurstPulseCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of bursts in each train.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1TrainBurstCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of RestCurrent.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh1)]
        public uint Ch1RestCurrent { get; set; }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
        /// </remarks>
        [Description("Specifies whether the electrical stimulator will respect triggers.")]
        [Category(DeviceFactory.StimulatorCh2)]
        public bool Ch2Enable { get; set; }

        /// <summary>
        /// 设置刺激通道2 需要进行刺激的通道.
        /// </summary>
        [Description("Ch2刺激通道.")]
        [Range(0, 63)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2StimulateChannel { get; set; }

        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2TriggerDelay { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the first phase of each pulse in μA.
        /// </summary>
        [Description("Amplitude of the first phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2PhaseTwoCurrent { get; set; }

        /// <summary>
        /// Gets or sets the duration of the first phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the first phase of each pulse in μsec.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2PhaseOneDuration { get; set; }

        /// <summary>
        /// Gets or sets the duration of the interphase interval of each pulse in μsec.
        /// </summary>
        [Description("The duration of the interphase interval of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2InterPhaseInterval { get; set; }

        /// <summary>
        /// Gets or sets the duration of the second phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the second phase of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2PhaseTwoDuration { get; set; }
        /// <summary>
        /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
        /// </summary>
        [Description("The duration of the inter-pulse interval within a single burst (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2InterPulseInterval { get; set; }

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2InterBurstInterval { get; set; }

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("The number of pulses per burst.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2BurstPulseCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of bursts in each train.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2TrainBurstCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of RestCurrent.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh2)]
        public uint Ch2RestCurrent { get; set; }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
        /// </remarks>
        [Description("Specifies whether the electrical stimulator will respect triggers.")]
        [Category(DeviceFactory.StimulatorCh3)]
        public bool Ch3Enable { get; set; }

        /// <summary>
        /// 设置刺激通道3 需要进行刺激的通道.
        /// </summary>
        [Description("Ch3刺激通道.")]
        [Range(0, 63)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3StimulateChannel { get; set; }

        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3TriggerDelay { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the first phase of each pulse in μA.
        /// </summary>
        [Description("Amplitude of the first phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3PhaseTwoCurrent { get; set; }

        /// <summary>
        /// Gets or sets the duration of the first phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the first phase of each pulse in μsec.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3PhaseOneDuration { get; set; }

        /// <summary>
        /// Gets or sets the duration of the interphase interval of each pulse in μsec.
        /// </summary>
        [Description("The duration of the interphase interval of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3InterPhaseInterval { get; set; }

        /// <summary>
        /// Gets or sets the duration of the second phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the second phase of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3PhaseTwoDuration { get; set; }
        /// <summary>
        /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
        /// </summary>
        [Description("The duration of the inter-pulse interval within a single burst (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3InterPulseInterval { get; set; }

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3InterBurstInterval { get; set; }

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("The number of pulses per burst.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3BurstPulseCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of bursts in each train.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3TrainBurstCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of RestCurrent.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh3)]
        public uint Ch3RestCurrent { get; set; }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
        /// </remarks>
        [Description("Specifies whether the electrical stimulator will respect triggers.")]
        [Category(DeviceFactory.StimulatorCh4)]
        public bool Ch4Enable { get; set; }

        /// <summary>
        /// 设置刺激通道4 需要进行刺激的通道.
        /// </summary>
        [Description("Ch4刺激通道.")]
        [Range(0, 63)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4StimulateChannel { get; set; }

        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4TriggerDelay { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the first phase of each pulse in μA.
        /// </summary>
        [Description("Amplitude of the first phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4PhaseTwoCurrent { get; set; }

        /// <summary>
        /// Gets or sets the duration of the first phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the first phase of each pulse in μsec.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4PhaseOneDuration { get; set; }

        /// <summary>
        /// Gets or sets the duration of the interphase interval of each pulse in μsec.
        /// </summary>
        [Description("The duration of the interphase interval of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4InterPhaseInterval { get; set; }

        /// <summary>
        /// Gets or sets the duration of the second phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the second phase of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4PhaseTwoDuration { get; set; }
        /// <summary>
        /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
        /// </summary>
        [Description("The duration of the inter-pulse interval within a single burst (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4InterPulseInterval { get; set; }

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4InterBurstInterval { get; set; }

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("The number of pulses per burst.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4BurstPulseCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of bursts in each train.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4TrainBurstCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of RestCurrent.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.StimulatorCh4)]
        public uint Ch4RestCurrent { get; set; }

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
                        GlobalState.HubStates[GlobalState.DeviceNameToHubName[SwitchDeviceName]] = HubState.Stimulation;
                        DeviceManager.GetDevice(SwitchDeviceName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(SwitchDevice));
                            device.WriteRegister(SwitchDevice.SwitchCref, 0);
                            device.CloseAllAdc();
                            if (Ch1Enable)
                            {
                                device.WriteStimulateChannel(Ch1StimulateChannel, 0);
                            }
                            if (Ch2Enable)
                            {
                                device.WriteStimulateChannel(Ch2StimulateChannel, 1);
                            }
                            if (Ch3Enable)
                            {
                                device.WriteStimulateChannel(Ch3StimulateChannel, 2);
                            }
                            if (Ch4Enable)
                            {
                                device.WriteStimulateChannel(Ch4StimulateChannel, 3);
                            }
                            device.StartSwitch();
                        });

                        uint channelEnable = 0;
                        DeviceManager.GetDevice(StimulationDevice).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                            if (Ch1Enable)
                            {
                                channelEnable += 0b0001;
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1BURSTCNT, Ch1BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1BURSTINTERVAL, Ch1InterBurstInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PHASEINTERVAL, Ch1InterPhaseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PULSEINTERVAL, Ch1InterPulseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1CURRENT1, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1CURRENT2, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PULSEDUR1, Ch1PhaseOneDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PULSEDUR2, Ch1PhaseTwoDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1TRAINDELAY, Ch1TriggerDelay);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1TRAINCNT, Ch1TrainBurstCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1RESTCURRENT, Ch1RestCurrent);
                            }
                            if (Ch2Enable)
                            {
                                channelEnable += 0b0010;
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2BURSTCNT, Ch2BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2BURSTINTERVAL, Ch2InterBurstInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2PHASEINTERVAL, Ch2InterPhaseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2PULSEINTERVAL, Ch2InterPulseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2CURRENT1, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2CURRENT2, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2PULSEDUR1, Ch2PhaseOneDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2PULSEDUR2, Ch2PhaseTwoDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2TRAINDELAY, Ch2TriggerDelay);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2TRAINCNT, Ch2TrainBurstCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2RESTCURRENT, Ch2RestCurrent);

                            }
                            if (Ch3Enable)
                            {
                                channelEnable += 0b0100;
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3BURSTCNT, Ch3BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3BURSTINTERVAL, Ch3InterBurstInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3PHASEINTERVAL, Ch3InterPhaseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3PULSEINTERVAL, Ch3InterPulseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3CURRENT1, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3CURRENT2, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3PULSEDUR1, Ch3PhaseOneDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3PULSEDUR2, Ch3PhaseTwoDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3TRAINDELAY, Ch3TriggerDelay);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3TRAINCNT, Ch3TrainBurstCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3RESTCURRENT, Ch3RestCurrent);

                            }
                            if (Ch4Enable)
                            {
                                channelEnable += 0b1000;
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4BURSTCNT, Ch4BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4BURSTINTERVAL, Ch4InterBurstInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4PHASEINTERVAL, Ch4InterPhaseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4PULSEINTERVAL, Ch4InterPulseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4CURRENT1, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4CURRENT2, 10); // 要转化
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4PULSEDUR1, Ch4PhaseOneDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4PULSEDUR2, Ch4PhaseTwoDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4TRAINDELAY, Ch4TriggerDelay);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4TRAINCNT, Ch4TrainBurstCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4RESTCURRENT, Ch4RestCurrent);

                            }

                            // 将设置的寄存器值写入设备
                            device.WriteRegister(Headstage64ElectricalStimulator.CHANNEL_ENABLE, channelEnable);  // 根据每个Enabel
                            device.WriteRegister(Headstage64ElectricalStimulator.RESISTOR_MODE, 0);
                            device.StartStimulate();
                        });
                        observer.OnNext(value);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(triggerObserver);
            });
        }
    }
}
