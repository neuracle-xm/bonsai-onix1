using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1;

[WorkflowElementCategory(ElementCategory.Source)]
public class ReadDataCache : Source<Mat>
{

    [TypeConverter(typeof(Rhd2164.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string DeviceName { get; set; }

    /// <summary>  
    /// 读取间隔  
    /// </summary>  

    public override IObservable<Mat> Generate()
    {
        return Observable.Interval(TimeSpan.FromMilliseconds(15))
            .Select<long, Mat>(_ =>
            {
                if (MatCache<Mat>.DataQueueDict.TryGetValue(DeviceName, out var queue))
                {
                    if (queue.TryDequeue(out var item))
                    {
                        return item;
                    }
                }
                return Mat.Zeros(64, 3200, Depth.S32, 1);
            });
    }
}

