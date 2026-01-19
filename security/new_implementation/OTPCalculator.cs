using System;
using System.IO;
using System.Security.Cryptography;
using System.Data.HashFunction.BuzHash;
using eu.advapay.core.hub;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.new_implementation
{
    public class OTPCalculator
    {
        private readonly uint mask;
        private readonly byte[] decryptedUserProfile;

        public OTPCalculator(int otpCodeLength, byte[] decryptedUserProfile)
        {
            this.mask = (uint)Math.Pow(10, otpCodeLength);
            this.decryptedUserProfile = decryptedUserProfile;
        }
        
        public uint CalcOtpWithTime(int secondsToSubtract, byte[] data) 
		{
			return CalcOtp(
			          AppendArrays(GetCurrentTimeMinus(secondsToSubtract), data)
					  );
		}

        public uint CalcOtp(byte[] data) 
        {
            // step-1: making sha hash:
            using var SHA256 = SHA256Managed.Create();
            var hash = SHA256.ComputeHash(AppendArrays(this.decryptedUserProfile, data));

            // step-2: encrypting hash data:
            var encryptedData = historical_implementation.CapiWrapper.KeyGen.Encrypt(hash);

            // step-3: reducing size of created hash
            byte[] reducedSizeHash = new byte[8];
            // default BuzHashFactory ctor applies 8byte size to computed hash
            reducedSizeHash = BuzHashFactory.Instance.Create().ComputeHash(data).Hash;
            
            // ToUInt64(Byte[], Int32) Returns a 64-bit unsigned integer converted from eight bytes at a specified position in a byte array.
            ulong result = BitConverter.ToUInt64(reducedSizeHash, 0); 
            // uint with 8 symb
            return (uint)(result % this.mask);
        }

        public bool ValidateOtp(uint pinCode, byte[] data)
        {
            int otpValidityInterval = Utils.ReadEnvVariableIntValue("OTP_CODE_VALIDITY_INTERVAL_SECONDS");

            for (int i = 0; i < otpValidityInterval; ++i)
            {
                uint pinCodeForThisSecond = CalcOtpWithTime(i, data);
                if (pinCodeForThisSecond == pinCode)
                {
                    return true;
                }
            }

            return false;
        }

        /************** UTILITY FUNCTIONS **************/
        private byte[] AppendArrays(byte[] b1, byte[] b2)
        {
            using var s = new MemoryStream();
            s.Write(b1, 0, b1.Length);
            s.Write(b2, 0, b2.Length);
            return s.ToArray();
        }

        private long GetUnixTime(DateTime tstmp)
        {
            var timeSpan = (tstmp - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        private byte[] GetCurrentTimeMinus(int seconds)
        {
            DateTime currentTime = DateTime.UtcNow;
            DateTime offsetTime = currentTime.AddSeconds(-seconds);
            return BitConverter.GetBytes(GetUnixTime(offsetTime));
        }
    }
}


