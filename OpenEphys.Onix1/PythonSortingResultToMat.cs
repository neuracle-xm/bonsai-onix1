using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

[Combinator]
[Description("A custom transform that transform python sorting result to C# Mat type.")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class PythonSortingResultToMat
{
    //private static float[] _testSinData = new float[30000];

    //static PythonSortingResultToMat()
    //{
    //    int freq = 10;
    //    int sampling_rate = 30000;
    //    float t = 0;
    //    float delta_t = 1.0f / sampling_rate;
    //    for (int i = 0; i < 30000; i++)
    //    {
    //        _testSinData[i] = (float)(100 * Math.Sin(2 * Math.PI * freq * t));
    //        t += delta_t;
    //    }
    //}

    /// <summary>
    /// 当前发到哪个点了，一次只发buffer_size个大小的点
    /// </summary>
    //private int _index = 0;

    public IObservable<Mat> Process(IObservable<dynamic> source)
    {
        return Observable.Create<Mat>(observer =>
        {
            return source.Subscribe(
                result =>
                {
                    var data = result["data"];
                    //var pyBytes = data.InvokeMethod("tobytes");
                    //byte[] buffer = pyBytes.As<byte[]>();
                    var address = (long)result["address"];
                    var rows = (int)result["rows"];
                    var cols = (int)result["cols"];
                    var mat = new Mat(rows, cols, Depth.F32, 1, (IntPtr)address);
                    //var mat = Mat.Zeros(rows, cols, Depth.F32, 1);
                    //mat.SetReal(0, 100, -150);
                    //var firstValue = mat.GetReal(0, 0);
                    //if (firstValue == 0)
                    //{
                    //    for (int i = 0; i < rows; i++)
                    //    {
                    //        for (int j = 0; j < cols; j++)
                    //        {
                    //            var v = mat.GetReal(i, j);
                    //            if (v < 0)
                    //            {
                    //                Debug.WriteLine($"i:{i},j:{j},value:{v}");
                    //            }
                    //        }
                    //    }
                    //}
                    //测试用
                    //var r = new Random();
                    //for (int i = 0; i < rows; i++)
                    //{
                    //    for (int j = 0; j < cols; j++)
                    //    {
                    //        if (r.Next(0, 2) == 0)
                    //        {
                    //            floatArray[i, j] = -150;
                    //        }
                    //        else
                    //        {
                    //            floatArray[i, j] = -50;
                    //        }
                    //    }
                    //}
                    //var mat = Mat.FromArray(floatArray);
                    //var mat = Mat.Ones(rows, cols, Depth.F32, 1) * 100;
                    //var matCopy = new Mat(rows, cols, Depth.F32, 1);
                    //CV.Copy(mat, matCopy);
                    observer.OnNext(mat);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }


}

