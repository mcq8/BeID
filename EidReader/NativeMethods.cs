using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Be.Mcq8.EidReader
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct SCardReaderState
    {
        public string ReaderName;
        private IntPtr UserData;
        public UInt32 CurrentState;
        public UInt32 EventState;
        public UInt32 AtrCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Atr;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SCardIORequest
    {
        public UInt32 Protocol;
        public UInt32 PciLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DevBroadcastDeviceinterface
    {
        internal int Size;
        internal int DeviceType;
        internal int Reserved;
        internal Guid ClassGuid;
        internal short Name;
    }

    internal class NativeMethods
    {
        private NativeMethods() { }

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardIsValidContext(IntPtr hContext);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardEstablishContext(UInt32 dwScope,
            IntPtr pvReserved1,
            IntPtr pvReserved2,
            IntPtr phContext);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardReleaseContext(IntPtr hContext);

        [DllImport("winscard.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int SCardListReaders(IntPtr hContext,
            [MarshalAs(UnmanagedType.LPTStr)] string mszGroups,
            IntPtr mszReaders,
            out UInt32 pcchReaders);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardGetStatusChange(IntPtr hContext,
            UInt32 dwTimeout,
            [In, Out] SCardReaderState[] rgReaderStates,
            UInt32 cReaders);

        [DllImport("winscard.dll", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int SCardConnect(IntPtr hContext,
            [MarshalAs(UnmanagedType.LPTStr)] string szReader,
            UInt32 dwShareMode,
            UInt32 dwPreferredProtocols,
            IntPtr phCard,
            IntPtr pdwActiveProtocol);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardTransmit(IntPtr hCard,
            [In] ref SCardIORequest pioSendPci,
            byte[] pbSendBuffer,
            UInt32 cbSendLength,
            IntPtr pioRecvPci,
            [Out] byte[] pbRecvBuffer,
            out UInt32 pcbRecvLength
            );

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardBeginTransaction(IntPtr hCard);

        [DllImport("winscard.dll", SetLastError = true)]
        internal static extern int SCardEndTransaction(IntPtr hCard, UInt32 dwDisposition);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterDeviceNotification(IntPtr handle);
    }
}
