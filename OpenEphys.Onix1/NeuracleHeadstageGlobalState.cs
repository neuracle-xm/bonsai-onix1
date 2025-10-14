namespace NeuracleExtension;

/// <summary>
/// 放一些Headstage共享的状态
/// </summary>
public class NeuracleHeadstageGlobalState
{
    /// <summary>
    /// 激励电流,单位nA
    /// </summary>
    public const float ExcitationCurrent = 38.0f;

    /// <summary>
    /// 测阻抗时累计的点数
    /// </summary>
    public const int ImpedanceBufferSize = 25000;

    /// <summary>
    /// 计算阻抗时跳过的点数
    /// </summary>
    public const int ImpedanceBufferSkipPoints = 5000;

    /// <summary>
    /// 滑窗计算平均电压时的窗长
    /// </summary>
    public const int WindowLength = 3200;

    /// <summary>
    /// 找最小值时用的点数
    /// </summary>
    public const int MinValueBufferRange = 800;

    /// <summary>
    /// FIR滤波器系数
    /// </summary>
    public static float[] bCoefficient = new float[] {
        0.00885542081476936f,
        -8.89952523603937e-18f,
        -0.0128595779236281f,
        9.46808654273140e-18f,
        0.0242171040481493f,
        -1.13975481486055e-17f,
        -0.0413482270019456f,
        3.60841232666345e-17f,
        0.0617129152132297f,
        -2.52340312039069e-17f,
        -0.0822097012008933f,
        1.27514178754475e-16f,
        0.0996716440146697f,
        7.39280702841398e-17f,
        -0.111377796278782f,
        0,
        0.115495227007866f,
        0,
        -0.111377796278782f,
        7.39280702841398e-17f,
        0.0996716440146697f,
        1.27514178754475e-16f,
        -0.0822097012008933f,
        -2.52340312039069e-17f,
        0.0617129152132297f,
        3.60841232666345e-17f,
        -0.0413482270019456f,
        -1.13975481486055e-17f,
        0.0242171040481493f,
        9.46808654273140e-18f,
        -0.0128595779236281f,
        -8.89952523603937e-18f,
        0.00885542081476936f };

    /// <summary>
    /// Headstage当前的状态
    /// </summary>
    public static HeadstageState HeadstageState { get; set; } = HeadstageState.Data;

    /// <summary>
    /// 当前查看哪个阻抗通道
    /// </summary>
    public static uint ImpedanceChannelIndex { get; set; } = 0;

    /// <summary>
    /// 带通滤波
    /// </summary>
    /// <param name="voltageArray"></param>
    /// <returns></returns>
    public static float[] FirFilt(int[] voltageArray)
    {
        var result = new float[voltageArray.Length];
        for (int i = 0; i < result.Length; i++)
        {
            float y = 0;
            for (int j = 0; j < bCoefficient.Length; j++)
            {
                if (j > i)
                {
                    break;
                }
                y += bCoefficient[j] * voltageArray[i - j];
            }
            result[i] = y;
        }
        return result;
    }

    /// <summary>
    /// 跳过一些点
    /// </summary>
    /// <param name="filterdArray"></param>
    /// <returns></returns>
    public static float[] SkipPoints(float[] filterdArray)
    {
        var result = new float[filterdArray.Length - ImpedanceBufferSkipPoints];
        for (int i = ImpedanceBufferSkipPoints; i < filterdArray.Length; i++)
        {
            result[i - ImpedanceBufferSkipPoints] = filterdArray[i];
        }
        return result;
    }

    /// <summary>
    /// 找一个窗内的峰峰值
    /// </summary>
    /// <param name="array"></param>
    /// <param name="startIndex"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static float FindPeakToPeak(float[] array, int startIndex, int length)
    {
        float maxValue = float.NegativeInfinity;
        int maxValueIndex = -1;
        for (int i = 0; i < length; i++)
        {
            var currentValue = array[startIndex + i];
            if (currentValue > maxValue)
            {
                maxValue = currentValue;
                maxValueIndex = i;
            }
        }
        float minValue = float.PositiveInfinity;
        //从该最大值后找最小值(不包括该最大值)
        if (maxValueIndex < MinValueBufferRange)
        {
            for (int i = 0; i < MinValueBufferRange; i++)
            {
                var currentValue = array[maxValueIndex + i + 1];
                if (currentValue < minValue)
                {
                    minValue = currentValue;
                }
            }
        }
        //从该最大值前找最小值(不包括该最大值)
        else
        {
            for (int i = 0; i < MinValueBufferRange; i++)
            {
                var currentValue = array[maxValueIndex - i - 1];
                if (currentValue < minValue)
                {
                    minValue = currentValue;
                }
            }
        }
        return maxValue - minValue;
    }

    /// <summary>
    /// 参考W3-SDK阻抗计算方法:
    /// 计算平均电压，单位uV
    /// 滑窗计算，窗长16，
    /// 每个窗里找出16个点里最大数Max，
    /// 如果最大数的位置出现在滑窗16个点里前4个点，取该点往后的4个点，否则取该点往前的4个点(都不包括该点)
    /// 算出这4个点的最小值Min，每个窗算一个峰峰值为Max-Min。
    /// 将每个窗的峰峰值汇总再算一次平均后得出的平均电压
    /// </summary>
    /// <param name="restArray"></param>
    /// <returns></returns>
    public static float MeanVoltage(float[] restArray)
    {
        float peakToPeakSum = 0;
        float peakToPeakCount = 0;
        //去掉最后多余不够一个窗的点
        var remainder = restArray.Length % WindowLength;
        for (int i = 0; i < restArray.Length - remainder; i += WindowLength)
        {
            var peakToPeak = FindPeakToPeak(restArray, i, WindowLength);
            peakToPeakSum += peakToPeak;
            peakToPeakCount += 1;
        }
        return peakToPeakSum / peakToPeakCount;
    }

    /// <summary>
    /// 计算阻抗的过程,单位Ω
    /// </summary>
    /// <param name="voltageArray"></param>
    /// <returns></returns>
    public static float ComputeImpedance(int[] voltageArray)
    {
        var filterdArray = FirFilt(voltageArray);
        var restArray = SkipPoints(filterdArray);
        var meanVoltage = MeanVoltage(restArray);
        var impedanceValue = meanVoltage / ExcitationCurrent * 1000;
        return impedanceValue;
    }
}

/// <summary>
/// Headstage当前处于什么状态
/// </summary>
public enum HeadstageState
{
    /// <summary>
    /// 采集模式
    /// </summary>
    Data,

    /// <summary>
    /// 阻抗模式
    /// </summary>
    Impedance
}
