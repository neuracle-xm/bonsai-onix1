using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("切换成刺激模式")]
public class NeuracleStimulateMode : Sink<bool>
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
                _stimulationDeviceName = deviceTuple.Item2;
                _switchDeviceName = deviceTuple.Item3;
            }
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
    [Description("是否使用这个通道进行刺激")]
    [Category(DeviceFactory.StimulatorCh1)]
    public bool Ch1Enable { get; set; }

    private uint _ch1StimulateChannel;
    /// <summary>
    /// 设置刺激通道1 需要进行刺激的通道.
    /// </summary>
    [Description("选择第几个通道进行刺激")]
    [Category(DeviceFactory.StimulatorCh1)]
    public uint Ch1StimulateChannel
    {
        get => _ch1StimulateChannel;
        set => _ch1StimulateChannel = value > 63 ? 63 : value;
    }

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
        set => _ch1PhaseOneCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    private int _ch1InterPhaseCurrent;
    [Description("双相波的相间电流(uA)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public int Ch1InterPhaseCurrent
    {
        get => _ch1InterPhaseCurrent;
        set => _ch1InterPhaseCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    private int _ch1PhaseTwoCurrent;
    [Description("双相波的第二相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh1)]
    public int Ch1PhaseTwoCurrent
    {
        get => _ch1PhaseTwoCurrent;
        set => _ch1PhaseTwoCurrent = NeuracleUtils.LimitedCurrent(value);
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
    /// Gets or sets the device enable state.
    /// </summary>
    /// <remarks>
    /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
    /// </remarks>
    [Description("是否使用这个通道进行刺激")]
    [Category(DeviceFactory.StimulatorCh2)]
    public bool Ch2Enable { get; set; }

    private uint _ch2StimulateChannel;
    /// <summary>
    /// 设置刺激通道2 需要进行刺激的通道.
    /// </summary>
    [Description("选择第几个通道进行刺激")]
    [Category(DeviceFactory.StimulatorCh2)]
    public uint Ch2StimulateChannel
    {
        get => _ch2StimulateChannel;
        set => _ch2StimulateChannel = value > 63 ? 63 : value;
    }

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
        set => _ch2PhaseOneCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    private int _ch2InterPhaseCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the interphase current of each pulse in μA.
    /// </summary>
    [Description("双相波的相间电流(uA)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public int Ch2InterPhaseCurrent
    {
        get => _ch2InterPhaseCurrent;
        set => _ch2InterPhaseCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    private int _ch2PhaseTwoCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the second phase of each pulse in μA.
    /// </summary>
    [Description("双相波的第二相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh2)]
    public int Ch2PhaseTwoCurrent
    {
        get => _ch2PhaseTwoCurrent;
        set => _ch2PhaseTwoCurrent = NeuracleUtils.LimitedCurrent(value);
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

    /// <summary>
    /// Gets or sets the device enable state.
    /// </summary>
    /// <remarks>
    /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
    /// </remarks>
    [Description("是否使用这个通道进行刺激")]
    [Category(DeviceFactory.StimulatorCh3)]
    public bool Ch3Enable { get; set; }

    private uint _ch3StimulateChannel;
    /// <summary>
    /// 设置刺激通道3 需要进行刺激的通道.
    /// </summary>
    [Description("选择第几个通道进行刺激")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3StimulateChannel
    {
        get => _ch3StimulateChannel;
        set => _ch3StimulateChannel = value > 63 ? 63 : value;
    }

    /// <summary>
    /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
    /// </summary>
    [Description("刺激开始前的延迟(us)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3TriggerDelay { get; set; }

    private int _ch3PhaseOneCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the first phase of each pulse in μA.
    /// </summary>
    [Description("双相波的第一相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public int Ch3PhaseOneCurrent
    {
        get => _ch3PhaseOneCurrent;
        set => _ch3PhaseOneCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    private int _ch3InterPhaseCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the interphase current of each pulse in μA.
    /// </summary>
    [Description("双相波的相间电流(uA)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public int Ch3InterPhaseCurrent
    {
        get => _ch3InterPhaseCurrent;
        set => _ch3InterPhaseCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    private int _ch3PhaseTwoCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the second phase of each pulse in μA.
    /// </summary>
    [Description("双相波的第二相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public int Ch3PhaseTwoCurrent
    {
        get => _ch3PhaseTwoCurrent;
        set => _ch3PhaseTwoCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    /// <summary>
    /// Gets or sets the duration of the first phase of each pulse in μsec.
    /// </summary>
    [Description("双相波的第一相脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3PhaseOneDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of the interphase interval of each pulse in μsec.
    /// </summary>
    [Description("双相波的相间脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3InterPhaseInterval { get; set; }

    /// <summary>
    /// Gets or sets the duration of the second phase of each pulse in μsec.
    /// </summary>
    [Description("双相波的第二相脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3PhaseTwoDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
    /// </summary>
    [Description("每个Burst内部的脉冲间隔(us)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3InterPulseInterval { get; set; }

    /// <summary>
    /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
    /// </summary>
    [Description("各个Burst之间的间隔(us)")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3InterBurstInterval { get; set; }

    private uint _ch3BurstPulseCount = 1;
    /// <summary>
    /// Gets or sets the number of pulses per burst.
    /// </summary>
    [Description("每个Burst的脉冲个数")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3BurstPulseCount
    {
        get => _ch3BurstPulseCount;
        set => _ch3BurstPulseCount = value < 1 ? 1 : value;
    }

    private uint _ch3TrainBurstCount = 1;
    /// <summary>
    /// Gets or sets the number of bursts in a stimulus train.
    /// </summary>
    [Description("每个Train的Burst的个数")]
    [Category(DeviceFactory.StimulatorCh3)]
    public uint Ch3TrainBurstCount
    {
        get => _ch3TrainBurstCount;
        set => _ch3TrainBurstCount = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets or set Ch3BiPhasic.
    /// </summary>
    [Description("是否使用双相波")]
    [Category(DeviceFactory.StimulatorCh3)]
    public bool Ch3BiPhasic { get; set; }

    /// <summary>
    /// Gets or sets the device enable state.
    /// </summary>
    /// <remarks>
    /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
    /// </remarks>
    [Description("是否使用这个通道进行刺激")]
    [Category(DeviceFactory.StimulatorCh4)]
    public bool Ch4Enable { get; set; }

    private uint _ch4StimulateChannel;
    /// <summary>
    /// 设置刺激通道4 需要进行刺激的通道.
    /// </summary>
    [Description("选择第几个通道进行刺激")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4StimulateChannel
    {
        get => _ch4StimulateChannel;
        set => _ch4StimulateChannel = value > 63 ? 63 : value;
    }

    /// <summary>
    /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec
    /// </summary>
    [Description("刺激开始前的延迟(us)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4TriggerDelay { get; set; }

    private int _ch4PhaseOneCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the first phase of each pulse in μA.
    /// </summary>
    [Description("双相波的第一相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public int Ch4PhaseOneCurrent
    {
        get => _ch4PhaseOneCurrent;
        set => _ch4PhaseOneCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    private int _ch4InterPhaseCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the interphase current of each pulse in μA.
    /// </summary>
    [Description("双相波的相间电流(uA)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public int Ch4InterPhaseCurrent
    {
        get => _ch4InterPhaseCurrent;
        set => _ch4InterPhaseCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    private int _ch4PhaseTwoCurrent;
    /// <summary>
    /// Gets or sets the amplitude of the second phase of each pulse in μA.
    /// </summary>
    [Description("双相波的第二相电流(uA)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public int Ch4PhaseTwoCurrent
    {
        get => _ch4PhaseTwoCurrent;
        set => _ch4PhaseTwoCurrent = NeuracleUtils.LimitedCurrent(value);
    }

    /// <summary>
    /// Gets or sets the duration of the first phase of each pulse in μsec.
    /// </summary>
    [Description("双相波的第一相脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4PhaseOneDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of the interphase interval of each pulse in μsec.
    /// </summary>
    [Description("双相波的相间脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4InterPhaseInterval { get; set; }

    /// <summary>
    /// Gets or sets the duration of the second phase of each pulse in μsec.
    /// </summary>
    [Description("双相波的第二相脉宽(us)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4PhaseTwoDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
    /// </summary>
    [Description("每个Burst内部的脉冲间隔(us)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4InterPulseInterval { get; set; }

    /// <summary>
    /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
    /// </summary>
    [Description("各个Burst之间的间隔(us)")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4InterBurstInterval { get; set; }

    private uint _ch4BurstPulseCount = 1;
    /// <summary>
    /// Gets or sets the number of pulses per burst.
    /// </summary>
    [Description("每个Burst的脉冲个数")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4BurstPulseCount
    {
        get => _ch4BurstPulseCount;
        set => _ch4BurstPulseCount = value < 1 ? 1 : value;
    }

    private uint _ch4TrainBurstCount = 1;
    /// <summary>
    /// Gets or sets the number of bursts in a stimulus train.
    /// </summary>
    [Description("每个Train的Burst的个数")]
    [Category(DeviceFactory.StimulatorCh4)]
    public uint Ch4TrainBurstCount
    {
        get => _ch4TrainBurstCount;
        set => _ch4TrainBurstCount = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets or set Ch4BiPhasic.
    /// </summary>
    [Description("是否使用双相波")]
    [Category(DeviceFactory.StimulatorCh4)]
    public bool Ch4BiPhasic { get; set; }

    /// <summary>
    /// 下发刺激
    /// </summary>
    /// <param name="stimulationDeviceName"></param>
    /// <param name="switchDeviceName"></param>
    /// <param name="ch1StimulateParameter"></param>
    /// <param name="ch2StimulateParameter"></param>
    /// <param name="ch3StimulateParameter"></param>
    /// <param name="ch4StimulateParameter"></param>
    public static void StimulateProcedure(string stimulationDeviceName, string switchDeviceName, StimulateParameter ch1StimulateParameter,
                                          StimulateParameter ch2StimulateParameter, StimulateParameter ch3StimulateParameter, StimulateParameter ch4StimulateParameter)
    {
        if (NeuracleGlobalState.DeviceNameToHubName.TryGetValue(switchDeviceName, out var hubName))
        {
            NeuracleGlobalState.HubStates[hubName] = HubState.Stimulation;
        }
        DeviceManager.GetDevice(switchDeviceName).Subscribe(x =>
        {
            var device = x.GetDeviceContext(typeof(SwitchDevice));
            device.WriteRegister(SwitchDevice.SwitchCref, 0);
            device.CloseAllAdc();
            //把所有使用的通道分成几个組
            Dictionary<uint, List<uint>> addressWithChannels = new();
            if (ch1StimulateParameter.ChEnable)
            {
                var chAddress = SwitchWriteRegisterFunctions.SelectRegisterAddressWithChannel(ch1StimulateParameter.ChStimulateChannel);
                if (!addressWithChannels.ContainsKey(chAddress))
                {
                    addressWithChannels[chAddress] = new List<uint>();
                }
                addressWithChannels[chAddress].Add(ch1StimulateParameter.ChStimulateChannel);
            }
            if (ch2StimulateParameter.ChEnable)
            {
                var chAddress = SwitchWriteRegisterFunctions.SelectRegisterAddressWithChannel(ch2StimulateParameter.ChStimulateChannel);
                if (!addressWithChannels.ContainsKey(chAddress))
                {
                    addressWithChannels[chAddress] = new List<uint>();
                }
                addressWithChannels[chAddress].Add(ch2StimulateParameter.ChStimulateChannel);
            }
            if (ch3StimulateParameter.ChEnable)
            {
                var chAddress = SwitchWriteRegisterFunctions.SelectRegisterAddressWithChannel(ch3StimulateParameter.ChStimulateChannel);
                if (!addressWithChannels.ContainsKey(chAddress))
                {
                    addressWithChannels[chAddress] = new List<uint>();
                }
                addressWithChannels[chAddress].Add(ch3StimulateParameter.ChStimulateChannel);
            }
            if (ch4StimulateParameter.ChEnable)
            {
                var chAddress = SwitchWriteRegisterFunctions.SelectRegisterAddressWithChannel(ch4StimulateParameter.ChStimulateChannel);
                if (!addressWithChannels.ContainsKey(chAddress))
                {
                    addressWithChannels[chAddress] = new List<uint>();
                }
                addressWithChannels[chAddress].Add(ch4StimulateParameter.ChStimulateChannel);
            }
            uint newDacValue = 0;
            foreach (var item in addressWithChannels)
            {
                var address = item.Key;
                var channels = item.Value;
                uint totalWriteValue = 0;
                foreach (var channel in channels)
                {
                    uint stimIndex = 0;
                    //刺激通道1用stima
                    if (channel == ch1StimulateParameter.ChStimulateChannel)
                    {
                        stimIndex = 0;
                        newDacValue += 0b01000000;
                    }
                    //刺激通道2用stimb
                    else if (channel == ch2StimulateParameter.ChStimulateChannel)
                    {
                        stimIndex = 1;
                        newDacValue += 0b01000000_00000000;
                    }
                    //刺激通道3用stimc
                    else if (channel == ch3StimulateParameter.ChStimulateChannel)
                    {
                        stimIndex = 2;
                        newDacValue += 0b01000000_00000000_00000000;
                    }
                    //刺激通道4用stimd
                    else
                    {
                        stimIndex = 3;
                        newDacValue += 0b01000000_00000000_00000000_00000000;
                    }
                    var writeValue = SwitchWriteRegisterFunctions.GetWriteValue(channel, stimIndex);
                    totalWriteValue += writeValue;
                }
                device.WriteRegister(address, totalWriteValue);
            }
            device.WriteRegister(SwitchDevice.SwitchNewDac, newDacValue);
            device.StartSwitch();
        });
        uint channelEnable = 0;
        int maxDuration = 0;
        DeviceManager.GetDevice(stimulationDeviceName).Subscribe(x =>
        {
            var device = x.GetDeviceContext(typeof(ElectricalStimulator));
            if (ch1StimulateParameter.ChEnable)
            {
                channelEnable += 0b0001;
                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = NeuracleUtils.ChangeParamByBiPhasic(ch1StimulateParameter.ChBiPhasic, ch1StimulateParameter.ChPhaseTwoCurrent, ch1StimulateParameter.ChInterPhaseCurrent, ch1StimulateParameter.ChPhaseTwoDuration, ch1StimulateParameter.ChInterPhaseInterval, ch1StimulateParameter.ChInterPulseInterval);
                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = NeuracleUtils.ValidateDuration(ch1StimulateParameter.ChPhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, ch1StimulateParameter.ChInterBurstInterval, ch1StimulateParameter.ChTriggerDelay);
                int ch1Duration = NeuracleUtils.CalStimulationDuration(triggerDelayValidated, ch1StimulateParameter.ChTrainBurstCount, ch1StimulateParameter.ChBurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                if (ch1Duration > maxDuration)
                {
                    maxDuration = ch1Duration;
                }
                device.WriteRegister(ElectricalStimulator.CH1BURSTCNT, ch1StimulateParameter.ChBurstPulseCount);
                device.WriteRegister(ElectricalStimulator.CH1BURSTINTERVAL, interBurstIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH1PHASEINTERVAL, interPhaseIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH1PULSEINTERVAL, interPulseIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH1CURRENT1, NeuracleUtils.ConvertUAToDeviceValue(ch1StimulateParameter.ChPhaseOneCurrent));
                device.WriteRegister(ElectricalStimulator.CH1CURRENT2, NeuracleUtils.ConvertUAToDeviceValue(phaseTwoCurrent));
                device.WriteRegister(ElectricalStimulator.CH1PULSEDUR1, phaseOneDurationValidated);
                device.WriteRegister(ElectricalStimulator.CH1PULSEDUR2, phaseTwoDurationValidated);
                device.WriteRegister(ElectricalStimulator.CH1TRAINDELAY, triggerDelayValidated);
                device.WriteRegister(ElectricalStimulator.CH1TRAINCNT, ch1StimulateParameter.ChTrainBurstCount);
                device.WriteRegister(ElectricalStimulator.CH1RESTCURRENT, NeuracleUtils.ConvertUAToDeviceValue(interPhaseCurrent));
            }
            if (ch2StimulateParameter.ChEnable)
            {
                channelEnable += 0b0010;
                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = NeuracleUtils.ChangeParamByBiPhasic(ch2StimulateParameter.ChBiPhasic, ch2StimulateParameter.ChPhaseTwoCurrent, ch2StimulateParameter.ChInterPhaseCurrent, ch2StimulateParameter.ChPhaseTwoDuration, ch2StimulateParameter.ChInterPhaseInterval, ch2StimulateParameter.ChInterPulseInterval);
                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = NeuracleUtils.ValidateDuration(ch2StimulateParameter.ChPhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, ch2StimulateParameter.ChInterBurstInterval, ch2StimulateParameter.ChTriggerDelay);
                int ch2Duration = NeuracleUtils.CalStimulationDuration(triggerDelayValidated, ch2StimulateParameter.ChTrainBurstCount, ch2StimulateParameter.ChBurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                if (ch2Duration > maxDuration)
                {
                    maxDuration = ch2Duration;
                }
                device.WriteRegister(ElectricalStimulator.CH2BURSTCNT, ch2StimulateParameter.ChBurstPulseCount);
                device.WriteRegister(ElectricalStimulator.CH2BURSTINTERVAL, interBurstIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH2PHASEINTERVAL, interPhaseIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH2PULSEINTERVAL, interPulseIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH2CURRENT1, NeuracleUtils.ConvertUAToDeviceValue(ch2StimulateParameter.ChPhaseOneCurrent));
                device.WriteRegister(ElectricalStimulator.CH2CURRENT2, NeuracleUtils.ConvertUAToDeviceValue(phaseTwoCurrent));
                device.WriteRegister(ElectricalStimulator.CH2PULSEDUR1, phaseOneDurationValidated);
                device.WriteRegister(ElectricalStimulator.CH2PULSEDUR2, phaseTwoDurationValidated);
                device.WriteRegister(ElectricalStimulator.CH2TRAINDELAY, triggerDelayValidated);
                device.WriteRegister(ElectricalStimulator.CH2TRAINCNT, ch2StimulateParameter.ChTrainBurstCount);
                device.WriteRegister(ElectricalStimulator.CH2RESTCURRENT, NeuracleUtils.ConvertUAToDeviceValue(interPhaseCurrent));
            }
            if (ch3StimulateParameter.ChEnable)
            {
                channelEnable += 0b0100;
                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = NeuracleUtils.ChangeParamByBiPhasic(ch3StimulateParameter.ChBiPhasic, ch3StimulateParameter.ChPhaseTwoCurrent, ch3StimulateParameter.ChInterPhaseCurrent, ch3StimulateParameter.ChPhaseTwoDuration, ch3StimulateParameter.ChInterPhaseInterval, ch3StimulateParameter.ChInterPulseInterval);
                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = NeuracleUtils.ValidateDuration(ch3StimulateParameter.ChPhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, ch3StimulateParameter.ChInterBurstInterval, ch3StimulateParameter.ChTriggerDelay);
                int ch3Duration = NeuracleUtils.CalStimulationDuration(triggerDelayValidated, ch3StimulateParameter.ChTrainBurstCount, ch3StimulateParameter.ChBurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                if (ch3Duration > maxDuration)
                {
                    maxDuration = ch3Duration;
                }
                device.WriteRegister(ElectricalStimulator.CH3BURSTCNT, ch3StimulateParameter.ChBurstPulseCount);
                device.WriteRegister(ElectricalStimulator.CH3BURSTINTERVAL, interBurstIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH3PHASEINTERVAL, interPhaseIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH3PULSEINTERVAL, interPulseIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH3CURRENT1, NeuracleUtils.ConvertUAToDeviceValue(ch3StimulateParameter.ChPhaseOneCurrent));
                device.WriteRegister(ElectricalStimulator.CH3CURRENT2, NeuracleUtils.ConvertUAToDeviceValue(phaseTwoCurrent));
                device.WriteRegister(ElectricalStimulator.CH3PULSEDUR1, phaseOneDurationValidated);
                device.WriteRegister(ElectricalStimulator.CH3PULSEDUR2, phaseTwoDurationValidated);
                device.WriteRegister(ElectricalStimulator.CH3TRAINDELAY, triggerDelayValidated);
                device.WriteRegister(ElectricalStimulator.CH3TRAINCNT, ch3StimulateParameter.ChTrainBurstCount);
                device.WriteRegister(ElectricalStimulator.CH3RESTCURRENT, NeuracleUtils.ConvertUAToDeviceValue(interPhaseCurrent));
            }
            if (ch4StimulateParameter.ChEnable)
            {
                channelEnable += 0b1000;
                (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = NeuracleUtils.ChangeParamByBiPhasic(ch4StimulateParameter.ChBiPhasic, ch4StimulateParameter.ChPhaseTwoCurrent, ch4StimulateParameter.ChInterPhaseCurrent, ch4StimulateParameter.ChPhaseTwoDuration, ch4StimulateParameter.ChInterPhaseInterval, ch4StimulateParameter.ChInterPulseInterval);
                (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = NeuracleUtils.ValidateDuration(ch4StimulateParameter.ChPhaseOneDuration, phaseTwoDuration, interPhaseInterval, interPulseInterval, ch4StimulateParameter.ChInterBurstInterval, ch4StimulateParameter.ChTriggerDelay);
                int ch4Duration = NeuracleUtils.CalStimulationDuration(triggerDelayValidated, ch4StimulateParameter.ChTrainBurstCount, ch4StimulateParameter.ChBurstPulseCount, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
                if (ch4Duration > maxDuration)
                {
                    maxDuration = ch4Duration;
                }
                device.WriteRegister(ElectricalStimulator.CH4BURSTCNT, ch4StimulateParameter.ChBurstPulseCount);
                device.WriteRegister(ElectricalStimulator.CH4BURSTINTERVAL, interBurstIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH4PHASEINTERVAL, interPhaseIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH4PULSEINTERVAL, interPulseIntervalValidated);
                device.WriteRegister(ElectricalStimulator.CH4CURRENT1, NeuracleUtils.ConvertUAToDeviceValue(ch4StimulateParameter.ChPhaseOneCurrent));
                device.WriteRegister(ElectricalStimulator.CH4CURRENT2, NeuracleUtils.ConvertUAToDeviceValue(phaseTwoCurrent));
                device.WriteRegister(ElectricalStimulator.CH4PULSEDUR1, phaseOneDurationValidated);
                device.WriteRegister(ElectricalStimulator.CH4PULSEDUR2, phaseTwoDurationValidated);
                device.WriteRegister(ElectricalStimulator.CH4TRAINDELAY, triggerDelayValidated);
                device.WriteRegister(ElectricalStimulator.CH4TRAINCNT, ch4StimulateParameter.ChTrainBurstCount);
                device.WriteRegister(ElectricalStimulator.CH4RESTCURRENT, NeuracleUtils.ConvertUAToDeviceValue(interPhaseCurrent));
            }
            // 将设置的寄存器值写入设备
            device.WriteRegister(ElectricalStimulator.CHANNEL_ENABLE, channelEnable);
            device.WriteRegister(ElectricalStimulator.RESISTOR_MODE, 0);
            device.StartStimulate();
            Task.Run((() =>
            {
                var _switchDeviceName = switchDeviceName;
                // 等待刺激完成
                Thread.Sleep(maxDuration / 1000);
                if (NeuracleGlobalState.DeviceNameToHubName.TryGetValue(_switchDeviceName, out var hubName))
                {
                    NeuracleGlobalState.HubStates[hubName] = HubState.Data;
                }
                DeviceManager.GetDevice(_switchDeviceName).Subscribe(x =>
                {
                    var device = x.GetDeviceContext(typeof(SwitchDevice));
                    device.WriteRegister(SwitchDevice.SwitchCref, 1028);
                    device.OpenAllAdc();
                    device.CloseAllDac();
                    device.StartSwitch();
                });
            }));
        });
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
                    var ch1StimulateParameter = new StimulateParameter(Ch1BiPhasic, Ch1BurstPulseCount, Ch1Enable, Ch1InterBurstInterval,
                                                                       Ch1InterPhaseCurrent, Ch1InterPhaseInterval, Ch1InterPulseInterval,
                                                                       Ch1PhaseOneCurrent, Ch1PhaseOneDuration, Ch1PhaseTwoCurrent, Ch1PhaseTwoDuration,
                                                                       Ch1StimulateChannel, Ch1TrainBurstCount, Ch1TriggerDelay);
                    var ch2StimulateParameter = new StimulateParameter(Ch2BiPhasic, Ch2BurstPulseCount, Ch2Enable, Ch2InterBurstInterval,
                                                                       Ch2InterPhaseCurrent, Ch2InterPhaseInterval, Ch2InterPulseInterval,
                                                                       Ch2PhaseOneCurrent, Ch2PhaseOneDuration, Ch2PhaseTwoCurrent, Ch2PhaseTwoDuration,
                                                                       Ch2StimulateChannel, Ch2TrainBurstCount, Ch2TriggerDelay);
                    var ch3StimulateParameter = new StimulateParameter(Ch3BiPhasic, Ch3BurstPulseCount, Ch3Enable, Ch3InterBurstInterval,
                                                                       Ch3InterPhaseCurrent, Ch3InterPhaseInterval, Ch3InterPulseInterval,
                                                                       Ch3PhaseOneCurrent, Ch3PhaseOneDuration, Ch3PhaseTwoCurrent, Ch3PhaseTwoDuration,
                                                                       Ch3StimulateChannel, Ch3TrainBurstCount, Ch3TriggerDelay);
                    var ch4StimulateParameter = new StimulateParameter(Ch4BiPhasic, Ch4BurstPulseCount, Ch4Enable, Ch4InterBurstInterval,
                                                                       Ch4InterPhaseCurrent, Ch4InterPhaseInterval, Ch4InterPulseInterval,
                                                                       Ch4PhaseOneCurrent, Ch4PhaseOneDuration, Ch4PhaseTwoCurrent, Ch4PhaseTwoDuration,
                                                                       Ch4StimulateChannel, Ch4TrainBurstCount, Ch4TriggerDelay);
                    StimulateProcedure(_stimulationDeviceName, _switchDeviceName,
                                       ch1StimulateParameter, ch2StimulateParameter,
                                       ch3StimulateParameter, ch4StimulateParameter);
                    var messageBox = new NeuracleMessageBox("下发刺激成功");
                    messageBox.Show();
                    observer.OnNext(value);
                },
                observer.OnError,
                observer.OnCompleted);
            return source.SubscribeSafe(triggerObserver);
        });
    }
}
