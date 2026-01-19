using eu.advapay.core.hub;
using mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper;
using mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.new_implementation;
using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.Text;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiCLRFunctions
{
public partial class CLRFunctions
 {

     [SqlFunction]
     static public void Xp_GetOtpKey(SqlInt32 user, SqlBinary userProfile, out SqlBoolean res, out SqlString resMsg, out SqlBinary key)
     {


         if (userProfile == null || userProfile.Value == null)
         {
             res = false;
             resMsg = "Error. User profile is empty!";
             key = null;
             return;
         }

         try
         {
            

                
             byte[] keyTemp = KeyGen.GetPlainKey(userProfile.Value);

             if (keyTemp == null)
             {
                 res = false;
                 resMsg = "Error. WrapKey!";
                 key = null;
                 return;
             }

             key = keyTemp;
             res = true;
             resMsg = res.IsFalse ? "Error. Get OTP key failed." : "Success.";
         }
         catch (Exception ex)
         {
            Console.WriteLine($"[Xp_GetOtpKey - Exception] Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]");
            key = null;
            res = false;
            resMsg = $"Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]";
         }
     }

     [SqlFunction]
     static public void Xp_GetMacKey(SqlInt32 user, SqlBinary userProfile, out SqlBoolean res, out SqlString resMsg, out SqlBinary key)
     {

         if (userProfile == null || userProfile.Value == null)
         {
             res = false;
             resMsg = "Error. User profile is empty!";
             key = null;
             return;
         }

         try
         {
             
             byte[] keyTemp = KeyGen.GetPlainKey(userProfile.Value);
             if (keyTemp == null)
             {
                 res = false;
                 resMsg = "Error. WrapKey!";
                 key = null;
                 return;
             }

             key = keyTemp;
             res = true;
             resMsg = res.IsFalse ? "Error. Get MAC key failed." : "Success.";
         }
         catch (Exception ex)
         {
            Console.WriteLine($"[Xp_GetMacKey - Exception] Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]");
            key = null;
            res = false;
            resMsg = $"Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]";
         }
     }

 

     [SqlFunction]
     static public void Xp_GenMacKey(SqlInt32 user, out SqlBoolean res, out SqlString resMsg, out SqlString key)
     {
        // necessary-2
         res = false;
         resMsg = "";

         try
         {
             byte[] keyVal = KeyGen.GenMacKey();

             StringBuilder hex = new StringBuilder(keyVal.Length * 2);
             foreach (byte b in keyVal)
                 hex.AppendFormat("{0:x2}", b);
             key = hex.ToString();

             res = true;
             resMsg = res.IsFalse ? "Error. Gen MAC Key failed." : "Success.";
         }
         catch(Exception ex)
         {
            Console.WriteLine($"[Xp_GenMacKey - Exception] Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]");
            key = null;
            res = false;
            resMsg = $"Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]";
         }
     }

     [SqlFunction]
     static public void Xp_GenOtpKey(SqlInt32 user, out SqlBoolean res, out SqlString resMsg, out SqlString key)
     {
        // necessary-4
         res = false;
         resMsg = "";

         byte[] keyVal = KeyGen.GenOtpKey();
         
         StringBuilder hex = new StringBuilder(keyVal.Length * 2);
         foreach (byte b in keyVal)
             hex.AppendFormat("{0:x2}", b);
         key = hex.ToString();
         
         res = true;
         resMsg = res.IsFalse ? "Error. Gen OTP Key failed." : "Success.";

     }

     [SqlFunction]
     static public void Xp_Authenticate(SqlInt32 user, SqlBinary userProfile, SqlInt32 code, out SqlBoolean res, out SqlString resMsg, out SqlInt64 timestamp)
     {

         if (userProfile == null || userProfile.Value == null)
         {
             res = false;
             resMsg = "Error. User profile is empty!";
             timestamp = 0;
             return;
         }

         try
         {
             ManagedTOTP totp = new ManagedTOTP();

             
            byte[] key = KeyGen.GetPlainKey(userProfile.Value);
            totp.InitKey(key);

             long tstmp = 0;
             uint checkCode = (uint)code.Value;

             res = totp.CheckTotp(checkCode, ref tstmp);
             timestamp = tstmp;

             resMsg = res.IsFalse ? "Your OTP is incorrect." : "Success.";
         }
         catch (Exception ex)
         {
            Console.WriteLine($"[Xp_Authenticate - Exception] Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]");
            timestamp = 0;
            res = false;
            resMsg = $"Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]";
         }
     }

        [SqlFunction]
        static public void CoreValidMac(SqlInt32 user, SqlBinary userProfile, SqlInt32 code, SqlString data, out SqlBoolean res, out SqlString resMsg)
        {

            if (userProfile == null || userProfile.Value == null)
            {
                res = false;
                resMsg = "Error. User profile is empty!";
                return;
            }

            try
            {
                byte[] dataBuffer = System.Text.Encoding.UTF8.GetBytes(data.Value);

                int macLength = Utils.ReadEnvVariableIntValue("SECURITY_FUNCTION_MAC_LENGTH");

                CMacAES mac = new CMacAES(macLength);

                byte[] key = KeyGen.GetPlainKey(userProfile.Value);

                mac.InitKey(key);

                uint checkCode = (uint)code.Value;

                res = mac.CheckMAC(checkCode, dataBuffer);

                resMsg = res.IsFalse ? "Error. Validation failed." : "Success.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Xp_ValidMacSign - Exception] Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]");

                res = false;
                resMsg = $"Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]";
            }

        }

        [SqlFunction]
        static public void CoreCreateMAC(SqlInt32 user, SqlBinary userProfile, SqlString data, out SqlInt32 code, out SqlBoolean res, out SqlString resMsg)
        {
            code = 0;
            if (userProfile == null || userProfile.Value == null)
            {
                res = false;
                resMsg = "Error. User profile is empty!";
                return;
            }

            try
            {
                byte[] dataBuffer = System.Text.Encoding.UTF8.GetBytes(data.Value);

                int macLength = Utils.ReadEnvVariableIntValue("SECURITY_FUNCTION_MAC_LENGTH");

                CMacAES mac = new CMacAES(macLength);

                byte[] key = KeyGen.GetPlainKey(userProfile.Value);

                mac.InitKey(key);

                uint checkCode = (uint)code.Value;

                code = (Int32)mac.CalcMac(dataBuffer);
                res = true;
                resMsg = "Success.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Xp_ValidMacSign - Exception] Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]");

                res = false;
                resMsg = $"Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]";
            }

        }

        [SqlFunction]
        static public void Xp_GetKeyCheckValue(SqlBinary key, out SqlString kcv, out SqlBoolean res, out SqlString resMsg)
        {

            kcv = "";

            if (key == null || key.Value == null)
            {
                res = false;
                resMsg = "Error. Key is empty!";
                return;
            }

            try
            {

                byte[] keyValue = key.Value;//KeyGen.GetPlainKey(key.Value);

                byte[] kcvValue = KCV.KeyCheckValue.Calculate(keyValue);
                string hex = BitConverter.ToString(kcvValue).Replace("-", string.Empty);
                kcv = hex;


                res = true;

                resMsg = res.IsFalse ? "Error. Validation failed." : "Success.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Xp_GetKeyCheckValue - Exception] Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]");
                res = false;
                resMsg = $"Security Function Exception: [{Utils.ConvertExceptionToString(ex)}]";
            }

        }

    }
}