using System;
using System.IO;
using System.Security.Cryptography;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    public class CMacAES
    {


        private readonly uint mask;

        private CMacAES() : this(8) { }

        public CMacAES(int codeLenth)
        {
            mask = (uint)Math.Pow(10, codeLenth);
        }


        /// <summary>
        /// 
        /// </summary>
        byte[] m_userKey = null;

        public bool InitKey(byte[] wrapKey)
        {
            m_userKey = wrapKey;
            return true;
        }

        byte[] AESEncrypt(byte[] key, byte[] iv, byte[] data)
        {
            using MemoryStream ms = new MemoryStream();
            RijndaelManaged aes = new RijndaelManaged();

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            using CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();

            byte[] mac = ms.ToArray();
            return mac;
        }

        byte[] Rol(byte[] b)
        {
            byte[] r = new byte[b.Length];
            byte carry = 0;

            for (int i = b.Length - 1; i >= 0; i--)
            {
                ushort u = (ushort)(b[i] << 1);
                r[i] = (byte)((u & 0xff) + carry);
                carry = (byte)((u & 0xff00) >> 8);
            }

            return r;
        }
        private byte[] AppendArrays(byte[] b1, byte[] b2)
        {
            using var s = new MemoryStream();
            s.Write(b1, 0, b1.Length);
            s.Write(b2, 0, b2.Length);
            return s.ToArray();
        }

        public uint CalcMacWithTime(byte[] data)
        {
            return CalcMac(AppendArrays(AddTime(0), data));
        }

        public uint CalcMac(byte[] data) 
        {

            byte[] L = AESEncrypt(m_userKey, new byte[16], new byte[16]);

            byte[] FirstSubkey = Rol(L);
            if ((L[0] & 0x80) == 0x80)
            {
                FirstSubkey[15] ^= 0x87;
            }

            byte[] SecondSubkey = Rol(FirstSubkey);
            if ((FirstSubkey[0] & 0x80) == 0x80)
            {
                SecondSubkey[15] ^= 0x87;
            }
            byte[] blocks = null;

            if (((data.Length != 0) && (data.Length % 16 == 0)) == true)
            {
                blocks = data;

                for (int j = 0; j < FirstSubkey.Length; j++)
                {
                    blocks[blocks.Length - 16 + j] ^= FirstSubkey[j];
                }
            }
            else
            {

                int paddingLength = 16 - (data.Length % 16);

                blocks = new byte[data.Length + paddingLength];
                for (int i = 0; i < data.Length; ++i)
                {
                    blocks[i] = data[i];
                }

                blocks[data.Length] = 0x80;
                for (int i = data.Length + 1; i < blocks.Length; ++i)
                {
                    blocks[i] = 0;
                }

                for (int j = 0; j < SecondSubkey.Length; j++)
                {
                    blocks[blocks.Length - 16 + j] ^= SecondSubkey[j];
                }
            }

            // The result of the previous process will be the input of the last encryption.
            byte[] encResult = AESEncrypt(m_userKey, new byte[16], blocks);

            byte[] HashValue = new byte[16];
            Array.Copy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);



            byte offset = (byte)(HashValue[15] & 0x07);

            uint dbc = (uint)((HashValue[offset] & 0x7f) << 24 |
                                   (HashValue[offset + 1] & 0xff) << 16 |
                                   (HashValue[offset + 2] & 0xff) << 8 |
                                   (HashValue[offset + 3] & 0xff));

            dbc = dbc % 100000000;
            return dbc;
        }

        private long GetUnixTime(DateTime tstmp)
        {
            var timeSpan = (tstmp - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
        const int TIME_CHANK = 30;

        const int SEARCH_WINDOW = 50;

        private int GetValidInterval()
        {
            return TIME_CHANK;
        }

        private byte[] AddTime(int iteration)
        {
            DateTime currentTime = DateTime.UtcNow;
            int startOffset = -1 * (GetValidInterval() * SEARCH_WINDOW);
            DateTime offsetTime = currentTime.AddSeconds(startOffset);
            long timeCode = GetUnixTime(offsetTime) / GetValidInterval();
            return BitConverter.GetBytes(timeCode + iteration);
        }

        public bool CheckMAC(uint sign, byte[] data)
        {
            uint dbc = CalcMac(data);
            return dbc == sign;
        }

        public bool CheckMACWithTime(uint sign, byte[] data)
        {
            for (int i = 0; i < 100; ++i)
            {
                
                byte[] timeMark = AddTime(i);
                byte[] extendedData = new byte[timeMark.Length + data.Length];
                System.Buffer.BlockCopy(timeMark, 0, extendedData, 0, timeMark.Length);
                System.Buffer.BlockCopy(data, 0, extendedData, timeMark.Length, data.Length);

                uint dbc = CalcMac(data);
                if (dbc == sign)
                    return true;
            }

            return false;            
        }
    }
}
