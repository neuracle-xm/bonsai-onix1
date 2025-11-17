using System;
using OpenEphys.Onix1;

namespace NeuracleExtension;

/// <summary>
/// 模拟开关的一些扩展方法
/// </summary>
public static class SwitchWriteRegisterFunctions
{
    /// <summary>
    /// 打开所有Adc通道
    /// </summary>
    /// <param name="deviceContext"></param>
    public static void OpenAllAdc(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc0_31, 0b11110000_11110000_11110000_11110000);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc32_63, 0b11110000_11110000_11110000_11110000);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc64_95, 0b11110000_11110000_11110000_11110000);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc96_127, 0b11110000_11110000_11110000_11110000);
    }

    /// <summary>
    /// 关闭所有Adc通道
    /// </summary>
    /// <param name="deviceContext"></param>
    public static void CloseAllAdc(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc0_31, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc32_63, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc64_95, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc96_127, 0);
    }

    /// <summary>
    /// 关闭所有Dac通道
    /// </summary>
    /// <param name="deviceContext"></param>
    public static void CloseAllDac(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac0_31, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac32_63, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac64_95, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac96_127, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac128_159, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac160_191, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac192_223, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac224_255, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac256_287, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac288_319, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac320_351, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac352_383, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac384_415, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac416_447, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac448_479, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac480_511, 0);
    }

    /// <summary>
    /// 切换开关
    /// </summary>
    /// <param name="deviceContext"></param>
    public static void StartSwitch(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchStart, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchStart, 1);
        deviceContext.WriteRegister(SwitchDevice.SwitchStart, 0);
    }

    /// <summary>
    /// 获取与被测阻抗通道的配对的那个通道
    /// 0 -> 1
    /// 1 -> 0
    /// 2 -> 3
    /// 3 -> 2
    /// 依次类推
    /// </summary>
    /// <param name="channelIndex"></param>
    /// <returns></returns>
    public static uint GetPairChannelIndex(uint channelIndex)
    {
        //偶数的配对通道就+1
        if (channelIndex % 2 == 0)
        {
            return channelIndex + 1;
        }
        //奇数就减1
        return channelIndex - 1;
    }

    /// <summary>
    /// 选择某个阻抗通道进行测量
    /// </summary>
    /// <param name="deviceContext"></param>
    /// <param name="channelIndex"></param>
    public static void SetImpedanceChannel(this DeviceContext deviceContext, uint channelIndex)
    {
        //先关闭所有Dac
        deviceContext.CloseAllDac();
        //Switch_dac中对应通道(0 - 63中的某个)的stima的bit置为1，其他置为0
        //计算这个通道在这个地址上是第几个位置
        var currentChannelIndexInAddress = 3 - channelIndex % 4;
        var pairChannelIndex = GetPairChannelIndex(channelIndex);
        var pairChannelIndexInAddress = 3 - pairChannelIndex % 4;
        //找到这俩电极的stim输出通道，currentChannel用stima，pairChannel用stimb
        uint stimaIndex;
        uint stimbIndex;
        //奇数时stima和stimb都在最后一个bit
        if (currentChannelIndexInAddress % 2 == 1)
        {
            stimaIndex = currentChannelIndexInAddress * 8 + 7;
            stimbIndex = pairChannelIndexInAddress * 8 + 7;
        }
        //偶数时stima和stimb都在倒数第二个bit
        else
        {
            stimaIndex = currentChannelIndexInAddress * 8 + 6;
            stimbIndex = pairChannelIndexInAddress * 8 + 6;
        }
        //最终需要写入的值
        uint writeValue = (uint)(Math.Pow(2, stimaIndex) + Math.Pow(2, stimbIndex));
        //找到这个通道对应的地址
        var address = SelectRegisterAddressWithChannel(channelIndex);
        //把stima和stimb打开
        deviceContext.WriteRegister(SwitchDevice.SwitchNewDac, 0b00000000_00000000_01000000_01000000);
        deviceContext.WriteRegister(address, writeValue);
    }

    /// <summary>
    /// 开始刺激
    /// </summary>
    /// <param name="deviceContext"></param>
    public static void StartStimulate(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(ElectricalStimulator.STIM_START, 0);
        deviceContext.WriteRegister(ElectricalStimulator.STIM_START, 1);
        deviceContext.WriteRegister(ElectricalStimulator.STIM_START, 0);
    }

    /// <summary>
    /// 根据通道号选择寄存器
    /// </summary>
    /// <param name="channel"></param>
    public static uint SelectRegisterAddressWithChannel(uint channel)
    {
        if (channel < 4)
        {
            return SwitchDevice.SwitchDac32_63;
        }
        else if (channel < 8)
        {
            return SwitchDevice.SwitchDac0_31;
        }
        else if (channel < 12)
        {
            return SwitchDevice.SwitchDac96_127;
        }
        else if (channel < 16)
        {
            return SwitchDevice.SwitchDac64_95;
        }
        else if (channel < 20)
        {
            return SwitchDevice.SwitchDac160_191;
        }
        else if (channel < 24)
        {
            return SwitchDevice.SwitchDac128_159;
        }
        else if (channel < 28)
        {
            return SwitchDevice.SwitchDac224_255;
        }
        else if (channel < 32)
        {
            return SwitchDevice.SwitchDac192_223;
        }
        else if (channel < 36)
        {
            return SwitchDevice.SwitchDac288_319;
        }
        else if (channel < 40)
        {
            return SwitchDevice.SwitchDac256_287;
        }
        else if (channel < 44)
        {
            return SwitchDevice.SwitchDac352_383;
        }
        else if (channel < 48)
        {
            return SwitchDevice.SwitchDac320_351;
        }
        else if (channel < 52)
        {
            return SwitchDevice.SwitchDac416_447;
        }
        else if (channel < 56)
        {
            return SwitchDevice.SwitchDac384_415;
        }
        else if (channel < 60)
        {
            return SwitchDevice.SwitchDac480_511;
        }
        else if (channel < 64)
        {
            return SwitchDevice.SwitchDac448_479;
        }
        else
        {
            return 100;
        }
    }

    /// <summary>
    /// 选择刺激通道和被刺激通道
    /// </summary>
    /// <param name="deviceContext"></param>
    /// <param name="channel">被刺激通道（0-63）</param>
    /// <param name="stiChIdx">用于刺激的通道(0-3) 对应abcd</param>
    public static uint GetWriteValue(uint channel, uint stiChIdx)
    {
        // 奇数通道时，stim由高至低为 badc
        // 偶数通道时，stim由高至低为 abcd
        // 所以如果是奇数通道，stiChIdx需要调整一下
        if (channel % 2 == 1)
        {
            if (stiChIdx == 0 || stiChIdx == 2)
            {
                stiChIdx++;
            }
            else if (stiChIdx == 1 || stiChIdx == 3)
            {
                stiChIdx--;
            }
        }
        uint mask = 0b10000000_00000000_00000000_00000000;
        uint channelIndex = channel % 4;
        uint targetIndex = channelIndex * 8 + stiChIdx;
        uint writeValue = mask >> (int)targetIndex;
        return writeValue;
    }

    /// <summary>
    /// 通道0切到采集模式
    /// </summary>
    /// <param name="deviceContext"></param>
    public static void Channel0ToData(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc0_31, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc32_63, 0b10000000_00000000_00000000_00000000);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc64_95, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc96_127, 0);
    }

    /// <summary>
    /// 通道1切到刺激模式
    /// </summary>
    /// <param name="deviceContext"></param>
    public static void Channel1ToStim(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac0_31, 0);
        //使用通道1的stima
        deviceContext.WriteRegister(SwitchDevice.SwitchDac32_63, 0b00000000_01000000_00000000_00000000);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac64_95, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac96_127, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac128_159, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac160_191, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac192_223, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac224_255, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac256_287, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac288_319, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac320_351, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac352_383, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac384_415, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac416_447, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac448_479, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchDac480_511, 0);
    }
}

