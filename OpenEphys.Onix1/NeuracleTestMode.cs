using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using oni;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("切换成测试模式")]
public class NeuracleTestMode : Sink<bool>
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
                _switchDeviceName = deviceTuple.Item3;
            }
        }
    }

    /// <summary>
    /// 模拟开关的DeviceName
    /// </summary>
    private string _switchDeviceName;

    /// <summary>
    /// 切换到采集模式
    /// </summary>
    /// <param name="switchDeviceName"></param>
    public static void DataProcedure(string switchDeviceName)
    {
        NeuracleGlobalState.HubStates[NeuracleGlobalState.DeviceNameToHubName[switchDeviceName]] = HubState.Data;
        DeviceManager.GetDevice(switchDeviceName).Subscribe(deviceInfo =>
        {
            var switchDevice = deviceInfo.GetDeviceContext(typeof(SwitchDevice));
            //几个刺激的开关，把stim_a b c d 切到 S4上
            switchDevice.WriteRegister(SwitchDevice.SwitchNewDac, 0b00010000000100000001000000010000);
            //刺激A输出0mA给所有电极
            //Switch_dac 中所有通道的stima bit 置为 1，其他置为 0
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac0_31, 0b10000000010000001000000001000000);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac0_31, 0);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac32_63, 0b10000000010000001000000001000000);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac32_63, 0);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac64_95, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac96_127, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac128_159, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac160_191, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac192_223, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac224_255, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac256_287, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac288_319, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac320_351, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac352_383, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac384_415, 0b10000000010000001000000001000000);
            //switchDevice.WriteRegister(SwitchDevice.SwitchDac416_447, 0b10000000010000001000000001000000);

            switchDevice.WriteRegister(SwitchDevice.SwitchDac64_95, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac96_127, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac128_159, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac160_191, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac192_223, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac224_255, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac256_287, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac288_319, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac320_351, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac352_383, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac384_415, 0);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac416_447, 0);

            switchDevice.WriteRegister(SwitchDevice.SwitchDac448_479, 0b10000000000000001000000000000000);
            switchDevice.WriteRegister(SwitchDevice.SwitchDac480_511, 0b10000000010000001000000001000000);
            //把控制stima的dac开关打开
            switchDevice.WriteRegister(SwitchDevice.SwitchNewDac, 0b01000000);
            //Switch_start 先置 0，再置 1，再置 0
            switchDevice.StartSwitch();
            //所有刺激参数设置好(刺激通道1为0mA，10秒，其他不用管)
            var current = 0;
            (var phaseTwoCurrent, var interPhaseCurrent, var phaseTwoDuration, var interPhaseInterval, var interPulseInterval) = NeuracleUtils.ChangeParamByBiPhasic(true, current, current, 4 * 1000 * 1000, 2 * 1000 * 1000, 0);
            (var phaseOneDurationValidated, var phaseTwoDurationValidated, var interPhaseIntervalValidated, var interPulseIntervalValidated, var interBurstIntervalValidated, var triggerDelayValidated) = NeuracleUtils.ValidateDuration(4 * 1000 * 1000, phaseTwoDuration, interPhaseInterval, interPulseInterval, 0, 0);
            int ch1Duration = NeuracleUtils.CalStimulationDuration(triggerDelayValidated, 1, 1, interPulseIntervalValidated, interBurstIntervalValidated, phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated);
            switchDevice.WriteRegister(ElectricalStimulator.CH1BURSTCNT, 1);
            switchDevice.WriteRegister(ElectricalStimulator.CH1BURSTINTERVAL, interBurstIntervalValidated);
            switchDevice.WriteRegister(ElectricalStimulator.CH1PHASEINTERVAL, interPhaseIntervalValidated);
            switchDevice.WriteRegister(ElectricalStimulator.CH1PULSEINTERVAL, interPulseIntervalValidated);
            switchDevice.WriteRegister(ElectricalStimulator.CH1CURRENT1, NeuracleUtils.ConvertUAToDeviceValue(current));
            switchDevice.WriteRegister(ElectricalStimulator.CH1CURRENT2, NeuracleUtils.ConvertUAToDeviceValue(phaseTwoCurrent));
            switchDevice.WriteRegister(ElectricalStimulator.CH1PULSEDUR1, phaseOneDurationValidated);
            switchDevice.WriteRegister(ElectricalStimulator.CH1PULSEDUR2, phaseTwoDurationValidated);
            switchDevice.WriteRegister(ElectricalStimulator.CH1TRAINDELAY, triggerDelayValidated);
            switchDevice.WriteRegister(ElectricalStimulator.CH1TRAINCNT, 1);
            switchDevice.WriteRegister(ElectricalStimulator.CH1RESTCURRENT, NeuracleUtils.ConvertUAToDeviceValue(interPhaseCurrent));
            //Channel_enable 设置成需要刺激通道1
            switchDevice.WriteRegister(ElectricalStimulator.CHANNEL_ENABLE, 0b0001);
            //resistor_mode 置为 0
            switchDevice.WriteRegister(ElectricalStimulator.RESISTOR_MODE, 0);
            //Stim_start 置为 0，再置为 1，再置 0
            switchDevice.StartStimulate();
            //采集全打开
            //Switch_cref 置为 4 或 2
            switchDevice.WriteRegister(SwitchDevice.SwitchCref, 0x505);
            //Switch_adc 全部打开，就是对应的 bit 置 1
            switchDevice.OpenAllAdc();
        });
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
                        DataProcedure(_switchDeviceName);
                        Console.WriteLine("已经下发");
                        observer.OnNext(value);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(triggerObserver);
            });
    }
}

