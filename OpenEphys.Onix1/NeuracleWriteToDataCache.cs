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
    public IObservable<NeuracleHubDataFrame> Process(IObservable<NeuracleHubDataFrame> sources)
    {
        return sources.Do(
            neuracleHubDataFrame =>
            {
                MatCache<Mat>.AddToQueue(neuracleHubDataFrame.DeviceName, neuracleHubDataFrame.AmplifierData);
            }
        );
    }
}

