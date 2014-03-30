using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Be.Mcq8.EidReader
{
    public class IdFile
    {
        public UInt64 CardNumber { get; private set; }
        public BigInteger ChipNumber { get; private set; }
        public DateTime CardValidityDateBegin { get; private set; }
        public DateTime CardValidityDateEnd { get; private set; }
        public String CardDeliveryMunicipality { get; private set; }
        public UInt64 NationalNumber { get; private set; }
        public String Name { get; private set; }
        public String FirstNames { get; private set; }
        public String FirstName
        {
            get
            {
                int space = FirstNames.IndexOf(' ');
                if (space > 0)
                {
                    return FirstNames.Substring(0, space);
                }
                else
                {
                    return FirstNames;
                }
            }
        }
        public String ThirdName { get; private set; }
        public String Nationality { get; private set; }
        public String BirthLocation { get; private set; }
        public DateTime Birthdate { get; private set; }
        public Char Sex { get; private set; }
        public String NobleCondition { get; private set; }
        public byte DocumentType { get; private set; }
        public byte SpecialStatus { get; private set; }
        public byte[] PhotoHash { get; private set; }

        public IdFile(byte[] result)
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
                            CardNumber = UInt64.Parse(Encoding.ASCII.GetString(result, p, len), CultureInfo.InvariantCulture);
                            break;
                        case 2:
                            byte[] _ChipNumber = new Byte[len];
                            for (int i = 0; i < len; i++)
                            {
                                _ChipNumber[len - 1 - i] = result[p + i];
                            }
                            ChipNumber = new BigInteger(_ChipNumber);
                            break;
                        case 3:
                            String _sCardValidityDateBegin = Encoding.ASCII.GetString(result, p, len);
                            CardValidityDateBegin = new DateTime(int.Parse(_sCardValidityDateBegin.Substring(6, 4), CultureInfo.InvariantCulture), int.Parse(_sCardValidityDateBegin.Substring(3, 2), CultureInfo.InvariantCulture), int.Parse(_sCardValidityDateBegin.Substring(0, 2), CultureInfo.InvariantCulture));
                            break;
                        case 4:
                            String _sCardValidityDateEnd = Encoding.ASCII.GetString(result, p, len);
                            CardValidityDateEnd = new DateTime(int.Parse(_sCardValidityDateEnd.Substring(6, 4), CultureInfo.InvariantCulture), int.Parse(_sCardValidityDateEnd.Substring(3, 2), CultureInfo.InvariantCulture), int.Parse(_sCardValidityDateEnd.Substring(0, 2), CultureInfo.InvariantCulture));
                            break;
                        case 5:
                            CardDeliveryMunicipality = Encoding.UTF8.GetString(result, p, len);
                            break;
                        case 6:
                            NationalNumber = UInt64.Parse(Encoding.ASCII.GetString(result, p, len), CultureInfo.InvariantCulture);
                            break;
                        case 7:
                            Name = Encoding.UTF8.GetString(result, p, len);
                            break;
                        case 8:
                            FirstNames = Encoding.UTF8.GetString(result, p, len);
                            break;
                        case 9:
                            ThirdName = Encoding.UTF8.GetString(result, p, len);
                            break;
                        case 10:
                            Nationality = Encoding.UTF8.GetString(result, p, len);
                            break;
                        case 11:
                            BirthLocation = Encoding.UTF8.GetString(result, p, len);
                            break;
                        case 12:
                            String _sBirthDate = Encoding.UTF8.GetString(result, p, len);
                            int month = 0;
                            switch (_sBirthDate.Substring(3, 4).Trim())
                            {
                                case "JAN":
                                    month = 1;
                                    break;
                                case "FEV":
                                case "FEB":
                                    month = 2;
                                    break;
                                case "MARS":
                                case "MAAR":
                                case "MÄR":
                                    month = 3;
                                    break;
                                case "AVR":
                                case "APR":
                                    month = 4;
                                    break;
                                case "MAI":
                                case "MEI":
                                    month = 5;
                                    break;
                                case "JUIN":
                                case "JUN":
                                    month = 6;
                                    break;
                                case "JUIL":
                                case "JUL":
                                    month = 7;
                                    break;
                                case "AOUT":
                                case "AUG":
                                    month = 8;
                                    break;
                                case "SEPT":
                                case "SEP":
                                    month = 9;
                                    break;
                                case "OCT":
                                case "OKT":
                                    month = 10;
                                    break;
                                case "NOV":
                                    month = 11;
                                    break;
                                case "DEC":
                                case "DEZ":
                                    month = 12;
                                    break;
                            }
                            Birthdate = new DateTime(int.Parse(_sBirthDate.Substring(_sBirthDate.Length - 4, 4), CultureInfo.InvariantCulture), month, int.Parse(_sBirthDate.Substring(0, 2), CultureInfo.InvariantCulture));
                            break;
                        case 13:
                            Sex = (result[p] == 77) ? 'M' : 'F';
                            break;
                        case 14:
                            NobleCondition = Encoding.UTF8.GetString(result, p, len);
                            break;
                        case 15:
                            DocumentType = byte.Parse(Encoding.ASCII.GetString(result, p, len), CultureInfo.InvariantCulture);
                            break;
                        case 16:
                            SpecialStatus = byte.Parse(Encoding.ASCII.GetString(result, p, len), CultureInfo.InvariantCulture);
                            break;
                        case 17:
                            PhotoHash = new byte[len];
                            Buffer.BlockCopy(result, p, PhotoHash, 0, len);
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