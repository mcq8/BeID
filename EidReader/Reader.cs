using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Threading;

namespace Be.Mcq8.EidReader
{
    public class Reader : IDisposable
    {
        public event EventHandler<ReaderEventArgs> OnIdRead = null;
        public event EventHandler<ReaderEventArgs> OnAddressRead = null;
        public event EventHandler<ReaderEventArgs> OnPhotoRead = null;
        public event EventHandler<ReaderEventArgs> OnDataCleared = null;

        public IdFile idFile { get; private set; }
        public AddressFile addressFile { get; private set; }
        public byte[] photoFile { get; private set; }
        public String name { get; private set; }

        public ReadOption readOption = ReadOption.All;

        private const int APDU_MIN_LENGTH = 4;
        private const uint WAIT_TIME = 250;
        private const int MAX_RETRIES = 3;

        private IntPtr cardPointer = IntPtr.Zero;
        private IntPtr contextPointer = IntPtr.Zero;

        private SCardIORequest ioRequest;
        private bool runCardDetection;
        private Thread thread;

        public Reader(String name)
        {
            ioRequest = new SCardIORequest
            {
                Protocol = 1,
                PciLength = 8
            };
            this.name = name;
            thread = new Thread(RunCardDetection);
            thread.Start();
        }

        ~Reader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnIdRead = null;
                OnAddressRead = null;
                OnDataCleared = null;
                OnPhotoRead = null;
                runCardDetection = false;
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                }
            }
        }

        protected void RunCardDetection()
        {
            try
            {
                bool bFirstLoop = true;
                runCardDetection = true;
                int lastError;

                contextPointer = EstablishContext(ReaderScope.User);

                UInt32 nbReaders = 1;
                SCardReaderState[] readerState = new SCardReaderState[nbReaders];

                readerState[0].CurrentState = (UInt32)CardState.UNAWARE;
                readerState[0].ReaderName = name;
                while (runCardDetection)
                {
                    lastError = NativeMethods.SCardGetStatusChange(contextPointer, WAIT_TIME, readerState, nbReaders);
                    if (lastError == (int)Scard.SCARD_E_TIMEOUT)
                    {
                        continue;
                    }
                    else if (lastError == (int)Scard.SCARD_E_NO_SERVICE || lastError == (int)Scard.SCARD_E_SERVICE_STOPPED)
                    {
                        break;
                    }
                    else if (lastError != (int)Scard.SCARD_S_SUCCESS)
                    {
                        throw new ReaderException(this, "SCardGetStatusChange error: " + lastError);
                    }
                    UInt32 eventState = readerState[0].EventState;
                    UInt32 currentState = readerState[0].CurrentState;

                    if (((eventState & (uint)CardState.CHANGED) == (uint)CardState.CHANGED))
                    {
                        if ((eventState & (uint)CardState.EMPTY) == (uint)CardState.EMPTY)
                        {
                            cardRemoved();
                        }
                        else if ((((eventState & (uint)CardState.PRESENT) == (uint)CardState.PRESENT) &&
                            ((eventState & (uint)CardState.PRESENT) != (currentState & (uint)CardState.PRESENT))) ||
                            (eventState & (uint)CardState.ATRMATCH) == (uint)CardState.ATRMATCH ||
                            bFirstLoop)
                        {
                            try
                            {
                                cardInserted();
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex.Message);
                                Logger.Log(ex.StackTrace);
                            }
                        }
                    }

                    readerState[0].CurrentState = eventState;
                    bFirstLoop = false;
                }

                ReleaseContext(contextPointer);

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private void cardRemoved()
        {
            idFile = null;
            addressFile = null;
            photoFile = null;
            if (OnDataCleared != null)
                OnDataCleared(this, new ReaderEventArgs(this));
        }

        private void cardInserted()
        {
            try
            {
                Connect();
                RSAPKCS1SignatureDeformatter RSADeformatter;
                SHA1Managed sha = new SHA1Managed();
                byte[] result;
                byte[] resultSignId;
                int tries = -1;
                do
                {
                    tries++;
                    byte[] rnCertData = getFile(FileID.RnCert);
                    X509Certificate2 certificate = new X509Certificate2(rnCertData);
                    RSADeformatter = new RSAPKCS1SignatureDeformatter(certificate.PublicKey.Key);
                    RSADeformatter.SetHashAlgorithm("SHA1");

                    result = getFile(FileID.Id);
                    resultSignId = getFile(FileID.IdSign);
                }
                while (!(RSADeformatter.VerifySignature(sha.ComputeHash(result), resultSignId)) && tries < MAX_RETRIES);

                if (tries == MAX_RETRIES)
                {
                    throw new ReaderException(this, "Failed to read the Id file");
                }

                idFile = new IdFile(result);
                if (OnIdRead != null)
                {
                    OnIdRead(this, new ReaderEventArgs(this));
                }

                if (readOption == ReadOption.IdAndAddress || readOption == ReadOption.All)
                {
                    byte[] resultSign;
                    byte[] resultSignAppended;
                    tries = -1;
                    do
                    {
                        tries++;

                        result = getFile(FileID.Address);
                        resultSign = getFile(FileID.AddressSign);

                        int resultLength = 0;
                        while (resultLength < result.Length && result[resultLength] != 0)
                            resultLength++;

                        resultSignAppended = new byte[resultLength + resultSignId.Length];
                        Buffer.BlockCopy(result, 0, resultSignAppended, 0, resultLength);
                        Buffer.BlockCopy(resultSignId, 0, resultSignAppended, resultLength, resultSignId.Length);
                    }
                    while (!(RSADeformatter.VerifySignature(sha.ComputeHash(resultSignAppended), resultSign)) && tries < MAX_RETRIES);

                    if (tries == MAX_RETRIES)
                    {
                        throw new ReaderException(this, "Failed to read the address file");
                    }

                    addressFile = new AddressFile(result);

                    if (OnAddressRead != null)
                    {
                        OnAddressRead(this, new ReaderEventArgs(this));
                    }

                    if (readOption == ReadOption.All)
                    {
                        tries = -1;
                        do
                        {
                            tries++;
                            photoFile = getFile(FileID.Photo);
                        }
                        while (!(sha.ComputeHash(photoFile).SequenceEqual(idFile.PhotoHash)) && tries < MAX_RETRIES);

                        if (tries == MAX_RETRIES)
                        {
                            throw new ReaderException(this, "Failed to read the image file");
                        }

                        if (OnPhotoRead != null)
                        {
                            OnPhotoRead(this, new ReaderEventArgs(this));
                        }
                    }
                }
            }
            finally
            {
                if (cardPointer != IntPtr.Zero)
                {
                    int lastError = NativeMethods.SCardEndTransaction(cardPointer, 0);
                    if (lastError != (int)Scard.SCARD_S_SUCCESS && lastError != (int)Scard.SCARD_W_REMOVED_CARD)
                    {
                        throw new ReaderException(this, "SCardEndTransaction error: " + lastError);
                    }
                }
            }
        }

        private byte[] getFile(FileID fileId)
        {
            byte[] fileIdBytes = new byte[6];
            byte[] convertArray = BitConverter.GetBytes((long)fileId);
            Array.Reverse(convertArray);
            Buffer.BlockCopy(convertArray, 2, fileIdBytes, 0, 6);
            convertArray = null;

            APDUResponse apduResp = Transmit(0, 0xa4, 0x08, 0x0C, fileIdBytes, 0);
            if (apduResp.Status != 0x9000)
            {
                throw new ReaderException(this, "File select error response: " + apduResp.Status);
            }

            List<byte[]> tmp = new List<byte[]>();

            byte pagenum = 0;
            apduResp = Transmit(0, 0xb0, pagenum, 0, null, 256);
            while (apduResp.Status == 0x9000)
            {
                tmp.Add(apduResp.Data);
                pagenum++;
                apduResp = Transmit(0, 0xb0, pagenum, 0, null, 256);
            }
            byte[] result;
            if (apduResp.SW2 > 0)
            {
                apduResp = Transmit(0, 0xb0, pagenum, 0, null, apduResp.SW2);
                result = new byte[apduResp.Data.Length + (tmp.Count * 256)];
                tmp.Add(apduResp.Data);
            }
            else
            {
                result = new byte[tmp.Count * 256];
            }

            for (int i = 0; i < tmp.Count; i++)
            {
                Buffer.BlockCopy(tmp[i], 0, result, i * 256, tmp[i].Length);
            }
            return result;
        }

        private APDUResponse Transmit(byte bCla, byte bIns, byte bP1, byte bP2, byte[] baData, uint bLe)
        {
            byte[] ApduBuffer = null;

            uint RecvLength = bLe + APDUResponse.SW_LENGTH;
            byte[] ApduResponse = new byte[RecvLength];

            if (baData == null)
            {
                ApduBuffer = new byte[APDU_MIN_LENGTH + 1];
                ApduBuffer[APDU_MIN_LENGTH] = (byte)bLe;
            }
            else
            {
                ApduBuffer = new byte[APDU_MIN_LENGTH + 1 + baData.Length];
                Buffer.BlockCopy(baData, 0, ApduBuffer, APDU_MIN_LENGTH + 1, baData.Length);
                ApduBuffer[APDU_MIN_LENGTH] = (byte)baData.Length;
            }

            ApduBuffer[0] = bCla;
            ApduBuffer[1] = bIns;
            ApduBuffer[2] = bP1;
            ApduBuffer[3] = bP2;

            int lastError = NativeMethods.SCardTransmit(cardPointer, ref ioRequest, ApduBuffer, (uint)ApduBuffer.Length, IntPtr.Zero, ApduResponse, out RecvLength);

            if (lastError != (int)Scard.SCARD_S_SUCCESS)
            {
                throw new ReaderException(this, "SCardTransmit error: " + lastError + " Apdu: " + BitConverter.ToString(ApduBuffer));
            }
            return new APDUResponse(ApduResponse, RecvLength);
        }

        private void Connect()
        {
            IntPtr hCard = Marshal.AllocHGlobal(Marshal.SizeOf(cardPointer));
            IntPtr pProtocol = Marshal.AllocHGlobal(Marshal.SizeOf(ioRequest.Protocol));

            try
            {
                int lastError = NativeMethods.SCardConnect(contextPointer, this.name, (uint)ReaderShare.Shared, ioRequest.Protocol, hCard, pProtocol);

                if (lastError != (int)Scard.SCARD_S_SUCCESS)
                {
                    throw new ReaderException(this, "SCardConnect error: " + lastError);
                }

                cardPointer = Marshal.ReadIntPtr(hCard);
                ioRequest.Protocol = (uint)Marshal.ReadInt32(pProtocol);
                ioRequest.PciLength = 8;

                lastError = NativeMethods.SCardBeginTransaction(cardPointer);
                if (lastError != (int)Scard.SCARD_S_SUCCESS)
                {
                    throw new ReaderException(this, "SCardBeginTransaction error: " + lastError);
                }
            }
            catch (ReaderException)
            {
                cardPointer = IntPtr.Zero;
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(hCard);
                Marshal.FreeHGlobal(pProtocol);
            }
        }

        internal static IntPtr EstablishContext(ReaderScope scope)
        {
            IntPtr context = IntPtr.Zero;
            IntPtr hContext = Marshal.AllocHGlobal(Marshal.SizeOf(context));
            int lastError = 0;
            try
            {
                ServiceController service = new ServiceController("SCardSvr");
                if (service.Status != ServiceControllerStatus.Running)
                {
                    throw new SCardSvrNotRunningException();
                }
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
    }
}