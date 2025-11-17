using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("两个Headstage同时刺激")]
public class NeuracleTwoHeadstageStimulateMode : NeuracleHeadstageStimulateMode
{
    /// <summary>
    /// 刺激设备2的DeviceName
    /// </summary>
    [TypeConverter(typeof(HeadstageStimulator.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string Device2Name { get; set; }

    /// <summary>
    /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
    /// </summary>
    [Description("刺激开始前的延迟(us)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2TriggerDelay { get; set; }

    private int _ch2PhaseOneCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the first phase of each pulse in μA.
    /// </summary>
    [Description("双相波的第一相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public int Ch2PhaseOneCurrent
    {
        get => _ch2PhaseOneCurrent;
        set => _ch2PhaseOneCurrent = NeuracleUtils.HeadstageLimitedCurrent(value);
    }

    private int _ch2InterPhaseCurrent;
    [Description("双相波的相间电流(uA)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public int Ch2InterPhaseCurrent
    {
        get => _ch2InterPhaseCurrent;
        set => _ch2InterPhaseCurrent = NeuracleUtils.HeadstageLimitedCurrent(value);
    }

    private int _ch2PhaseTwoCurrent;
    [Description("双相波的第二相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public int Ch2PhaseTwoCurrent
    {
        get => _ch2PhaseTwoCurrent;
        set => _ch2PhaseTwoCurrent = NeuracleUtils.HeadstageLimitedCurrent(value);
    }

    /// <summary>
    /// Gets or sets the duration of the first phase of each pulse in μsec.
    /// </summary>
    [Description("双相波的第一相脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2PhaseOneDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of the interphase interval of each pulse in μsec.
    /// </summary>
    [Description("双相波的相间脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2InterPhaseInterval { get; set; }

    /// <summary>
    /// Gets or sets the duration of the second phase of each pulse in μsec.
    /// </summary>
    [Description("双相波的第二相脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2PhaseTwoDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
    /// </summary>
    [Description("每个Burst内部的脉冲间隔(us)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2InterPulseInterval { get; set; }

    /// <summary>
    /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
    /// </summary>
    [Description("各个Burst之间的间隔(us)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2InterBurstInterval { get; set; }

    private uint _ch2BurstPulseCount = 1;
    /// <summary>
    /// Gets or sets the number of pulses per burst.
    /// </summary>
    [Description("每个Burst的脉冲个数")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2BurstPulseCount
    {
        get => _ch2BurstPulseCount;
        set => _ch2BurstPulseCount = value < 1 ? 1 : value;
    }

    private uint _ch2TrainBurstCount = 1;
    /// <summary>
    /// Gets or sets the number of bursts in a stimulus train.
    /// </summary>
    [Description("每个Train的Burst的个数")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2TrainBurstCount
    {
        get => _ch2TrainBurstCount;
        set => _ch2TrainBurstCount = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets or set Ch2BiPhasic.
    /// </summary>
    [Description("是否使用双相波")]
    [Category(DeviceFactory.StimulatorCh2)]
    public bool Ch2BiPhasic { get; set; }

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
                    var ch2StimulateParameter = new StimulateParameter(Ch2BiPhasic, Ch2BurstPulseCount, true, Ch2InterBurstInterval,
                                                                       Ch2InterPhaseCurrent, Ch2InterPhaseInterval, Ch2InterPulseInterval,
                                                                       Ch2PhaseOneCurrent, Ch2PhaseOneDuration, Ch2PhaseTwoCurrent, Ch2PhaseTwoDuration,
                                                                       0, Ch2TrainBurstCount, Ch2TriggerDelay);
                    StimulateProcedure(Device2Name, ch2StimulateParameter);
                    observer.OnNext(value);
                },
                observer.OnError,
                observer.OnCompleted);
            return source.SubscribeSafe(triggerObserver);
        });
    }
}
