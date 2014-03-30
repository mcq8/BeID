using System;
using System.Globalization;
using System.Text;

namespace Be.Mcq8.EidReader
{
    public class AddressFile
    {
        public String StreetAndNumber { get; private set; }
        public Int16 ZipCode { get; private set; }
        public String Municipality { get; private set; }

        internal AddressFile(byte[] result)
        {
            int p = 0;
            int id = 0;
            int len = 0;
            while (p < (result.Length - 2))
            {
                id = result[p++];
                len = result[p++];
                try
                {
                    switch (id)
                    {
                        case 1:
                            StreetAndNumber = Encoding.UTF8.GetString(result, p, len);
                            break;
                        case 2:
                            ZipCode = Int16.Parse(Encoding.ASCII.GetString(result, p, len), CultureInfo.InvariantCulture);
                            break;
                        case 3:
                            Municipality = Encoding.UTF8.GetString(result, p, len);
                            break;
                    }
                }
                catch (Exception)
                {
                }
                p += len;
            }
        }
    }
}