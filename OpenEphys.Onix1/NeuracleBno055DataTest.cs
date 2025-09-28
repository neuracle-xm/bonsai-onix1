using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("Bno055假数据测试")]
public class NeuracleBno055DataTest : Source<Bno055DataFrame>
{
    public override IObservable<Bno055DataFrame> Generate()
    {
        var r = new Random();
        var minValue = -0b100000000000000;
        var maxValue = 0b100000000000000;
        return Observable.Create<Bno055DataFrame>(observer =>
        {
            // 采样率100Hz
            return Observable.Interval(TimeSpan.FromSeconds(1.0 / 100))
                .Subscribe(_ =>
                {
                    Int16 w = (Int16)r.Next(minValue, maxValue);
                    Int16 x = (Int16)r.Next(minValue, maxValue);
                    Int16 y = (Int16)r.Next(minValue, maxValue);
                    Int16 z = (Int16)r.Next(minValue, maxValue);
                    // 只用四元数就行
                    Int16[] bno055Buffer = new Int16[] {
                        // 欧拉角
                        0, 0, 0, 
                        // 四元数
                        w, x, y, z,
                        // 线性加速度
                        0, 0, 0,
                        // 重力加速度
                        0, 0, 0,
                        // 温度
                        0, 
                        // 校准值
                        0, };
                    observer.OnNext(new Bno055DataFrame(
                       0, 0, bno055Buffer));
                }, observer.OnError, observer.OnCompleted);
        });
    }
}
