using System;

namespace NeuracleExtension;

/// <summary>
/// 放一些辅助函数
/// </summary>
public class NeuracleUtils
{
    /// <summary>
    /// 获取32位有符号整数的低16位有符号整数
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Int16 GetInt32LowInt16(Int32 x)
    {
        var bytes = BitConverter.GetBytes(x);
        var result = BitConverter.ToInt16(bytes, 0);
        return result;
    }

    /// <summary>
    /// 获取32位有符号整数的低16位无符号整数
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static UInt16 GetInt32LowUInt16(Int32 x)
    {
        var bytes = BitConverter.GetBytes(x);
        var result = BitConverter.ToUInt16(bytes, 0);
        return result;
    }

    /// <summary>
    /// 计算单个刺激通道的刺激时间
    /// </summary>
    /// <param name="trainDelay"></param>
    /// <param name="trainCnt"></param>
    /// <param name="burstCnt"></param>
    /// <param name="interPulseInterval"></param>
    /// <param name="interBurstInterval"></param>
    /// <param name="pulseDur1"></param>
    /// <param name="pulseDur2"></param>
    /// <param name="interPhaseInterval"></param>
    /// <returns></returns>
    public static int CalStimulationDuration(uint trainDelay, uint trainCnt, uint burstCnt, uint interPulseInterval, uint interBurstInterval, uint pulseDur1, uint pulseDur2, uint interPhaseInterval)
    {
        int CalPulseDuration(uint pulseDur1, uint pulseDur2, uint interPhaseInterval)
        {
            return (int)(pulseDur1 + interPhaseInterval + pulseDur2);
        }

        int CalBurstDuration(uint burstCnt, uint interPulseInterval, uint pulseDur1, uint pulseDur2, uint interPhaseInterval)
        {
            return (int)(burstCnt * CalPulseDuration(pulseDur1, pulseDur2, interPhaseInterval) + (burstCnt - 1) * interPulseInterval);
        }

        return (int)(CalBurstDuration(burstCnt, interPulseInterval, pulseDur1, pulseDur2, interPhaseInterval) * trainCnt + (trainCnt - 1) * interBurstInterval + trainDelay);
    }

    /// <summary>
    /// 将参数转换成适配本设备的参数
    /// 如果是单向波将对应的参数设置为0或1
    /// </summary>
    /// <param name="biPhasic"></param>
    /// <param name="phaseTwoCurrent"></param>
    /// <param name="interPhaseCurrent"></param>
    /// <param name="phaseTwoDuration"></param>
    /// <param name="interPhaseInterval"></param>
    /// <param name="interPulseInterval"></param>
    /// <returns></returns>
    public static Tuple<int, int, uint, uint, uint> ChangeParamByBiPhasic(bool biPhasic, int phaseTwoCurrent, int interPhaseCurrent, uint phaseTwoDuration, uint interPhaseInterval, uint interPulseInterval)
    {
        // 单向波参数适配
        if (!biPhasic)
        {
            phaseTwoCurrent = 0;
            interPhaseCurrent = 0;
            phaseTwoDuration = 1;
            interPhaseInterval = 1;
            interPulseInterval -= 2;
        }
        return Tuple.Create(phaseTwoCurrent, interPhaseCurrent, phaseTwoDuration, interPhaseInterval, interPulseInterval);
    }

    /// <summary>
    /// 验证刺激参数的有效性,将小于1的参数设置为1
    /// </summary>
    /// <param name="phaseOneDuration"></param>
    /// <param name="phaseTwoDuration"></param>
    /// <param name="interPhaseInterval"></param>
    /// <param name="interPulseInterval"></param>
    /// <param name="interBurstInterval"></param>
    /// <param name="triggerDelay"></param>
    /// <returns></returns>
    public static Tuple<uint, uint, uint, uint, uint, uint> ValidateDuration(uint phaseOneDuration, uint phaseTwoDuration, uint interPhaseInterval, uint interPulseInterval, uint interBurstInterval, uint triggerDelay)
    {
        uint phaseOneDurationValidated = Math.Max(phaseOneDuration, 1);
        uint phaseTwoDurationValidated = Math.Max(phaseTwoDuration, 1);
        uint interPhaseIntervalValidated = Math.Max(interPhaseInterval, 1);
        uint interPulseIntervalValidated = Math.Max(interPulseInterval, 1);
        uint interBurstIntervalValidated = Math.Max(interBurstInterval, 1);
        uint triggerDelayValidated = Math.Max(triggerDelay, 1);
        return Tuple.Create(phaseOneDurationValidated, phaseTwoDurationValidated, interPhaseIntervalValidated, interPulseIntervalValidated, interBurstIntervalValidated, triggerDelayValidated);
    }

    /// <summary>
    /// 将微安转换为设备值
    /// </summary>
    /// <param name="currentUA"></param>
    /// <returns></returns>
    public static uint ConvertUAToDeviceValue(int currentUA)
    {
        if (currentUA > 0)
        {
            return (uint)(currentUA / 4000.0f * 32767);
        }

        return (uint)((4000 + currentUA) * 32767.0f / 4000 + 32768);
    }

    /// <summary>
    /// Headstage将微安转换为设备值
    /// </summary>
    /// <param name="currentUA"></param>
    /// <returns></returns>
    public static uint HeadstageConvertUAToDeviceValue(int currentUA)
    {
        if (currentUA > 0)
        {
            return (uint)(currentUA / 2500.0f * 32767);
        }

        return (uint)((2500 + currentUA) * 32767.0f / 2500 + 32768);
    }

    /// <summary>
    /// 返回限制过后的电流大小
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public static int LimitedCurrent(int current)
    {
        if (current < -4000)
        {
            return -4000;
        }
        if (current > 4000)
        {
            return 4000;
        }
        return current;
    }

    /// <summary>
    /// 返回限制过后的电流大小
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public static int HeadstageLimitedCurrent(int current)
    {
        if (current < -2500)
        {
            return -2500;
        }
        if (current > 2500)
        {
            return 2500;
        }
        return current;
    }
}

