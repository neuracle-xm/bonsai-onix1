using System;
using System.Reflection;
using oni;

namespace OpenEphys.Onix1
{
    static class ContextHelper
    {
        public static DeviceContext GetDeviceContext(this ContextTask context, uint address, Type expectedType)
        {
            if (!context.DeviceTable.TryGetValue(address, out Device device))
            {
                ThrowDeviceNotFoundException(expectedType, address);
            }

            if (device.ID != GetDeviceID(expectedType))
            {
                ThrowInvalidDeviceException(expectedType, address);
            }

            return new DeviceContext(context, device);
        }

        public static DeviceContext GetDeviceContext(this DeviceInfo deviceInfo, Type expectedType)
        {
            deviceInfo.AssertType(expectedType);
            if (!deviceInfo.Context.DeviceTable.TryGetValue(deviceInfo.DeviceAddress, out Device device))
            {
                ThrowDeviceNotFoundException(expectedType, deviceInfo.DeviceAddress);
            }

            return new DeviceContext(deviceInfo.Context, device);
        }

        public static DeviceContext GetPassthroughDeviceContext(this ContextTask context, uint address, Type expectedType)
        {
            var passthroughDeviceAddress = context.GetPassthroughDeviceAddress(address);
            return GetDeviceContext(context, passthroughDeviceAddress, expectedType);
        }

        public static DeviceContext GetPassthroughDeviceContext(this DeviceContext device, Type expectedType)
        {
            return GetPassthroughDeviceContext(device.Context, device.Address, expectedType);
        }

        static int GetDeviceID(Type deviceType)
        {
            var fieldInfo = deviceType.GetField(
                "ID",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.IgnoreCase);
            if (fieldInfo == null || !fieldInfo.IsLiteral)
            {
                throw new ArgumentException($"The specified device type {deviceType} does not have a const ID field.", nameof(deviceType));
            }

            return (int)fieldInfo.GetRawConstantValue();
        }

        static void ThrowDeviceNotFoundException(Type expectedType, uint address)
        {
            throw new InvalidOperationException($"Device '{expectedType.Name}' was not found in the device table at address {address}.");
        }

        static void ThrowInvalidDeviceException(Type expectedType, uint address)
        {
            throw new InvalidOperationException($"Invalid device ID. The device found at address {address} is not a '{expectedType.Name}' device.");
        }

        /// <summary>
        /// 遍历context中的DeviceTable,自动根据ID来找到DeviceAddress
        /// </summary>
        /// <param name="context"></param>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        public static uint? GetAddressByID(this ContextTask context, int deviceID)
        {
            uint? deviceAddress = null;
            foreach (var item in context.DeviceTable)
            {
                if (item.Value.ID == deviceID)
                {
                    deviceAddress = item.Key;
                    break;
                }
            }
            return deviceAddress;
        }
    }
}
