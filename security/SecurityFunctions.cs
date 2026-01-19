using mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.new_implementation;
using mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiCLRFunctions;
using mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper;
using NLog;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace eu.advapay.core.hub.security
{
    public static class SecurityFunctions
    {

       /**
         * 
         * This funciton takes incomming JSON and prepares response. If something happens - throws exception. 
         *
         *  Expected JSON fields: 
         *  HttpUtilsRequestType - value can be httpGetRequest, httpPostRequest, httpAnyRequest
         */
         public static string ProcessTaskAndPrepareResponse(string MbTaskParams, ILogger log)
         {
            /*
             *  Parsing json received
             */
            var requestParams = Utils.ParseJsonToOneLevelDictionary(MbTaskParams);
            var functionToBeInvoked = requestParams["functionName"];

            string ResponseBody = "";
            switch (functionToBeInvoked)
            {
                case "xp_GetMacKey":
                    HandleGetMacKey(requestParams, out ResponseBody);
                    break;
                case "xp_GenMacKey":
                    HandleGenMacKey(requestParams, out ResponseBody);
                    break;
                case "xp_ValidMacSign":
                    HandleValidMacSign(requestParams, out ResponseBody);
                    break;
                case "xp_GetOtpKey":
                    HandleGetOtpKey(requestParams, out ResponseBody);
                    break;
                case "xp_GenOtpKey":
                    HandleGenOtpKey(requestParams, out ResponseBody);
                    break;
                case "xp_Authenticate":
                    HandleAuthenticate(requestParams, out ResponseBody);
                    break;
                case "xp_GetKeyCheckValue":
                    HandleGetKeyCheckValue(requestParams, out ResponseBody);
                    break;
                case "xp_ValidateESignature":
                    HandleValidateESignature(requestParams, out ResponseBody);
                    break;
                case "xp_ApplyESignature":
                    HandleApplyESignature(requestParams, out ResponseBody);
                    break;
                default:
                    Utils.Assert(false, string.Format("Fatal: Unsupported request Type={0}", functionToBeInvoked));
                    break;
            }

            return ResponseBody;

        }

        private static void HandleGetMacKey(Dictionary<string, string> requestParams, out string responseBody)
        {
            // xp_GetMacKey(SqlInt32 user, SqlBinary userProfile, out SqlBoolean res, out SqlString resMsg, out SqlBinary key)
            SqlInt32 user = Utils.ConvertStringToSqlInt32(requestParams["user"]);

            byte[] userProfileBytes = System.Convert.FromBase64String(requestParams["userProfile"]);
            SqlBinary userProfile = new SqlBinary(userProfileBytes);

            CLRFunctions.Xp_GetMacKey(user, userProfile, out SqlBoolean res, out SqlString resMsg, out SqlBinary key);

            var jsonObj = new JObject
            {
                { "res", res.ToString() },
                { "resMsg", resMsg.ToString() },
                { "key", System.Convert.ToBase64String(key.IsNull ? new byte[0] : key.Value) }
            };

            responseBody = jsonObj.ToString();

        }

        private static void HandleGenMacKey(Dictionary<string, string> requestParams, out string responseBody)
        {
            //  xp_GenMacKey(SqlInt32 user, out SqlBoolean res, out SqlString resMsg, out SqlString key)
            SqlInt32 user = Utils.ConvertStringToSqlInt32(requestParams["user"]);

            CLRFunctions.Xp_GenMacKey(user, out SqlBoolean res, out SqlString resMsg, out SqlString key);

            var jsonObj = new JObject
            {
                { "res", res.ToString() },
                { "resMsg", resMsg.ToString() },
                { "key", key.IsNull ? "" : key.ToString() }
            };

            responseBody = jsonObj.ToString();

        }

        private static void HandleValidMacSign(Dictionary<string, string> requestParams, out string responseBody)
        {
            //     xp_ValidMacSign(SqlInt32 user, SqlBinary userProfile, SqlInt32 code, SqlString data, out SqlBoolean res, out SqlString resMsg)
            SqlInt32 user = Utils.ConvertStringToSqlInt32(requestParams["user"]);

            byte[] userProfileBytes = System.Convert.FromBase64String(requestParams["userProfile"]);
            SqlBinary userProfile = new SqlBinary(userProfileBytes);

            SqlInt32 code = Utils.ConvertStringToSqlInt32(requestParams["code"]);
            SqlString data = new SqlString(requestParams["data"]);

            CLRFunctions.CoreValidMac(user, userProfile, code, data, out SqlBoolean res, out SqlString resMsg);

            var jsonObj = new JObject
            {
                { "res", res.ToString() },
                { "resMsg", resMsg.ToString() }
            };

            responseBody = jsonObj.ToString();
        }



        private static void HandleGetOtpKey(Dictionary<string, string> requestParams, out string responseBody)
        {
            // static public void xp_GetOtpKey(SqlInt32 user, SqlBinary userProfile, out SqlBoolean res, out SqlString resMsg, out SqlBinary key)

            SqlInt32 user = Utils.ConvertStringToSqlInt32(requestParams["user"]);

            byte[] userProfileBytes = System.Convert.FromBase64String(requestParams["userProfile"]);
            SqlBinary userProfile = new SqlBinary(userProfileBytes);

            CLRFunctions.Xp_GetOtpKey(user, userProfile, out SqlBoolean res, out SqlString resMsg, out SqlBinary key);

            var jsonObj = new JObject
            {
                { "res", res.ToString() },
                { "resMsg", resMsg.ToString() },
                { "key", System.Convert.ToBase64String(key.IsNull ? new byte[0] : key.Value) }
            };

            responseBody = jsonObj.ToString();
        }

        private static void HandleGenOtpKey(Dictionary<string, string> requestParams, out string responseBody)
        {
            // void xp_GenOtpKey(SqlInt32 user, out SqlBoolean res, out SqlString resMsg, out SqlString key)

            SqlInt32 user = Utils.ConvertStringToSqlInt32(requestParams["user"]);
            CLRFunctions.Xp_GenOtpKey(user, out SqlBoolean res, out SqlString resMsg, out SqlString key);

            var jsonObj = new JObject
            {
                { "res", res.ToString() },
                { "resMsg", resMsg.ToString() },
                { "key", key.IsNull ? "" : key.ToString() }
            };

            responseBody = jsonObj.ToString();
        }

        private static void HandleAuthenticate(Dictionary<string, string> requestParams, out string responseBody)
        {
            // void xp_Authenticate(SqlInt32 user, SqlBinary userProfile, SqlInt32 code, out SqlBoolean res, out SqlString resMsg, out SqlInt64 timestamp)
            SqlInt32 user = Utils.ConvertStringToSqlInt32(requestParams["user"]);

            byte[] userProfileBytes = System.Convert.FromBase64String(requestParams["userProfile"]);
            SqlBinary userProfile = new SqlBinary(userProfileBytes);

            SqlInt32 code = Utils.ConvertStringToSqlInt32(requestParams["code"]);

            CLRFunctions.Xp_Authenticate(user, userProfile, code, out SqlBoolean res, out SqlString resMsg, out SqlInt64 timestamp);

            var jsonObj = new JObject
            {
                { "res", res.ToString() },
                { "resMsg", resMsg.ToString() },
                { "timestamp", timestamp.Value }
            };

            responseBody = jsonObj.ToString();
        }
        
        private static void HandleGetKeyCheckValue(Dictionary<string, string> requestParams, out string responseBody)
        {
            // void xp_GetKeyCheckValue(SqlBinary key, out SqlString kcv, out SqlBoolean res, out SqlString resMsg)

            byte[] keyBytes = System.Convert.FromBase64String(requestParams["key"]);
            SqlBinary key = new SqlBinary(keyBytes);

            CLRFunctions.Xp_GetKeyCheckValue(key, out SqlString kcv, out SqlBoolean res, out SqlString resMsg);

            var jsonObj = new JObject
            {
                { "res", res.ToString() },
                { "resMsg", resMsg.ToString() },
                { "kcv", kcv.IsNull ? "" : kcv.ToString()}
            };

            responseBody = jsonObj.ToString();
        }

        private static void HandleValidateESignature(Dictionary<string, string> requestParams, out string responseBody)
        {
            var encryptedUserKey = System.Convert.FromBase64String(requestParams["userProfile"]);
            var decryptedUserKey = KeyGen.GetPlainKey(encryptedUserKey);

            var data = Encoding.UTF8.GetBytes(requestParams["data"]);
            Utils.Assert(data.Length > 0, "[Fatal] No document data supplied for validate");
            
            int macLength = Utils.ReadEnvVariableIntValue("SECURITY_FUNCTION_MAC_LENGTH");

            uint pinCode = System.Convert.ToUInt32(requestParams["code"]);

            bool res = new OTPCalculator(macLength,decryptedUserKey).ValidateOtp(pinCode, data);

            var jsonObj = new JObject
            {
                { "res", res.ToString() },
                { "resMsg", "OTP validation status: " + res.ToString() }
            };

            responseBody = jsonObj.ToString();
        }

        private static void HandleApplyESignature(Dictionary<string, string> requestParams, out string responseBody)
        {
            var encryptedUserKey = System.Convert.FromBase64String(requestParams["userProfile"]);
            var decryptedUserKey = KeyGen.GetPlainKey(encryptedUserKey);

            var data = Encoding.UTF8.GetBytes(requestParams["data"]);
            Utils.Assert(data.Length > 0, "[Fatal] No document data supplied for sign");

            int macLength = Utils.ReadEnvVariableIntValue("SECURITY_FUNCTION_MAC_LENGTH");

            uint pinCode = new OTPCalculator(macLength, decryptedUserKey).CalcOtpWithTime(0, data);

            var jsonObj = new JObject
            {
                { "res", true.ToString() },
                { "resMsg", "OTP calculation status: " + true.ToString()},
                { "code", pinCode.ToString("D5")}
            };

            responseBody = jsonObj.ToString();
        }

    }

}

