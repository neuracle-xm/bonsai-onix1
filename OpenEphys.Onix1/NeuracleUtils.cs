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
}

