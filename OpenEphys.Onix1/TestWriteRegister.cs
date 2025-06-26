namespace OpenEphys.Onix1;

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
