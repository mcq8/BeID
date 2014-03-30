using System;

namespace Be.Mcq8.EidReader
{
    internal class APDUResponse
    {
        public const int SW_LENGTH = 2;

        public byte[] Data { get; private set; }
        public byte SW1 { get; private set; }
        public byte SW2 { get; private set; }

        public ushort Status
        {
            get
            {
                return (ushort)(((short)SW1 << 8) + (short)SW2);
            }
        }

        public APDUResponse(byte[] baData, uint length)
        {
            if (length > SW_LENGTH)
            {
                int dataLength = (int)length - SW_LENGTH;
                Data = new byte[dataLength];
                Buffer.BlockCopy(baData, 0, Data, 0, dataLength);
            }

            SW1 = baData[length - 2];
            SW2 = baData[length - 1];
        }
    }
}