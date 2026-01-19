using System;
using System.ComponentModel;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Генерация ключей
    /// </summary>
    internal class KeyGenerator
     {
          ProviderSettings Settings = null;
          public KeyGenerator(ProviderSettings settings)
          {
               Settings = settings;
          }
          
          /// <summary>
          /// Генерация ключа для OTP токена
          /// </summary>
          /// <param name="userID">ID пользлвателя</param>
          /// <param name="secretKey">Секретный ключ</param>
          /// <returns>Результат операции</returns>
          public FunctionResult GenOtpKey(int userID, ref byte[] secretKey)
          {
               FunctionResult result = new FunctionResult();

               try
               {
                    result.AddToLog(String.Format("Generated OTP key for user {0}", userID));
                    secretKey = CryptoFactory.GenData(Settings.Name, Settings.Type, 20);
                    result.AddToLog(String.Format("Generated OTP key for user {0} success!", userID));
                    result.ResultOperation = true;
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("Generated OTP key for user {0} failed! Error: {2}, code: 0x{1:x}", userID, ex.Error, ex.Text));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("{2} Generated OTP key for user {0} failed! Error: {1}", userID, ex.Message));
               }

               return result;
          }

          public FunctionResult GenCmacKey(int userID, ref byte[] secretKey)
          {
               FunctionResult result = new FunctionResult();

               try
               {
                    result.AddToLog(string.Format("Generation mac key for user ({0})", userID));
                    
                    secretKey = CryptoFactory.GenCmacKey(Settings.Name, Settings.Type);

                    result.AddToLog(String.Format("Generated CMAC key for user {0} success!", userID));
                    result.ResultOperation = true;
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("Generated CMAC key for user {0} failed! Error: {1}, code: 0x{2:x}", userID, ex.Error, new Win32Exception(ex.Error).Message));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("Generated CMAC key for user {0} failed! Error: {1}", userID, ex.Message));
               }

               return result;
          }

          
          public FunctionResult GenChallenge(int userID, ref byte[] challenge)
          {
               FunctionResult result = new FunctionResult();
               try
               {
                    result.AddToLog(String.Format("Generated challenge for user {0}.", userID));
                    challenge = CryptoFactory.GenData(Settings.Name, Settings.Type, 4);
                    //challenge = (uint)data[0] << 24;
                    //challenge |= (uint)data[1] << 16;
                    //challenge |= (uint)data[2] << 8;
                    //challenge |= (uint)data[3];

                    result.AddToLog(String.Format("Generated challenge for user {0} success!", userID));
                    result.ResultOperation = true;
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("Generated challenge for user {0} failed! Error: {2}, code: 0x{1:x}", userID, ex.Error, new Win32Exception(ex.Error).Message));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("Generated challenge for user {0} failed! Error: {1}", userID, ex.Message));
               }

               return result;
          }

          public FunctionResult GenKeyForChallengeRequest(int userID, ref byte[] secretKey)
          {
               FunctionResult result = new FunctionResult();
               try
               {
                    result.AddToLog(String.Format("Generated Challenge-Request key for user {0}.", userID));
                    secretKey = CryptoFactory.GenCmacKey(Settings.Name, Settings.Type);

                    result.AddToLog(String.Format("Generated Challenge-Request key for user {0} success!", userID));
                    result.ResultOperation = true;
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("Generated Challenge-Request key for user {0} failed! Error: {2}, code: 0x{1:x}", userID, ex.Error, new Win32Exception(ex.Error).Message));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("Generated Challenge-Request key for user {0} failed! Error: {1}", userID, ex.Message));
               }

               return result;
          }
     }
}
