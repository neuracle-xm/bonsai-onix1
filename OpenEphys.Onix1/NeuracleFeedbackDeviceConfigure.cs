using System;
using System.ComponentModel;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Neuracle闭环反馈模式专用")]
public class NeuracleFeedbackDeviceConfigure : SingleDeviceFactory
{
    /// <summary>
    /// 这个设备的专用名字
    /// </summary>
    public const string FeedbackName = "NeuracleFeedbackDevice";

    public NeuracleFeedbackDeviceConfigure() : base(typeof(FeedbackDevice))
    {
    }

    public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
    {
        var deviceAddress = DeviceAddress;
        return source.ConfigureDevice(context =>
        {
            var device = context.GetDeviceContext(deviceAddress, DeviceType);
            return DeviceManager.RegisterDevice(FeedbackName, device, DeviceType);
        });
    }
}

public static class FeedbackDevice
{
    public const int ID = 5;

    /// <summary>
    /// 写阈值用的地址
    /// </summary>
    public const int Threshold = 1;

    /// <summary>
    /// 软复位的地址
    /// </summary>
    public const int Reset = 2;

    internal class NameConverter : DeviceNameConverter
    {
        public NameConverter() : base(typeof(FeedbackDevice))
        {
        }
    }
}

