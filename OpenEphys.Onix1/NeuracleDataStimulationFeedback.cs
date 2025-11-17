using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("只用于测试采集刺激闭环反馈延迟时间")]
public class NeuracleDataStimulationFeedback : Sink<bool>
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

    private uint _threshold = 0;
    /// <summary>
    /// 下发的阈值
    /// </summary>
    public uint Threshold
    {
        get
        {
            return _threshold;
        }
        set
        {
            _threshold = value;
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
                        DeviceManager.GetDevice(NeuracleFeedbackDeviceConfigure.FeedbackName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(FeedbackDevice));
                            //Console.WriteLine($"Address:{device.Address}");
                            //软复位
                            device.WriteRegister(FeedbackDevice.Reset, 0);
                            device.WriteRegister(FeedbackDevice.Reset, 1);
                            device.WriteRegister(FeedbackDevice.Threshold, 0);
                            device.WriteRegister(FeedbackDevice.Reset, 0);
                        });
                        DeviceManager.GetDevice(_switchDeviceName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(SwitchDevice));
                            device.WriteRegister(SwitchDevice.SwitchCref, 1028);
                            //通道0切到采集模式
                            device.Channel0ToData();
                            //通道1切换到刺激
                            device.Channel1ToStim();
                            //打通DAC开关
                            device.WriteRegister(SwitchDevice.SwitchNewDac, 0b00000000_00000000_00000000_01000000);
                            //开始切换
                            device.StartSwitch();
                        });
                        DeviceManager.GetDevice(_stimulationDeviceName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(ElectricalStimulator));
                            //下发刺激参数
                            device.WriteRegister(ElectricalStimulator.CH1BURSTCNT, 5);
                            device.WriteRegister(ElectricalStimulator.CH1BURSTINTERVAL, 1);
                            device.WriteRegister(ElectricalStimulator.CH1PHASEINTERVAL, 100);
                            device.WriteRegister(ElectricalStimulator.CH1PULSEINTERVAL, 500);
                            device.WriteRegister(ElectricalStimulator.CH1CURRENT1, NeuracleUtils.ConvertUAToDeviceValue(2000));
                            device.WriteRegister(ElectricalStimulator.CH1CURRENT2, NeuracleUtils.ConvertUAToDeviceValue(-2000));
                            device.WriteRegister(ElectricalStimulator.CH1PULSEDUR1, 1000);
                            device.WriteRegister(ElectricalStimulator.CH1PULSEDUR2, 1000);
                            device.WriteRegister(ElectricalStimulator.CH1TRAINDELAY, 1);
                            device.WriteRegister(ElectricalStimulator.CH1TRAINCNT, 1000);
                            device.WriteRegister(ElectricalStimulator.CH1RESTCURRENT, NeuracleUtils.ConvertUAToDeviceValue(500));
                            device.WriteRegister(ElectricalStimulator.CHANNEL_ENABLE, 1);
                            device.WriteRegister(ElectricalStimulator.RESISTOR_MODE, 0);
                        });
                        DeviceManager.GetDevice(NeuracleFeedbackDeviceConfigure.FeedbackName).Subscribe(x =>
                        {
                            var device = x.GetDeviceContext(typeof(FeedbackDevice));
                            //下发阈值
                            //Console.WriteLine($"Threshold:{Threshold}");
                            device.WriteRegister(FeedbackDevice.Threshold, Threshold);
                        });
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(triggerObserver);
            });
    }
}


