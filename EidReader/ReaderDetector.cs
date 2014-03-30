using System;
using System.Runtime.InteropServices;

namespace Be.Mcq8.EidReader
{
    internal static class ReaderDetector
    {
        public const int DbtDeviceArrival = 0x8000;
        public const int DbtDeviceRemoveComplete = 0x8004;
        public const int WmDeviceChange = 0x0219;
        private const int DbtDevtypDeviceinterface = 5;
        private static readonly Guid GuidSmartCardReaderGUID = new Guid("50dd5230-ba8a-11d1-bf5d-0000f805f530");
        private static IntPtr notificationHandle;

        /// <summary>
        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Handle to the window receiving notifications.</param>
        public static void RegisterUsbDeviceNotification(IntPtr windowHandle)
        {
            DevBroadcastDeviceinterface dbi = new DevBroadcastDeviceinterface
            {
                DeviceType = DbtDevtypDeviceinterface,
                Reserved = 0,
                ClassGuid = GuidSmartCardReaderGUID,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            Marshal.StructureToPtr(dbi, buffer, true);

            notificationHandle = NativeMethods.RegisterDeviceNotification(windowHandle, buffer, 0);
        }

        /// <summary>
        /// Unregisters the window for USB device notifications
        /// </summary>
        public static void UnregisterUsbDeviceNotification()
        {
            NativeMethods.UnregisterDeviceNotification(notificationHandle);
        }
    }
}