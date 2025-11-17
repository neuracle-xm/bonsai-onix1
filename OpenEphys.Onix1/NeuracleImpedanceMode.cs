using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Bonsai;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("切换成阻抗模式")]
public class NeuracleImpedanceMode : Sink<bool>
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

    private uint _channelIndex = 0;
    /// <summary>
    /// 选择哪个通道进行阻抗测量
    /// </summary>
    [Description("选择哪个通道查看阻抗")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public uint ChannelIndex
    {
        get { return _channelIndex; }
        set
        {
            _channelIndex = value;
            NeuracleGlobalState.ImpedanceChannelIndex = value;
        }
    }

    /// <summary>
    /// 阻抗下发命令的过程
    /// </summary>
    /// <param name="channelIndex">需要测的那个通道</param>
    /// <param name="stimulationDeviceName"></param>
    /// <param name="switchDeviceName"></param>
    private static void ImpedanceHelper(uint channelIndex, string stimulationDeviceName, string switchDeviceName)
    {
        //测(0 - 63通道)
        DeviceManager.GetDevice(switchDeviceName).Subscribe(deviceInfo =>
        {
            var switchDevice = deviceInfo.GetDeviceContext(typeof(SwitchDevice));
            //Switch_cref置为0
            switchDevice.WriteRegister(SwitchDevice.SwitchCref, 0);
            //Switch_adc全部关闭，全置0
            switchDevice.CloseAllAdc();
            switchDevice.SetImpedanceChannel(channelIndex);
            //开始切换
            switchDevice.StartSwitch();
        });
        DeviceManager.GetDevice(stimulationDeviceName).Subscribe(deviceInfo =>
        {
            var stimulationDevice = deviceInfo.GetDeviceContext(typeof(ElectricalStimulator));
            //刺激参数中Channel_enable置为0
            stimulationDevice.WriteRegister(ElectricalStimulator.CHANNEL_ENABLE, 15);
            //刺激参数中Ch1current1置为0.61mA
            stimulationDevice.WriteRegister(ElectricalStimulator.CH1CURRENT1, 37768);
            //刺激参数中Ch2current1置为0
            stimulationDevice.WriteRegister(ElectricalStimulator.CH2CURRENT1, 32768);
            //刺激参数中resistor_mode置为1
            stimulationDevice.WriteRegister(ElectricalStimulator.RESISTOR_MODE, 1);
        });
    }

    /// <summary>
    /// 阻抗测量的过程
    /// </summary>
    /// <param name="channelIndex"></param>
    /// <param name="stimulationDeviceName"></param>
    /// <param name="switchDeviceName"></param>
    public static void ImpedanceProcedure(uint channelIndex, string stimulationDeviceName, string switchDeviceName)
    {
        NeuracleGlobalState.ImpedanceChannelIndex = channelIndex;
        //先测的是配对通道的阻抗
        NeuracleGlobalState.IsPairImpedanceComplete = false;
        //设置需要检测的那个阻抗通道的配对通道
        var pairChannelIndex = SwitchWriteRegisterFunctions.GetPairChannelIndex(channelIndex);
        ImpedanceHelper(pairChannelIndex, stimulationDeviceName, switchDeviceName);
        if (NeuracleGlobalState.DeviceNameToHubName.TryGetValue(switchDeviceName, out var hubName))
        {
            NeuracleGlobalState.HubStates[hubName] = HubState.Impedance;
        }
        //等配对通道的阻抗计算完毕
        while (!NeuracleGlobalState.IsPairImpedanceComplete)
        {
            Thread.Sleep(1);
        }
        //再测真正的阻抗
        ImpedanceHelper(channelIndex, stimulationDeviceName, switchDeviceName);
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
                    ImpedanceProcedure(ChannelIndex, _stimulationDeviceName, _switchDeviceName);
                    var messageBox = new NeuracleMessageBox("切换到阻抗模式");
                    messageBox.Show();
                    observer.OnNext(value);
                },
                observer.OnError,
                observer.OnCompleted);
            return source.SubscribeSafe(triggerObserver);
        });
    }
}

