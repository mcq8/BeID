using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Be.Mcq8.EidReader
{
    internal class Verifier
    {
        private X509Certificate2 certificate;
        private readonly RSAPKCS1SignatureDeformatter RSADeformatter;
        private readonly ECDsa ecdsa;

        public HashAlgorithm Sha { get; private set; }

        public Verifier(byte[] cert)
        {
            certificate = new X509Certificate2(cert);

            if (certificate.SignatureAlgorithm.Value == "1.2.840.10045.4.3.3")
            {
                ecdsa = certificate.GetECDsaPublicKey();
                Sha = new SHA384Managed();
            }
            else
            {

                RSADeformatter = new RSAPKCS1SignatureDeformatter(certificate.PublicKey.Key);
                if (certificate.SignatureAlgorithm.Value == "1.2.840.113549.1.1.11")
                {
                    Sha = new SHA256Managed();
                    RSADeformatter.SetHashAlgorithm("SHA256");
                }
                else
                {
                    Sha = new SHA1Managed();
                    RSADeformatter.SetHashAlgorithm("SHA1");
                }
            }
        }

        public bool Verify(byte[] data, byte[] signature)
        {
            if (certificate.SignatureAlgorithm.Value == "1.2.840.10045.4.3.3")
            {
                //newer dotnet versions have RSADeformatter.VerifySignature(Sha.ComputeHash(data), signature, DSASignatureFormat.Rfc3279DerSequence);
                try
                {
                    signature = ConvertEcdsaDerToIeeeP1363(signature, 48);

                }
                catch (Exception)
                {
                    return false;
                }
                return ecdsa.VerifyHash(Sha.ComputeHash(data), signature);
            }

            return RSADeformatter.VerifySignature(Sha.ComputeHash(data), signature);
        }


        private static byte[] ConvertEcdsaDerToIeeeP1363(byte[] derSignature, int fieldSizeBytes)
        {
            int offset = 0;
            if (derSignature[offset++] != 0x30)
                throw new ArgumentException("Invalid DER sequence");

            // Read length
            int length = derSignature[offset++];
            if ((length & 0x80) != 0)
            {
                int lengthBytes = length & 0x7F;
                length = 0;
                for (int i = 0; i < lengthBytes; i++)
                    length = (length << 8) | derSignature[offset++];
            }

            if (derSignature[offset++] != 0x02)
                throw new ArgumentException("Expected INTEGER for r");

            int rLen = derSignature[offset++];
            byte[] r = new byte[rLen];
            Array.Copy(derSignature, offset, r, 0, rLen);
            offset += rLen;

            if (derSignature[offset++] != 0x02)
                throw new ArgumentException("Expected INTEGER for s");

            int sLen = derSignature[offset++];
            byte[] s = new byte[sLen];
            Array.Copy(derSignature, offset, s, 0, sLen);

            byte[] rFixed = LeftPadOrTruncate(r, fieldSizeBytes);
            byte[] sFixed = LeftPadOrTruncate(s, fieldSizeBytes);

            byte[] ieeeSignature = new byte[fieldSizeBytes * 2];
            Buffer.BlockCopy(rFixed, 0, ieeeSignature, 0, fieldSizeBytes);
            Buffer.BlockCopy(sFixed, 0, ieeeSignature, fieldSizeBytes, fieldSizeBytes);
            return ieeeSignature;
        }

        private static byte[] LeftPadOrTruncate(byte[] input, int size)
        {
            if (input.Length == size)
                return input;

            byte[] output = new byte[size];
            if (input.Length < size)
            {
                Buffer.BlockCopy(input, 0, output, size - input.Length, input.Length);
            }
            else
            {
                Buffer.BlockCopy(input, input.Length - size, output, 0, size);
            }
            return output;
        }
    }
}
