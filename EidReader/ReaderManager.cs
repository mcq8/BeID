using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Be.Mcq8.EidReader
{
    public class ReaderManager : IDisposable
    {
        public event EventHandler<ReaderEventArgs> OnReaderConnected = null;
        public event EventHandler<ReaderEventArgs> OnReaderDisconnected = null;

        private BackgroundWorker backgroundWorker1;

        private Dictionary<String, Reader> readers;

        private const uint WAIT_TIME = 250;


        public ReaderManager()
        {
            readers = new Dictionary<string, Reader>();
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.WorkerReportsProgress = true;

        }
        ~ReaderManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                OnReaderConnected = null;
                OnReaderDisconnected = null;
            }
        }

        private void ReaderManager_UpdateCards(object sender, EventArgs e)
        {
            UpdateReaderList();
        }

        protected void RunCardDetection()
        {

            try
            {
                IntPtr lastError;
                var contextPointer = NativeMethods.EstablishContext(ReaderScope.User);
                SCardReaderState[] readerState = new SCardReaderState[1];
                readerState[0].CurrentState = (IntPtr)CardState.UNAWARE;
                readerState[0].ReaderName = "\\\\?PnP?\\Notification";

                while (true)
                {
                    lastError = NativeMethods.SCardGetStatusChange(contextPointer, 0xFFFFFFFF, readerState, 1);
                    if ((uint)lastError == 0x8010001e)
                    {
                        Thread.Sleep(100);
                        contextPointer = NativeMethods.EstablishContext(ReaderScope.User);
                        continue;
                    }
                    UpdateReaderList();
                    readerState[0].CurrentState = (IntPtr)(readers.Count << 16);
                }

                NativeMethods.ReleaseContext(contextPointer);

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        public void Init()
        {
            UpdateReaderList();
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            RunCardDetection();
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                OnReaderConnected(this, (ReaderEventArgs)e.UserState);
            }
            if (e.ProgressPercentage == 0)
            {
                OnReaderDisconnected(this, (ReaderEventArgs)e.UserState);
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
                        backgroundWorker1.ReportProgress(1, new ReaderEventArgs(newReader));
                    }
                }
                oldList.Remove(newList[i]);
            }

            for (int i = 0; i < oldList.Count; i++)
            {
                if (OnReaderDisconnected != null)
                {
                    backgroundWorker1.ReportProgress(0, new ReaderEventArgs(readers[oldList[i]]));
                }
                readers.Remove(oldList[i]);
            }
        }

        private List<string> ListReaders()
        {
            List<string> readers = new List<string>();
            IntPtr byteCount = (IntPtr)0;
            IntPtr context = NativeMethods.EstablishContext(ReaderScope.User);

            if ((int)NativeMethods.SCardListReaders(context, null, IntPtr.Zero, out byteCount) == (int)Scard.SCARD_S_SUCCESS)
            {
                IntPtr szListReaders = Marshal.AllocHGlobal((int)byteCount);
                if ((int)NativeMethods.SCardListReaders(context, null, szListReaders, out byteCount) == (int)Scard.SCARD_S_SUCCESS)
                {
                    int byteCountt = (int)byteCount;
                    if (byteCountt > 1)
                    {
                        byte[] caReadersData = new byte[byteCountt];
                        Marshal.Copy(szListReaders, caReadersData, 0, (int)byteCount);

                        int start = 0;
                        int end;
                        while (start < byteCountt - 1)
                        {
                            end = start;
                            while (end < byteCountt && caReadersData[end] != 0)
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
            NativeMethods.ReleaseContext(context);
            return readers;
        }
    }
}