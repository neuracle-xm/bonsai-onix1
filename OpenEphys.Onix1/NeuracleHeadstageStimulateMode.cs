using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Headstage切换成刺激模式")]
public class NeuracleHeadstageStimulateMode : Sink<bool>
{
    /// <summary>
    /// 刺激设备的DeviceName
    /// </summary>
    [TypeConverter(typeof(HeadstageStimulator.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string DeviceName { get; set; }

    /// <summary>
    /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
    /// </summary>
    [Description("刺激开始前的延迟(us)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1TriggerDelay { get; set; }

    private int _ch1PhaseOneCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the first phase of each pulse in μA.
    /// </summary>
    [Description("双相波的第一相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public int Ch1PhaseOneCurrent
    {
        get => _ch1PhaseOneCurrent;
        set => _ch1PhaseOneCurrent = NeuracleUtils.HeadstageLimitedCurrent(value);
    }

    private int _ch1InterPhaseCurrent;
    [Description("双相波的相间电流(uA)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public int Ch1InterPhaseCurrent
    {
        get => _ch1InterPhaseCurrent;
        set => _ch1InterPhaseCurrent = NeuracleUtils.HeadstageLimitedCurrent(value);
    }

    private int _ch1PhaseTwoCurrent;
    [Description("双相波的第二相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public int Ch1PhaseTwoCurrent
    {
        get => _ch1PhaseTwoCurrent;
        set => _ch1PhaseTwoCurrent = NeuracleUtils.HeadstageLimitedCurrent(value);
    }

    /// <summary>
    /// Gets or sets the duration of the first phase of each pulse in μsec.
    /// </summary>
    [Description("双相波的第一相脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1PhaseOneDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of the interphase interval of each pulse in μsec.
    /// </summary>
    [Description("双相波的相间脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1InterPhaseInterval { get; set; }

    /// <summary>
    /// Gets or sets the duration of the second phase of each pulse in μsec.
    /// </summary>
    [Description("双相波的第二相脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1PhaseTwoDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
    /// </summary>
    [Description("每个Burst内部的脉冲间隔(us)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1InterPulseInterval { get; set; }

    /// <summary>
    /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
    /// </summary>
    [Description("各个Burst之间的间隔(us)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1InterBurstInterval { get; set; }

    private uint _ch1BurstPulseCount = 1;
    /// <summary>
    /// Gets or sets the number of pulses per burst.
    /// </summary>
    [Description("每个Burst的脉冲个数")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1BurstPulseCount
    {
        get => _ch1BurstPulseCount;
        set => _ch1BurstPulseCount = value < 1 ? 1 : value;
    }

    private uint _ch1TrainBurstCount = 1;
    /// <summary>
    /// Gets or sets the number of bursts in a stimulus train.
    /// </summary>
    [Description("每个Train的Burst的个数")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1TrainBurstCount
    {
        get => _ch1TrainBurstCount;
        set => _ch1TrainBurstCount = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets or set Ch1BiPhasic.
    /// </summary>
    [Description("是否使用双相波")]
    [Category(DeviceFactory.StimulatorCh1)]
    public bool Ch1BiPhasic { get; set; }

    /// <summary>
    /// 写寄存器之间的间隔时间,单位ms
    /// </summary>
    private const int _delay = 100;

    public static void StimulateProcedure(string deviceName, StimulateParameter ch1StimulateParameter)
    {
        Task task = Task.Run(() =>
        {
            DeviceManager.GetDevice(deviceName).Subscribe(x =>
            {
                var device = x.GetDeviceContext(typeof(HeadstageStimulator));
                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = NeuracleUtils.ChangeParamByBiPhasic(ch1StimulateParameter.ChBiPhasic, ch1StimulateParameter.ChPhaseTwoCurrent, ch1StimulateParameter.ChInterPhaseCurrent, ch1StimulateParameter.ChPhaseTwoDuration, ch1StimulateParameter.ChInterPhaseInterval, ch1StimulateParameter.ChInterPulseInterval);
                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = NeuracleUtils.ValidateDuration(ch1StimulateParameter.ChPhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, ch1StimulateParameter.ChInterBurstInterval, ch1StimulateParameter.ChTriggerDelay);
                int ch1Duration = NeuracleUtils.CalStimulationDuration(triggerDelayValidated, ch1StimulateParameter.ChTrainBurstCount, ch1StimulateParameter.ChBurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                device.WriteRegister(HeadstageStimulator.CH1PULSEDUR1, phaseOneDurationValidated);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1PULSEDUR2, phaseTwoDurationValidated);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1PHASEINTERVAL, interPhaseIntervalValidated);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1PULSEINTERVAL, interPulseIntervalValidated);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1BURSTCNT, ch1StimulateParameter.ChBurstPulseCount);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1BURSTINTERVAL, interBurstIntervalValidated);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1CURRENT1, NeuracleUtils.HeadstageConvertUAToDeviceValue(ch1StimulateParameter.ChPhaseOneCurrent));
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1CURRENT2, NeuracleUtils.HeadstageConvertUAToDeviceValue(phaseTwoCurrent));
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1RESTCURRENT, NeuracleUtils.HeadstageConvertUAToDeviceValue(interPhaseCurrent));
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1TRAINCNT, ch1StimulateParameter.ChTrainBurstCount);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.CH1TRAINDELAY, triggerDelayValidated);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.STIM_NULL, 1);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.POWER_EN, 1);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.STIM_START, 0);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.STIM_START, 1);
                Thread.Sleep(_delay);
                device.WriteRegister(HeadstageStimulator.STIM_START, 0);
            });
        });
        task.Wait();
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
                    //ChEnable和ChStimulateChannel在Headstage这边没用
                    var ch1StimulateParameter = new StimulateParameter(Ch1BiPhasic, Ch1BurstPulseCount, true, Ch1InterBurstInterval,
                                                                       Ch1InterPhaseCurrent, Ch1InterPhaseInterval, Ch1InterPulseInterval,
                                                                       Ch1PhaseOneCurrent, Ch1PhaseOneDuration, Ch1PhaseTwoCurrent, Ch1PhaseTwoDuration,
                                                                       0, Ch1TrainBurstCount, Ch1TriggerDelay);
                    StimulateProcedure(DeviceName, ch1StimulateParameter);
                    var messageBox = new NeuracleMessageBox("Headstage下发刺激成功");
                    messageBox.Show();
                    observer.OnNext(value);
                },
                observer.OnError,
                observer.OnCompleted);
            return source.SubscribeSafe(triggerObserver);
        });
    }
}
