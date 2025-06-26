using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1;

[Combinator]
[WorkflowElementCategory(ElementCategory.Transform)]
public class WriteToDataCache
{

    [TypeConverter(typeof(Rhd2164.NameConverter))]
    [Description(SingleDeviceFactory.DeviceNameDescription)]
    [Category(DeviceFactory.ConfigurationCategory)]
    public string DeviceName { get; set; }

    public IObservable<Mat> Process(IObservable<Mat> sources)
    {

        return sources.Do(
            data =>
            {
                MatCache<Mat>.AddToQueue(DeviceName, data);
            }
        );
    }
}

