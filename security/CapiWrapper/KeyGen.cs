using eu.advapay.core.hub;
using System.Security.Cryptography;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    public class KeyGen
    {

        private static byte[]  DeriveMasterKey()
        {
            //1. Значение пароль OTP_MASTER_PASWWORD
            //2. byte[] CipherKey = Derive(OTP_MASTER_PASWWORD)
            string password = Utils.ReadEnvVariable($"SECURITY_FUNCTION_ENCRYPTION_PASSWORD");
            byte[] saltRaw = Utils.Base64Decode(Utils.ReadEnvVariable($"SECURITY_FUNCTION_ENCRYPTION_SALT"));
            // number of iterations should be >= 1000. we are using 2000 
            int iterations = 2000;

            var pbkdf2 = new Rfc2898DeriveBytes(password, saltRaw, iterations);
            byte[] key = pbkdf2.GetBytes(32);

            return key;
        }



        private static byte[] Gen(int len)
        {
            
            var provider = new RNGCryptoServiceProvider();
            var response = new byte[len];

            provider.GetBytes(response);


            return response;
            
        }
        public static byte[] GenOtpKey()
        {
            return Encrypt(Gen(20));
        }
        public static byte[] GenMacKey()
        {
            return Encrypt(Gen(16));
        }

        public static byte[] Encrypt(byte[] userData) 
        {
            byte[] masterKey = DeriveMasterKey();
            return AESCryptor.Encrypt(masterKey, userData);
        }

        public static byte[] Decrypt(byte[] userKey) 
        {
            byte[] masterKey = DeriveMasterKey();
            return AESCryptor.Decrypt(masterKey, userKey);
        }

        public static byte[] GetPlainKey(byte[] userKey)
        {
            return Decrypt(userKey);
        }

    }
}
