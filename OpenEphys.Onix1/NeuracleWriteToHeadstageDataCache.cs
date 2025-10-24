using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace NeuracleExtension;

[Description("把Headstage的采集数据写到缓存中")]
[Combinator]
[WorkflowElementCategory(ElementCategory.Transform)]
public class NeuracleWriteToHeadstageDataCache
{
    public IObservable<NeuracleHeadstageDataFrame> Process(IObservable<NeuracleHeadstageDataFrame> sources)
    {
        return sources.Do(
            neuracleHeadstageDataFrame =>
            {
                MatCache<Mat>.AddToQueue(neuracleHeadstageDataFrame.DeviceName, neuracleHeadstageDataFrame.AmplifierData);
            }
        );
    }
}

