using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Bonsai;
using static OpenEphys.Onix1.ConfigureHeadstage64NoAux;

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
                _stimulationDeviceName = GlobalState.HubNameToDeviceName[_hubName].Item2;
                _switchDeviceName = GlobalState.HubNameToDeviceName[_hubName].Item3;
            }
        }

        /// <summary>
        /// 模拟开关的DeviceName
        /// </summary>
        private string _switchDeviceName;

        /// <summary>
        /// 刺激设备的DeviceName
        /// </summary>
        private string _stimulationDeviceName;

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
        public int Ch1PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh1)]
        public int Ch1InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh1)]
        public int Ch1PhaseTwoCurrent { get; set; }

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
        /// Gets or set Ch1BiPhasic.
        /// </summary>
        [Description("Ch1BiPhasic.")]
        [Category(DeviceFactory.StimulatorCh1)]
        public bool Ch1BiPhasic { get; set; }

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
        public int Ch2PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh2)]
        public int Ch2InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh2)]
        public int Ch2PhaseTwoCurrent { get; set; }

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
        /// Gets or set Ch2BiPhasic.
        /// </summary>
        [Description("Ch2BiPhasic.")]
        [Category(DeviceFactory.StimulatorCh2)]
        public bool Ch2BiPhasic { get; set; }

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
        public int Ch3PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh3)]
        public int Ch3InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh3)]
        public int Ch3PhaseTwoCurrent { get; set; }

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
        /// Gets or set Ch3BiPhasic.
        /// </summary>
        [Description("Ch3BiPhasic.")]
        [Category(DeviceFactory.StimulatorCh3)]
        public bool Ch3BiPhasic { get; set; }

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
        public int Ch4PhaseOneCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh4)]
        public int Ch4InterPhaseCurrent { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.StimulatorCh4)]
        public int Ch4PhaseTwoCurrent { get; set; }

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
        /// Gets or set Ch4BiPhasic.
        /// </summary>
        [Description("Ch1BiPhasic.")]
        [Category(DeviceFactory.StimulatorCh4)]
        public bool Ch4BiPhasic { get; set; }

        /// <summary>
        /// 计算单个刺激通道的刺激时间
        /// </summary>
        /// <param name="trainDelay"></param>
        /// <param name="trainCnt"></param>
        /// <param name="burstCnt"></param>
        /// <param name="interPulseInterval"></param>
        /// <param name="interBurstInterval"></param>
        /// <param name="pulseDur1"></param>
        /// <param name="pulseDur2"></param>
        /// <param name="interPhaseInterval"></param>
        /// <param name="biPhasic"></param>
        /// <returns></returns>
        private int CalStimulationDuration(uint trainDelay, uint trainCnt, uint burstCnt, uint interPulseInterval, uint interBurstInterval, uint pulseDur1, uint pulseDur2, uint interPhaseInterval)
        {
            int CalPulseDuration(uint pulseDur1, uint pulseDur2, uint interPhaseInterval)
            {
                return (int)(pulseDur1 + interPhaseInterval + pulseDur2);
            }

            int CalBurstDuration(uint burstCnt, uint interPulseInterval, uint pulseDur1, uint pulseDur2, uint interPhaseInterval)
            {
                return (int)(burstCnt * CalPulseDuration(pulseDur1, pulseDur2, interPhaseInterval) + (burstCnt - 1) * interPulseInterval);
            }

            return (int)(CalBurstDuration(burstCnt, interPulseInterval, pulseDur1, pulseDur2, interPhaseInterval) * trainCnt + (trainCnt - 1) * interBurstInterval + trainDelay);
        }
        /// <summary>
        /// 将参数转换成适配本设备的参数
        /// 如果是单向波将对应的参数设置为0或1
        /// </summary>
        /// <param name="biPhasic"></param>
        /// <param name="phaseTwoCurrent"></param>
        /// <param name="interPhaseCurrent"></param>
        /// <param name="phaseTwoDuration"></param>
        /// <param name="interPhaseInterval"></param>
        /// <param name="interPulseInterval"></param>
        /// <returns></returns>
        private Tuple<int, int, uint, uint, uint> ChangeParamByBiPhasic(bool biPhasic, int phaseTwoCurrent, int interPhaseCurrent, uint phaseTwoDuration, uint interPhaseInterval, uint interPulseInterval)
        {
            // 单向波参数适配
            if (!biPhasic)
            {
                phaseTwoCurrent = 0;
                interPhaseCurrent = 0;
                phaseTwoDuration = 1;
                interPhaseInterval = 1;
                interPulseInterval -= 2;
            }
            return Tuple.Create(phaseTwoCurrent, interPhaseCurrent, phaseTwoDuration, interPhaseInterval, interPulseInterval);
        }

        /// <summary>
        /// 验证刺激参数的有效性,将小于1的参数设置为1
        /// </summary>
        /// <param name="phaseOneDuration"></param>
        /// <param name="phaseTwoDuration"></param>
        /// <param name="interPhaseInterval"></param>
        /// <param name="interPulseInterval"></param>
        /// <param name="interBurstInterval"></param>
        /// <param name="triggerDelay"></param>
        /// <returns></returns>
        private Tuple<uint, uint, uint, uint, uint, uint> ValidateDuration(uint phaseOneDuration, uint phaseTwoDuration, uint interPhaseInterval, uint interPulseInterval, uint interBurstInterval, uint triggerDelay)
        {
            uint phaseOneDurationValidated = Math.Max(phaseOneDuration, 1);
            uint phaseTwoDurationValidated = Math.Max(phaseTwoDuration, 1);
            uint interPhaseIntervalValidated = Math.Max(interPhaseInterval, 1);
            uint interPulseIntervalValidated = Math.Max(interPulseInterval, 1);
            uint interBurstIntervalValidated = Math.Max(interBurstInterval, 1);
            uint triggerDelayValidated = Math.Max(triggerDelay, 1);
            return Tuple.Create(phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated, interPulseIntervalValidated, interBurstIntervalValidated, triggerDelayValidated);
        }

        /// <summary>
        /// 将微安转换为设备值
        /// </summary>
        /// <param name="CurrentUA"></param>
        /// <returns></returns>
        private uint ConvertUAToDeviceValue(int CurrentUA)
        {
            if (CurrentUA > 0)
            {
                return (uint)(CurrentUA / 4000.0f * 32767);
            }
            return (uint)((4000 + CurrentUA) * 32767.0f / 4000 + 32768);
        }

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
                        GlobalState.HubStates[GlobalState.DeviceNameToHubName[_switchDeviceName]] = HubState.Stimulation;
                        DeviceManager.GetDevice(_switchDeviceName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(SwitchDevice));
                            device.WriteRegister(SwitchDevice.SwitchCref, 0);
                            device.CloseAllAdc();
                            //把所有使用的通道分成几個組
                            Dictionary<uint, List<uint>> addressWithChannels = new();
                            if (Ch1Enable)
                            {
                                var chAddress = SwitchWriteRegisterFunctions.SelectRegisterAddressWithChannel(Ch1StimulateChannel);
                                if (!addressWithChannels.ContainsKey(chAddress))
                                {
                                    addressWithChannels[chAddress] = new List<uint>();
                                }
                                addressWithChannels[chAddress].Add(Ch1StimulateChannel);
                            }
                            if (Ch2Enable)
                            {
                                var chAddress = SwitchWriteRegisterFunctions.SelectRegisterAddressWithChannel(Ch2StimulateChannel);
                                if (!addressWithChannels.ContainsKey(chAddress))
                                {
                                    addressWithChannels[chAddress] = new List<uint>();
                                }
                                addressWithChannels[chAddress].Add(Ch2StimulateChannel);
                            }
                            if (Ch3Enable)
                            {
                                var chAddress = SwitchWriteRegisterFunctions.SelectRegisterAddressWithChannel(Ch3StimulateChannel);
                                if (!addressWithChannels.ContainsKey(chAddress))
                                {
                                    addressWithChannels[chAddress] = new List<uint>();
                                }
                                addressWithChannels[chAddress].Add(Ch3StimulateChannel);
                            }
                            if (Ch4Enable)
                            {
                                var chAddress = SwitchWriteRegisterFunctions.SelectRegisterAddressWithChannel(Ch4StimulateChannel);
                                if (!addressWithChannels.ContainsKey(chAddress))
                                {
                                    addressWithChannels[chAddress] = new List<uint>();
                                }
                                addressWithChannels[chAddress].Add(Ch4StimulateChannel);
                            }
                            foreach (var item in addressWithChannels)
                            {
                                var address = item.Key;
                                var channels = item.Value;
                                uint totalWriteValue = 0;
                                foreach (var channel in channels)
                                {
                                    uint stimIndex = 0;
                                    if (channel == Ch1StimulateChannel)
                                    {
                                        stimIndex = 0;
                                    }
                                    else if (channel == Ch2StimulateChannel)
                                    {
                                        stimIndex = 1;
                                    }
                                    else if (channel == Ch3StimulateChannel)
                                    {
                                        stimIndex = 2;
                                    }
                                    else
                                    {
                                        stimIndex = 3;
                                    }
                                    var writeValue = SwitchWriteRegisterFunctions.GetWriteValue(channel, stimIndex);
                                    totalWriteValue += writeValue;
                                }
                                device.WriteRegister(address, totalWriteValue);
                            }
                            device.StartSwitch();
                        });

                        uint channelEnable = 0;
                        int maxDuration = 0;
                        DeviceManager.GetDevice(_stimulationDeviceName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                            if (Ch1Enable)
                            {
                                channelEnable += 0b0001;
                                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = ChangeParamByBiPhasic(Ch1BiPhasic, Ch1PhaseTwoCurrent, Ch1InterPhaseCurrent, Ch1PhaseTwoDuration, Ch1InterPhaseInterval, Ch1InterPulseInterval);
                                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = ValidateDuration(Ch1PhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, Ch1InterBurstInterval, Ch1TriggerDelay);

                                int ch1Duration = CalStimulationDuration(triggerDelayValidated, Ch1TrainBurstCount, Ch1BurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                                if (ch1Duration > maxDuration)
                                {
                                    maxDuration = ch1Duration;
                                }

                                device.WriteRegister(Headstage64ElectricalStimulator.CH1BURSTCNT, Ch1BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1BURSTINTERVAL, interBurstIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PHASEINTERVAL, interPhaseIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PULSEINTERVAL, interPulseIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1CURRENT1, ConvertUAToDeviceValue(Ch1PhaseOneCurrent));
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1CURRENT2, ConvertUAToDeviceValue(phaseTwoCurrent));
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PULSEDUR1, phaseOneDurationValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1PULSEDUR2, phaseTwoDurationValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1TRAINDELAY, triggerDelayValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1TRAINCNT, Ch1TrainBurstCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH1RESTCURRENT, ConvertUAToDeviceValue(interPhaseCurrent));
                            }
                            if (Ch2Enable)
                            {
                                channelEnable += 0b0010;
                                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = ChangeParamByBiPhasic(Ch2BiPhasic, Ch2PhaseTwoCurrent, Ch2InterPhaseCurrent, Ch2PhaseTwoDuration, Ch2InterPhaseInterval, Ch2InterPulseInterval);
                                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = ValidateDuration(Ch2PhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, Ch2InterBurstInterval, Ch2TriggerDelay);

                                int ch2Duration = CalStimulationDuration(triggerDelayValidated, Ch2TrainBurstCount, Ch2BurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                                if (ch2Duration > maxDuration)
                                {
                                    maxDuration = ch2Duration;
                                }

                                device.WriteRegister(Headstage64ElectricalStimulator.CH2BURSTCNT, Ch2BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2BURSTINTERVAL, interBurstIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2PHASEINTERVAL, interPhaseIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2PULSEINTERVAL, interPulseIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2CURRENT1, ConvertUAToDeviceValue(Ch2PhaseOneCurrent));
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2CURRENT2, ConvertUAToDeviceValue(phaseTwoCurrent));
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2PULSEDUR1, phaseOneDurationValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2PULSEDUR2, phaseTwoDurationValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2TRAINDELAY, triggerDelayValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2TRAINCNT, Ch2TrainBurstCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH2RESTCURRENT, ConvertUAToDeviceValue(interPhaseCurrent));
                            }
                            if (Ch3Enable)
                            {
                                channelEnable += 0b0100;
                                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = ChangeParamByBiPhasic(Ch3BiPhasic, Ch3PhaseTwoCurrent, Ch3InterPhaseCurrent, Ch3PhaseTwoDuration, Ch3InterPhaseInterval, Ch3InterPulseInterval);
                                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = ValidateDuration(Ch3PhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, Ch3InterBurstInterval, Ch3TriggerDelay);

                                int ch3Duration = CalStimulationDuration(triggerDelayValidated, Ch3TrainBurstCount, Ch3BurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                                if (ch3Duration > maxDuration)
                                {
                                    maxDuration = ch3Duration;
                                }

                                device.WriteRegister(Headstage64ElectricalStimulator.CH3BURSTCNT, Ch3BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3BURSTINTERVAL, interBurstIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3PHASEINTERVAL, interPhaseIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3PULSEINTERVAL, interPulseIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3CURRENT1, ConvertUAToDeviceValue(Ch3PhaseOneCurrent));
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3CURRENT2, ConvertUAToDeviceValue(phaseTwoCurrent));
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3PULSEDUR1, phaseOneDurationValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3PULSEDUR2, phaseTwoDurationValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3TRAINDELAY, triggerDelayValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3TRAINCNT, Ch3TrainBurstCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH3RESTCURRENT, ConvertUAToDeviceValue(interPhaseCurrent));
                            }
                            if (Ch4Enable)
                            {
                                channelEnable += 0b1000;
                                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = ChangeParamByBiPhasic(Ch4BiPhasic, Ch4PhaseTwoCurrent, Ch4InterPhaseCurrent, Ch4PhaseTwoDuration, Ch4InterPhaseInterval, Ch4InterPulseInterval);
                                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = ValidateDuration(Ch4PhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, Ch4InterBurstInterval, Ch4TriggerDelay);

                                int ch4Duration = CalStimulationDuration(triggerDelayValidated, Ch4TrainBurstCount, Ch4BurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                                if (ch4Duration > maxDuration)
                                {
                                    maxDuration = ch4Duration;
                                }

                                device.WriteRegister(Headstage64ElectricalStimulator.CH4BURSTCNT, Ch4BurstPulseCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4BURSTINTERVAL, interBurstIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4PHASEINTERVAL, interPhaseIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4PULSEINTERVAL, interPulseIntervalValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4CURRENT1, ConvertUAToDeviceValue(Ch4PhaseOneCurrent));
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4CURRENT2, ConvertUAToDeviceValue(phaseTwoCurrent));
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4PULSEDUR1, phaseOneDurationValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4PULSEDUR2, phaseTwoDurationValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4TRAINDELAY, triggerDelayValidated);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4TRAINCNT, Ch4TrainBurstCount);
                                device.WriteRegister(Headstage64ElectricalStimulator.CH4RESTCURRENT, ConvertUAToDeviceValue(interPhaseCurrent));
                            }

                            // 将设置的寄存器值写入设备
                            device.WriteRegister(Headstage64ElectricalStimulator.CHANNEL_ENABLE, channelEnable);
                            device.WriteRegister(Headstage64ElectricalStimulator.RESISTOR_MODE, 0);
                            device.StartStimulate();

                            Thread.Sleep(maxDuration / 1000); // 等待刺激完成
                            GlobalState.HubStates[GlobalState.DeviceNameToHubName[_switchDeviceName]] = HubState.Data;

                            DeviceManager.GetDevice(_switchDeviceName).Subscribe(x =>
                            {
                                var device = x.GetDeviceContext(typeof(SwitchDevice));
                                device.WriteRegister(SwitchDevice.SwitchCref, 4);
                                device.OpenAllAdc();
                                device.CloseAllDac();
                                device.StartSwitch();
                            });


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
