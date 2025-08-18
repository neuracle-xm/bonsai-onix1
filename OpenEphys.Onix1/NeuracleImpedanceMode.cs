using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
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
            if (GlobalState.HubNameToDeviceName.TryGetValue(_hubName, out var deviceTuple))
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
    /// 选择哪个通道进行阻抗测量
    /// </summary>
    [Description("选择哪个通道查看阻抗")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public uint ChannelIndex { get; set; } = 0;

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
                    if (GlobalState.DeviceNameToHubName.TryGetValue(_switchDeviceName, out var hubName))
                    {
                        GlobalState.HubStates[hubName] = HubState.Impedance;
                    }
                    //测(0 - 63通道)
                    DeviceManager.GetDevice(_switchDeviceName).Subscribe(deviceInfo =>
                    {
                        var switchDevice = deviceInfo.GetDeviceContext(typeof(SwitchDevice));
                        //Switch_cref置为0
                        switchDevice.WriteRegister(SwitchDevice.SwitchCref, 0);
                        //Switch_adc全部关闭，全置0
                        switchDevice.CloseAllAdc();
                        //设置需要检测的那个阻抗通道
                        switchDevice.SetImpedanceChannel(ChannelIndex);
                        //开始切换
                        switchDevice.StartSwitch();
                    });
                    DeviceManager.GetDevice(_stimulationDeviceName).Subscribe(deviceInfo =>
                    {
                        // TODO: 待完善公式
                        var stimulationDevice = deviceInfo.GetDeviceContext(typeof(ElectricalStimulator));
                        //刺激参数中Channel_enable置为0
                        stimulationDevice.WriteRegister(ElectricalStimulator.CHANNEL_ENABLE, 15);
                        //刺激参数中Ch1current1置为1mA(暂定)
                        stimulationDevice.WriteRegister(ElectricalStimulator.CH1CURRENT1, 37768);
                        //刺激参数中Ch2current1置为0
                        stimulationDevice.WriteRegister(ElectricalStimulator.CH2CURRENT1, 32768);
                        //刺激参数中resistor_mode置为1
                        stimulationDevice.WriteRegister(ElectricalStimulator.RESISTOR_MODE, 1);
                    });
                    ////内部测Cref(Cref1,Cref2)
                    //DeviceManager.GetDevice(SwitchDeviceName).Subscribe(deviceInfo =>
                    //{
                    //    var switchDevice = deviceInfo.GetDeviceContext(typeof(SwitchDevice));
                    //    //Switch_cref置为5(测Cref1)或3(测Cref2)
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchCref, 5);
                    //    //Switch_adc全部关闭，全置0
                    //    switchDevice.CloseAllAdc();
                    //    //Switch_dac中通道0的stimb的bit置为1，其他置为0
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac32_63, 0b01000000_00000000_00000000_00000000);
                    //    //其他Dac都关闭
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac0_31, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac64_95, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac96_127, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac128_159, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac160_191, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac192_223, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac224_255, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac256_287, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac288_319, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac320_351, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac352_383, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac384_415, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac416_447, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac448_479, 0);
                    //    switchDevice.WriteRegister(SwitchDevice.SwitchDac480_511, 0);
                    //    //开始切换
                    //    switchDevice.Start();
                    //});
                    //DeviceManager.GetDevice(StimulationDeviceName).Subscribe(deviceInfo =>
                    //{
                    //    var stimulationDevice = deviceInfo.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                    //    //刺激参数中Channel_enable置为0
                    //    stimulationDevice.WriteRegister(Headstage64ElectricalStimulator.CHANNEL_ENABLE, 0);
                    //    //刺激参数中Ch1current1置为1mA(暂定)
                    //    stimulationDevice.WriteRegister(Headstage64ElectricalStimulator.CH1CURRENT1, 1);
                    //    //刺激参数中Ch2current1置为0
                    //    stimulationDevice.WriteRegister(Headstage64ElectricalStimulator.CH2CURRENT1, 0);
                    //    //刺激参数中resistor_mode置为1
                    //    stimulationDevice.WriteRegister(Headstage64ElectricalStimulator.RESISTOR_MODE, 1);
                    //});
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