/// <summary>
/// 这里放各种用于测试写寄存器是否正确的测试函数，都写成扩展方法
/// </summary>
public static class TestSwitchCrefWriteRegister
{
    public static void TestCref1(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchCref, 4);
    }

    public static void TestCref2(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchCref, 2);
    }

    public static void TestStima(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchCref, 1);
    }

    public static void TestCref1_Stima(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchCref, 5);
    }

    public static void TestCref2_Stima(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchCref, 3);
    }
}

public static class TestSwitchAdcWriteRegister
{
    public static void TestChannel0(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc32_63, 0b10000000_00000000_00000000_0000000);
    }

    public static void TestChannel1(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc32_63, 0b01000000_00000000_00000000_0000000);
    }

    public static void TestCloseAll(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc0_31, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc32_63, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc64_95, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc96_127, 0);
    }

    public static void TestOpenAll(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc0_31, 0b11110000_11110000_11110000_11110000);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc32_63, 0b11110000_11110000_11110000_11110000);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc64_95, 0b11110000_11110000_11110000_11110000);
        deviceContext.WriteRegister(SwitchDevice.SwitchAdc96_127, 0b11110000_11110000_11110000_11110000);
    }
}

public static class TestSwitchDacWriteRegister
{
    public static void TestChannel2Stima(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac32_63, 0b00000000_00000000_10000000_00000000);
    }

    public static void TestChannel2Stimb(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac32_63, 0b00000000_00000000_01000000_00000000);
    }

    public static void TestChannel2Stimc(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac32_63, 0b00000000_00000000_00100000_00000000);
    }

    public static void TestChannel2Stimd(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac32_63, 0b00000000_00000000_00010000_00000000);
    }

    public static void TestChannel9Stima(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac96_127, 0b00000000_00000000_00000010_00000000);
    }

    public static void TestChannel9Stimb(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac96_127, 0b00000000_00000000_00000001_00000000);
    }

    public static void TestChannel9Stimc(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac96_127, 0b00000000_00000000_00001000_00000000);
    }

    public static void TestChannel9Stimd(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchDac96_127, 0b00000000_00000000_00000100_00000000);
    }
}
