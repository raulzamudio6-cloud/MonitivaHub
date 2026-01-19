using System;
using System.ComponentModel;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    public class UserProfileCreator
     {
          KeyGenerator core = null;

          public UserProfileCreator(ProviderSettings settings)
          {
               core = new KeyGenerator(settings);
          }

          /// <summary>
          /// Создает профиль пользователя
          /// </summary>
          /// <param name="user">ID пользователя</param>
          /// <param name="result">Результат операции</param>
          /// <returns>профиль</returns>
          byte[] CreateProfile(int user, FunctionResult result)
          {
               byte[] otpKey = null;
               byte[] cmacKey = null;
               byte[] crKey = null;


               result.ResultOperation = true;
               result.Add(core.GenOtpKey(user, ref otpKey));
               result.Add(core.GenCmacKey(user, ref cmacKey));
               result.Add(core.GenKeyForChallengeRequest(user, ref crKey));
            
               if (!result.ResultOperation)
               {
                    return null;
               }

               byte[] profile = new byte[3 + otpKey.Length + cmacKey.Length + crKey.Length];
               
               int index = 0;
               
               profile[index++] = (byte)otpKey.Length;
               
               for (int i = 0; i < otpKey.Length; ++i)
               {
                    profile[index++] = otpKey[i];
               }

               profile[index++] = (byte)cmacKey.Length;
               
               for (int i = 0; i < cmacKey.Length; ++i)
               {
                    profile[index++] = cmacKey[i];
               }

               profile[index++] = (byte)crKey.Length;

               for (int i = 0; i < crKey.Length; ++i)
               {
                    profile[index++] = crKey[i];
               }

               return profile;
          }

          /// <summary>
          /// Профиль пользователя создается в памяти
          /// </summary>
          /// <param name="userID">ID пользователя</param>
          /// <param name="profile">Профиль</param>
          /// <param name="counter">Счетчик одноразовых паролей</param>
          /// <returns>Результат выполнения операции</returns>
          public FunctionResult CreateProfileToMemory(Int32 userID, ref byte[] profile, ref UInt32 counter)
          {
               FunctionResult result = new FunctionResult();
               result.AddToLog(string.Format("Start user ({0}) profile creation", userID));

               try
               {
                    profile = CreateProfile(userID, result);
                    
                    if (profile == null || !result.ResultOperation)
                    {
                         result.ResultOperation = false;
                         return result;
                    }
                    counter = 0;
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("CAPI ERROR! \"{1}\"Code: 0x{0:x} ; Source:{2}; StackTrace: {3}.", ex.Error, new Win32Exception(ex.Error).Message, ex.Source, ex.StackTrace));
                    return result;
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("System ERROR! {0}; Source:{1}; StackTrace: {2}.", ex.Message, ex.Source, ex.StackTrace));
                    return result;
               }

               result.ResultOperation = true;
               return result;
          }

          
     }
}
