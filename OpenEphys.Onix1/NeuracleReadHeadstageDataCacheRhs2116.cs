using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("从NeuracleHeadstageRhs2116的缓存中读取数据")]
[WorkflowElementCategory(ElementCategory.Source)]
public class NeuracleReadHeadstageDataCacheRhs2116 : Source<Mat>
{
    [TypeConverter(typeof(NeuracleHeadstageDataRhs2116.NameConverter))]
    [Description("需要读取的Headstage的名称")]
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
            return Mat.Zeros(NeuracleHeadstageGlobalState.ChannelNumber, 1, Depth.F32, 1);
        });
    }
}

