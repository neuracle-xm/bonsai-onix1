namespace OpenEphys.Onix1;

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
    public static void Start(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(SwitchDevice.SwitchStart, 0);
        deviceContext.WriteRegister(SwitchDevice.SwitchStart, 1);
        deviceContext.WriteRegister(SwitchDevice.SwitchStart, 0);
    }

    /// <summary>
    /// 开始刺激
    /// </summary>
    /// <param name="deviceContext"></param>
    public static void StartStimulate(this DeviceContext deviceContext)
    {
        deviceContext.WriteRegister(Headstage64ElectricalStimulator.STIM_START, 0);
        deviceContext.WriteRegister(Headstage64ElectricalStimulator.STIM_START, 1);
        deviceContext.WriteRegister(Headstage64ElectricalStimulator.STIM_START, 0);
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
    /// <param name="stiChIdx">用于刺激的通道(0-3)</param>
    public static void WriteStimulateChannel(this DeviceContext deviceContext, uint channel, uint stiChIdx)
    {
        uint mask = 0b10000000_00000000_00000000_00000000;
        uint channelIndex = channel % 4;
        uint targetIndex = channelIndex * 8 + stiChIdx;
        uint ch1value = mask >> (int)targetIndex;
        var registerAddress = SelectRegisterAddressWithChannel(channel);

        deviceContext.WriteRegister(registerAddress, ch1value);
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
