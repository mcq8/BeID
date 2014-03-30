using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Be.Mcq8.EidReader
{
    public class ReaderManager : NativeWindow
    {
        public event EventHandler<ReaderEventArgs> OnReaderConnected = null;
        public event EventHandler<ReaderEventArgs> OnReaderDisconnected = null;

        private Dictionary<String, Reader> readers;

        public ReaderManager()
        {
            this.CreateHandle(new CreateParams());
            readers = new Dictionary<string, Reader>();
            ReaderDetector.RegisterUsbDeviceNotification(this.Handle);
        }

        public void Init()
        {
            UpdateReaderList();
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var reader in readers)
                {
                    reader.Value.Dispose();
                }
                readers.Clear();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == ReaderDetector.WmDeviceChange &&
             ((int)m.WParam == ReaderDetector.DbtDeviceRemoveComplete || (int)m.WParam == ReaderDetector.DbtDeviceArrival))
            {
                UpdateReaderList();
            }
        }

        private void UpdateReaderList()
        {
            List<string> oldList = new List<string>(readers.Keys);
            List<string> newList = ListReaders();

            for (int i = 0; i < newList.Count; i++)
            {
                if (!readers.ContainsKey(newList[i]))
                {
                    Reader newReader = new Reader(newList[i]);
                    readers.Add(newList[i], newReader);
                    if (OnReaderConnected != null)
                    {
                        OnReaderConnected(this, new ReaderEventArgs(newReader));
                    }
                }
                oldList.Remove(newList[i]);
            }

            for (int i = 0; i < oldList.Count; i++)
            {
                if (OnReaderDisconnected != null)
                {
                    OnReaderDisconnected(this, new ReaderEventArgs(readers[oldList[i]]));
                }
                readers.Remove(oldList[i]);
            }
        }

        private List<string> ListReaders()
        {
            List<string> readers = new List<string>();
            UInt32 byteCount = 0;
            IntPtr context = Reader.EstablishContext(ReaderScope.User);

            if (NativeMethods.SCardListReaders(context, null, IntPtr.Zero, out byteCount) == (int)Scard.SCARD_S_SUCCESS)
            {
                IntPtr szListReaders = Marshal.AllocHGlobal((int)byteCount);
                if (NativeMethods.SCardListReaders(context, null, szListReaders, out byteCount) == (int)Scard.SCARD_S_SUCCESS)
                {
                    if (byteCount > 1)
                    {
                        byte[] caReadersData = new byte[byteCount];
                        Marshal.Copy(szListReaders, caReadersData, 0, (int)byteCount);

                        int start = 0;
                        int end;
                        while (start < byteCount - 1)
                        {
                            end = start;
                            while (end < byteCount && caReadersData[end] != 0)
                            {
                                end++;
                            }
                            readers.Add(Encoding.ASCII.GetString(caReadersData, start, end - start));
                            start = end + 1;
                        }
                    }
                }
                Marshal.FreeHGlobal(szListReaders);
            }
            Reader.ReleaseContext(context);
            return readers;
        }
    }
}