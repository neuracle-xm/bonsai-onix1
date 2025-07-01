using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("从缓存中读取数据")]
[WorkflowElementCategory(ElementCategory.Source)]
public class NeuracleReadDataCache : Source<Mat>
{
    [Description("需要读取的缓存的名称，和某个NeuracleWriteToDataCache对应")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string CacheName { get; set; }

    public override IObservable<Mat> Generate()
    {
        return Observable.Interval(TimeSpan.FromMilliseconds(15)).Select(_ =>
        {
            if (MatCache<Mat>.DataQueueDict.TryGetValue(CacheName, out var queue))
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

