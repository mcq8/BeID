using System;
using System.Runtime.InteropServices;

namespace Be.Mcq8.EidReader
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct SCardReaderState
    {
        public string ReaderName;
        private IntPtr UserData;
        public IntPtr CurrentState;
        public IntPtr EventState;
        public IntPtr AtrCount;
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

    internal class LinuxNativeMethods
    {
        const string lib = "libpcsclite.so.1";

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardIsValidContext(IntPtr hContext);

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardEstablishContext(UInt32 dwScope,
            IntPtr pvReserved1,
            IntPtr pvReserved2,
            IntPtr phContext);

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardReleaseContext(IntPtr hContext);

        [DllImport(lib, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr SCardListReaders(
            [In] IntPtr hContext,
            [In] byte[] mszGroups,
            [Out] IntPtr mszReaders,
            out IntPtr pcchReaders);

        [DllImport(lib, SetLastError = true)]
        internal static extern IntPtr SCardGetStatusChange(IntPtr hContext,
            uint dwTimeout,
            [In, Out] SCardReaderState[] rgReaderStates,
            int cReaders);

        [DllImport(lib, SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int SCardConnect(IntPtr hContext,
            [MarshalAs(UnmanagedType.LPTStr)] string szReader,
            UInt32 dwShareMode,
            UInt32 dwPreferredProtocols,
            IntPtr phCard,
            IntPtr pdwActiveProtocol);

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardTransmit(IntPtr hCard,
            [In] ref SCardIORequest pioSendPci,
            byte[] pbSendBuffer,
            UInt32 cbSendLength,
            IntPtr pioRecvPci,
            [Out] byte[] pbRecvBuffer,
            out IntPtr pcbRecvLength
            );

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardBeginTransaction(IntPtr hCard);

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardEndTransaction(IntPtr hCard, UInt32 dwDisposition);
    }

    internal class WindowsNativeMethods
    {
        const string lib = "winscard.dll";
        internal static IntPtr EstablishContext(ReaderScope scope)
        {
            IntPtr context = IntPtr.Zero;
            IntPtr hContext = Marshal.AllocHGlobal(Marshal.SizeOf(context));
            int lastError = 0;
            try
            {
                /*
                ServiceController service = new ServiceController("SCardSvr");
                if (service.Status != ServiceControllerStatus.Running)
                {
                    throw new SCardSvrNotRunningException();
                }
                 */
                lastError = NativeMethods.SCardEstablishContext((uint)scope, IntPtr.Zero, IntPtr.Zero, hContext);
                if (lastError != (int)Scard.SCARD_S_SUCCESS)
                {
                    throw new ReaderException(null, "SCardEstablishContext error: " + lastError);
                }
                context = Marshal.ReadIntPtr(hContext);
            }
            finally
            {
                Marshal.FreeHGlobal(hContext);
            }
            return context;
        }

        internal static void ReleaseContext(IntPtr context)
        {
            if (NativeMethods.SCardIsValidContext(context) == (int)Scard.SCARD_S_SUCCESS)
            {
                int lastError = NativeMethods.SCardReleaseContext(context);
                if (lastError != (int)Scard.SCARD_S_SUCCESS)
                {
                    throw new ReaderException(null, "SCardReleaseContext error: " + lastError);
                }
            }
        }

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardIsValidContext(IntPtr hContext);

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardEstablishContext(UInt32 dwScope,
            IntPtr pvReserved1,
            IntPtr pvReserved2,
            IntPtr phContext);

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardReleaseContext(IntPtr hContext);

        [DllImport(lib, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr SCardListReaders(
            [In] IntPtr hContext,
            [In] byte[] mszGroups,
            [Out] IntPtr mszReaders,
            out IntPtr pcchReaders);

        [DllImport(lib, SetLastError = true)]
        internal static extern IntPtr SCardGetStatusChange(IntPtr hContext,
            uint dwTimeout,
            [In, Out] SCardReaderState[] rgReaderStates,
            int cReaders);

        [DllImport(lib, SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int SCardConnect(IntPtr hContext,
            [MarshalAs(UnmanagedType.LPTStr)] string szReader,
            UInt32 dwShareMode,
            UInt32 dwPreferredProtocols,
            IntPtr phCard,
            IntPtr pdwActiveProtocol);

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardTransmit(IntPtr hCard,
            [In] ref SCardIORequest pioSendPci,
            byte[] pbSendBuffer,
            UInt32 cbSendLength,
            IntPtr pioRecvPci,
            [Out] byte[] pbRecvBuffer,
            out IntPtr pcbRecvLength
            );

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardBeginTransaction(IntPtr hCard);

        [DllImport(lib, SetLastError = true)]
        internal static extern int SCardEndTransaction(IntPtr hCard, UInt32 dwDisposition);
    }

    internal class NativeMethods
    {
        internal static IntPtr EstablishContext(ReaderScope scope)
        {
            IntPtr context = IntPtr.Zero;
            IntPtr hContext = Marshal.AllocHGlobal(Marshal.SizeOf(context));
            int lastError = 0;
            try
            {
                lastError = NativeMethods.SCardEstablishContext((uint)scope, IntPtr.Zero, IntPtr.Zero, hContext);
                if (lastError != (int)Scard.SCARD_S_SUCCESS)
                {
                    throw new ReaderException(null, "SCardEstablishContext error: " + lastError);
                }
                context = Marshal.ReadIntPtr(hContext);
            }
            finally
            {
                Marshal.FreeHGlobal(hContext);
            }
            return context;
        }

        internal static void ReleaseContext(IntPtr context)
        {
            if (NativeMethods.SCardIsValidContext(context) == (int)Scard.SCARD_S_SUCCESS)
            {
                int lastError = NativeMethods.SCardReleaseContext(context);
                if (lastError != (int)Scard.SCARD_S_SUCCESS)
                {
                    throw new ReaderException(null, "SCardReleaseContext error: " + lastError);
                }
            }
        }
        internal static  int SCardIsValidContext(IntPtr hContext)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardIsValidContext(hContext);
            }
            else
            {
                return LinuxNativeMethods.SCardIsValidContext(hContext);
            }
        }

        internal static  int SCardEstablishContext(UInt32 dwScope,
            IntPtr pvReserved1,
            IntPtr pvReserved2,
            IntPtr phContext)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardEstablishContext(dwScope, pvReserved1, pvReserved2, phContext);
            }
            else
            {
                return LinuxNativeMethods.SCardEstablishContext(dwScope, pvReserved1, pvReserved2, phContext);
            }
        }

        internal static  int SCardReleaseContext(IntPtr hContext)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardReleaseContext(hContext);
            }
            else
            {
                return LinuxNativeMethods.SCardReleaseContext(hContext);
            }
        }

        public static  IntPtr SCardListReaders(
            [In] IntPtr hContext,
            [In] byte[] mszGroups,
            [Out] IntPtr mszReaders,
            out IntPtr pcchReaders)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardListReaders(hContext, mszGroups, mszReaders, out pcchReaders);
            }
            else
            {
                return LinuxNativeMethods.SCardListReaders(hContext, mszGroups, mszReaders, out pcchReaders);
            }
        }

        internal static  IntPtr SCardGetStatusChange(IntPtr hContext,
            uint dwTimeout,
            [In, Out] SCardReaderState[] rgReaderStates,
            int cReaders)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardGetStatusChange(hContext, dwTimeout, rgReaderStates, cReaders);
            }
            else
            {
                return LinuxNativeMethods.SCardGetStatusChange(hContext, dwTimeout, rgReaderStates, cReaders);
            }
        }

        internal static  int SCardConnect(IntPtr hContext,
            [MarshalAs(UnmanagedType.LPTStr)] string szReader,
            UInt32 dwShareMode,
            UInt32 dwPreferredProtocols,
            IntPtr phCard,
            IntPtr pdwActiveProtocol)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardConnect(hContext, szReader, dwShareMode, dwPreferredProtocols, phCard, pdwActiveProtocol);
            }
            else
            {
                return LinuxNativeMethods.SCardConnect(hContext, szReader, dwShareMode, dwPreferredProtocols, phCard, pdwActiveProtocol);
            }
        }

        internal static  int SCardTransmit(IntPtr hCard,
            [In] ref SCardIORequest pioSendPci,
            byte[] pbSendBuffer,
            UInt32 cbSendLength,
            IntPtr pioRecvPci,
            [Out] byte[] pbRecvBuffer,
            out IntPtr pcbRecvLength
            )
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardTransmit(hCard, ref pioSendPci, pbSendBuffer, cbSendLength, pioRecvPci, pbRecvBuffer, out pcbRecvLength);
            }
            else
            {
                return LinuxNativeMethods.SCardTransmit(hCard, ref pioSendPci, pbSendBuffer, cbSendLength, pioRecvPci, pbRecvBuffer, out pcbRecvLength);
            }
        }

        internal static  int SCardBeginTransaction(IntPtr hCard)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardBeginTransaction(hCard);
            }
            else
            {
                return LinuxNativeMethods.SCardBeginTransaction(hCard);
            }
        }

        internal static  int SCardEndTransaction(IntPtr hCard, UInt32 dwDisposition)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsNativeMethods.SCardEndTransaction(hCard, dwDisposition);
            }
            else
            {
                return LinuxNativeMethods.SCardEndTransaction(hCard, dwDisposition);
            }
        }
    }

}
