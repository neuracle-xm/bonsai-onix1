using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

[Combinator]
[Description("A custom transform that merge mat data.")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class MergeMat
{
    //因为接收的是CombineLatest的结果，所以这里需要匹配CombineLatest的参数个数
    public IObservable<Mat> Process(IObservable<Tuple<Mat, Mat>> sources)
    {
        return Observable.Create<Mat>(observer =>
        {
            return sources.Subscribe(
                mats =>
                {
                    Mat mergeMat = MergeMats(new List<Mat> { mats.Item1, mats.Item2 });
                    observer.OnNext(mergeMat);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }

    public IObservable<Mat> Process(IObservable<Tuple<Mat, Mat, Mat>> sources)
    {
        return Observable.Create<Mat>(observer =>
        {

            return sources.Subscribe(
                mats =>
                {
                    Mat mergeMat = MergeMats(new List<Mat> { mats.Item1, mats.Item2, mats.Item3 });
                    observer.OnNext(mergeMat);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }

    public IObservable<Mat> Process(IObservable<Tuple<Mat, Mat, Mat, Mat>> sources)
    {
        return Observable.Create<Mat>(observer =>
        {

            return sources.Subscribe(
                mats =>
                {
                    Mat mergeMat = MergeMats(new List<Mat> { mats.Item1, mats.Item2, mats.Item3, mats.Item4 });
                    observer.OnNext(mergeMat);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }

    public IObservable<Mat> Process(IObservable<Tuple<Mat, Mat, Mat, Mat, Mat>> sources)
    {
        return Observable.Create<Mat>(observer =>
        {

            return sources.Subscribe(
                mats =>
                {
                    Mat mergeMat = MergeMats(new List<Mat> { mats.Item1, mats.Item2, mats.Item3, mats.Item4, mats.Item5 });
                    observer.OnNext(mergeMat);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }

    public IObservable<Mat> Process(IObservable<Tuple<Mat, Mat, Mat, Mat, Mat, Mat>> sources)
    {
        return Observable.Create<Mat>(observer =>
        {

            return sources.Subscribe(
                mats =>
                {
                    Mat mergeMat = MergeMats(new List<Mat> { mats.Item1, mats.Item2, mats.Item3, mats.Item4, mats.Item5, mats.Item6 });
                    observer.OnNext(mergeMat);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }

    public IObservable<Mat> Process(IObservable<Tuple<Mat, Mat, Mat, Mat, Mat, Mat, Mat>> sources)
    {
        return Observable.Create<Mat>(observer =>
        {

            return sources.Subscribe(
                mats =>
                {
                    Mat mergeMat = MergeMats(new List<Mat> { mats.Item1, mats.Item2, mats.Item3, mats.Item4, mats.Item5, mats.Item6, mats.Item7 });
                    observer.OnNext(mergeMat);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }

    public IObservable<Mat> Process(IObservable<IEnumerable<Mat>> sources)
    {
        return Observable.Create<Mat>(observer =>
        {
            return sources.Subscribe(
                mats =>
                {
                    if (mats.Any())
                    {
                        Mat mergeMat = MergeMats(mats.ToList());
                        observer.OnNext(mergeMat);
                    }
                    else
                    {
                        //Debug.WriteLine("No Mat in queue");
                    }
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }

    //private static Stopwatch _stopwatch = new();

    private static Mat MergeMats(List<Mat> matList)
    {
        //_stopwatch.Restart();
        var totalRows = matList.Sum(m => m.Rows);
        var rows = matList.First().Rows;
        var cols = matList.First().Cols;
        var depth = matList.First().Depth;
        var channels = matList.First().Channels;
        var combined = new Mat(totalRows, cols, depth, channels);
        for (int i = 0; i < matList.Count; i++)
        {
            using var subRect = combined.GetRows(i * rows, (i + 1) * rows);
            CV.Copy(matList[i], subRect);
        }

        //Debug.WriteLine($"time:{_stopwatch.ElapsedMilliseconds}");
        return combined;
    }
}
