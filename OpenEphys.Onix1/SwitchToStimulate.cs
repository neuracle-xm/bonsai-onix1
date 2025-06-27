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
        public double Ch1PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh1)]
        public double Ch1InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh1)]
        public double Ch1PhaseTwoCurrent { get; set; }

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
        public double Ch2PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh2)]
        public double Ch2InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh2)]
        public double Ch2PhaseTwoCurrent { get; set; }

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
        public double Ch3PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh3)]
        public double Ch3InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh3)]
        public double Ch3PhaseTwoCurrent { get; set; }

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
        public double Ch4PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh4)]
        public double Ch4InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh4)]
        public double Ch4PhaseTwoCurrent { get; set; }

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
        /// 根据通道号选择寄存器
        /// </summary>
        /// <param name="channel"></param>
        public static uint SelectRegisterAddressWithChannel(uint channel)
        {
            if (channel < 4)
            {
                return SwitchDevice.SwitchDac32_63;
            }
            else if (channel < 8)
            {
                return SwitchDevice.SwitchDac0_31;
            }
            else if (channel < 12)
            {
                return SwitchDevice.SwitchDac96_127;
            }
            else if (channel < 16)
            {
                return SwitchDevice.SwitchDac64_95;
            }
            else if (channel < 20)
            {
                return SwitchDevice.SwitchDac160_191;
            }
            else if (channel < 24)
            {
                return SwitchDevice.SwitchDac128_159;
            }
            else if (channel < 28)
            {
                return SwitchDevice.SwitchDac224_255;
            }
            else if (channel < 32)
            {
                return SwitchDevice.SwitchDac192_223;
            }
            else if (channel < 36)
            {
                return SwitchDevice.SwitchDac288_319;
            }
            else if (channel < 40)
            {
                return SwitchDevice.SwitchDac256_287;
            }
            else if (channel < 44)
            {
                return SwitchDevice.SwitchDac352_383;
            }
            else if (channel < 48)
            {
                return SwitchDevice.SwitchDac320_351;
            }
            else if (channel < 52)
            {
                return SwitchDevice.SwitchDac416_447;
            }
            else if (channel < 56)
            {
                return SwitchDevice.SwitchDac384_415;
            }
            else if (channel < 60)
            {
                return SwitchDevice.SwitchDac480_511;
            }
            else if (channel < 64)
            {
                return SwitchDevice.SwitchDac448_479;
            }
            else
            {
                return 100;
            }
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
                        GlobalState.HubState = HubState.Stimulation;
                        var d1 = DeviceManager.GetDevice(SwitchDeviceName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(SwitchDevice));
                            device.WriteRegister(SwitchDevice.SwitchCref, 0);
                            device.CloseAllAdc();
                            if (Ch1Enable)
                            {
                                device.WriteRegister(SelectRegisterAddressWithChannel(Ch1StimulateChannel), 0b10000000_00000000_00000000_0000000);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1BURSTCNT, Ch1BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1BURSTINTERVAL, Ch1InterBurstInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PHASEINTERVAL, Ch1InterPhaseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PULSEINTERVAL, Ch1InterPulseInterval);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1CURRENT1, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1CURRENT2, 10);
                                device.WriteRegister(Headstage64ElectricalStimulator.CHIPULSEDUR1, Ch1PhaseOneDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PULSEDUR2, Ch1PhaseTwoDuration);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1TRAINDELAY, Ch1TriggerDelay);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1TRAINCNT, Ch1TrainBurstCount);
                            }
                            if (Ch2Enable)
                            {
                                device.WriteRegister(SelectRegisterAddressWithChannel(Ch2StimulateChannel), 0b01000000_00000000_00000000_0000000);
                            }
                            if (Ch3Enable)
                            {
                                device.WriteRegister(SelectRegisterAddressWithChannel(Ch3StimulateChannel), 0b00100000_00000000_00000000_0000000);
                            }
                            if (Ch4Enable)
                            {
                                device.WriteRegister(SelectRegisterAddressWithChannel(Ch4StimulateChannel), 0b00010000_00000000_00000000_0000000);
                            }
                            device.Start();
                        });

                        var d2 = DeviceManager.GetDevice(StimulationDevice).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                            // 将设置的寄存器值写入设备
                            device.WriteRegister(Headstage64ElectricalStimulator.CHANNEL_ENABLE, 0x1111);
                            device.WriteRegister(Headstage64ElectricalStimulator.RESISTOR_MODE, 0);
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
