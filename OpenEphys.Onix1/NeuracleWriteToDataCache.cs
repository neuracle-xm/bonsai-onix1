using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("把采集数据写到缓存中")]
[Combinator]
[WorkflowElementCategory(ElementCategory.Transform)]
public class NeuracleWriteToDataCache
{
    [Description("写入的缓存名称")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string CacheName { get; set; }

    public IObservable<Mat> Process(IObservable<Mat> sources)
    {
        return sources.Do(
            data =>
            {
                MatCache<Mat>.AddToQueue(CacheName, data);
            }
        );
    }
}

