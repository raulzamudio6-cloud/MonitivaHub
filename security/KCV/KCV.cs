using System.IO;
using System.Security.Cryptography;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.KCV
{
    public class KeyCheckValue
    {
        public static byte[] Calculate(byte[] keyValue)
        {
            byte[] key = new byte[16];
            byte[] zero = new byte[16];
            byte[] iv = new byte[16];

            int keyLenBody = keyValue.Length > 16 ? 16 : keyValue.Length;
            for (int i = 0; i < keyLenBody; ++i)
            {
                key[i] = keyValue[i];
                zero[i] = 0;
                iv[i] = 0;
            }

            // шифруем 16 нулевых байт


            using (MemoryStream ms = new MemoryStream())
            {
                RijndaelManaged aes = new RijndaelManaged();

                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    cs.Write(zero, 0, zero.Length);
                    cs.FlushFinalBlock();

                    byte[] kcv = ms.ToArray();

                    byte[] result = new byte[3];
                    for (int i = 0; i < 3; ++i)
                    {
                        result[i] = kcv[i];
                    }
                    return result;
                }
            }
        }

    }
}
