using System.IO;
using System.Security.Cryptography;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{

    public class AESCryptor
    {
        static public byte[] Encrypt(byte[] key, byte[] data)
        {
            byte[] iv = new byte[16] { 0, 0, 0, 0 , 0, 0, 0, 0 , 0, 0, 0, 0 , 0, 0, 0, 0 };
            using (MemoryStream ms = new MemoryStream())
            {
                RijndaelManaged aes = new RijndaelManaged();

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.ISO10126;

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                }

            byte[] buffer = ms.ToArray();
            return buffer;
            }
        }

        static public byte[] Decrypt(byte[] key, byte[] data)
        {
            byte[] iv = new byte[16] { 0,0,0,0, 0, 0, 0, 0 , 0, 0, 0, 0 , 0, 0, 0, 0 };
            using (MemoryStream srDecrypt = new MemoryStream())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    RijndaelManaged aes = new RijndaelManaged();

                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.ISO10126;

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    byte[] result = ms.ToArray();
                    return result;

                }

            }

        }

    }
}
