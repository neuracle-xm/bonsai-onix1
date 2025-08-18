using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;
using OpenEphys.Onix1;

namespace NeuracleExtension;

[Description("模拟数据")]
public class NeuracleSimulationData : Source<NeuracleHubDataFrame>
{
    private static float[,] _example_data;
    private static int _col;

    static NeuracleSimulationData()
    {
        string csvPath = "./unit_100_channel_64_secs_10.csv";
        string[] lines = File.ReadAllLines(csvPath);
        int row = lines.Length;
        _col = lines[0].Split(',').Length;
        _example_data = new float[Rhd2164.AmplifierChannelCount, _col];
        for (int i = 0; i < Rhd2164.AmplifierChannelCount; i++)
        {
            string[] values = lines[i % row].Split(',');
            for (int j = 0; j < _col; j++)
            {
                _example_data[i, j] = float.Parse(values[j]);
            }
        }
    }

    /// <summary>
    /// 当前发到哪一个点了
    /// </summary>
    private int _index = 0;

    [Description("缓存的帧大小")]
    [Category(DeviceFactory.ConfigurationCategory)]
    public int BufferSize { get; set; } = GlobalState.BufferSize;

    /// <summary>
    /// 生成测试用的方波
    /// </summary>
    /// <returns></returns>
    private float[,] GenerateSquareWave()
    {
        var middleIndex = BufferSize / 2;
        var result = new float[Rhd2164.AmplifierChannelCount, BufferSize];
        for (int row = 0; row < Rhd2164.AmplifierChannelCount; row++)
        {
            for (int col = 0; col < middleIndex; col++)
            {
                result[row, col] = 150;
            }
            for (int col = middleIndex; col < BufferSize; col++)
            {
                result[row, col] = -150;
            }
            //result[row, 150] = -150;
        }
        return result;
    }

    /// <summary>
    /// 生成一个一维的数组，数据按通道顺序排列
    /// </summary>
    /// <returns></returns>
    private float[] GenerateOneDimensionSquareWave()
    {
        float[] result = new float[BufferSize * Rhd2164.AmplifierChannelCount];
        var middle = BufferSize / 2;
        for (int channel = 0; channel < Rhd2164.AmplifierChannelCount; channel++)
        {
            for (int index = channel * BufferSize; index < channel * BufferSize + middle; index++)
            {
                result[index] = 150;
            }
            for (int index = channel * BufferSize + middle; index < (channel + 1) * BufferSize; index++)
            {
                result[index] = -150;
            }
        }
        return result;
    }

    /// <summary>
    /// Generates a sequence of <see cref="Rhd2164DataFrame"/> objects, each of which are a buffered set of multichannel samples an Rhd2164 device.
    /// </summary>
    /// <returns>A sequence of <see cref="Rhd2164DataFrame"/> objects.</returns>
    public unsafe override IObservable<NeuracleHubDataFrame> Generate()
    {
        var bufferSize = BufferSize;
        return Observable.Create<NeuracleHubDataFrame>(observer =>
        {
            var hubClockBuffer = new ulong[bufferSize];
            var clockBuffer = new ulong[bufferSize];
            return Observable.Interval(TimeSpan.FromSeconds(1.0 / 20))
                .Subscribe(_ =>
                {
                    float[,] ampliferArray = new float[NeuracleData.AmplifierChannelCount, bufferSize];
                    for (int col = 0; col < bufferSize; col++)
                    {
                        for (int row = 0; row < NeuracleData.AmplifierChannelCount; row++)
                        {
                            ampliferArray[row, col] = _example_data[row, _index];
                        }
                        _index++;
                        if (_index >= _col)
                        {
                            _index = 0;
                        }
                    }
                    //var ampliferArray = GenerateSquareWave();
                    //var ampliferArray = GenerateOneDimensionSquareWave();
                    //var data = BufferHelper.CopyTranspose(ampliferArray, bufferSize, Rhd2164.AmplifierChannelCount, Depth.F32);
                    //数据的形状是(BufferSize,ChannelCount)
                    observer.OnNext(new NeuracleHubDataFrame(
                        "test",
                        clockBuffer,
                        hubClockBuffer,
                        Mat.FromArray(ampliferArray),
                        float.PositiveInfinity, float.PositiveInfinity));
                }, observer.OnError, observer.OnCompleted);
        });
    }
}
