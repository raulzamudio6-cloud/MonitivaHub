using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Аутентифицирует пользователя
    /// </summary>
    public class Authentificator
     {
          /// <summary>
          /// Профиль пользователя
          /// </summary>
          private byte[] m_profile = null;

          /// <summary>
          /// ID пользователя
          /// </summary>
          Int32 m_user = 0;

          /// <summary>
          /// Настройки криптосистемы
          /// </summary>
          ProviderSettings Settings = null;
          
          /// <summary>
          /// Аутентификация пользователя
          /// </summary>
          /// <param name="user">ID пользователя</param>
          /// <param name="profile">Профиль</param>
          /// <param name="settings">Настройки криптосистемы</param>
          public Authentificator(Int32 user, byte[] profile, ProviderSettings settings)
          {
               m_profile = profile;
               m_user = user;
               Settings = settings;
          }

          /// <summary>
          /// Аутентификация по одноразовому паролю
          /// <param name="tokenValue">пароль</param>
          /// <param name="counter">счетчик успешных аутентификаций</param>
          /// <returns>Результат операции</returns>
          public FunctionResult OTP(UInt32 tokenValue, ref UInt32 counter)
          {
               FunctionResult result = new FunctionResult();
               
               if (m_profile == null)
               {
                    result.AddToLog(string.Format("OTP Authetication for user: {0}. Error. Profile is null.", m_user));
                    return result;
               }

               try
               {
                    // Размер ключа
                    byte[] key = new byte[m_profile[0]];

                    // Копируем ключ
                    for (int i = 0; i < key.Length; ++i)
                    {
                         key[i] = m_profile[i + 1];
                    }

                    // Создаем генератор паролей
                    OtpGenerator gen = new OtpGenerator(key, counter);

                    // Проверяем одноразовый пароль
                    result.ResultOperation = gen.Check(tokenValue);

                    // Если ауьентификация прошла успешно, увеличиваем счетчик 
                    if (result.ResultOperation)
                    {
                         counter++;
                    }

                    result.AddToLog(string.Format("OTP authentication for user {0} {1}!", m_user, result.ResultOperation ? "success" : "failed"));
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("OTP Authetication for user: {0} failed! Error: {2}, code: 0x{1:x}", m_user, ex.Error, ex.Text));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("OTP Authetication for user: {0} failed! Error: {1}", m_user, ex.Message));
               }

               return result;
          }

          /// <summary>
          /// Проверка подписи данных пользователя
          /// </summary>
          /// <param name="data">Данные</param>
          /// <param name="macValue">Подпись</param>
          /// <returns></returns>
          public FunctionResult CMAC(byte[] data, byte[] macValue)
          {
               FunctionResult result = new FunctionResult();

               if(m_profile == null)
               {
                    result.AddToLog(string.Format("MAC Authetication for user: {0}. Error. Profile is null.", m_user));
                    return result;
               }

               try
               {
                    // получаем смещение ключа в профыйле
                    int offset = m_profile[0]+1;
                    
                    // получаем длинну ключа
                    byte[] key = new byte[m_profile[offset]];
                    
                    // получаем ключ
                    for(int i = 0; i < key.Length; ++i)
                    {
                         key[i] = m_profile[offset+i+1];
                    }

                    // создаем генератор подписей
                    Cmac gen = new Cmac(key, Settings);

                    // данные, которые были подписаны
                    gen.Add(data);

                    // создаем подпись
                    byte[] sign = gen.Calc();
               
                    // сверяем подпись
                    if (sign.Length != macValue.Length)
                    {
                         result.AddToLog(string.Format("MAC authentication for user {0} failed! mac corrupt.", m_user));
                         return result;
                    }

                    for (int i = 0; i < sign.Length; ++i)
                    {
                         if (sign[i] != macValue[i])
                         {
                              result.AddToLog(string.Format("MAC authentication for user {0} failed!", m_user));
                              return result;
                         }
                    }

                    result.AddToLog(string.Format("MAC authentication for user {0} sucess!", m_user));
                    result.ResultOperation = true;
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("MAC Authetication for user: {0} failed! Error: {2}, code: 0x{1:x}", m_user, ex.Error, ex.Text));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("MAC Authetication for user: {0} failed! Error: {1}", m_user, ex.Message));
               }

               return result;
          }

          /// <summary>
          /// Аутентифткация пользователя по схеме запрос-ответ
          /// </summary>
          /// <param name="challenge">запрос</param>
          /// <param name="macValue">ответ</param>
          /// <returns></returns>
          public FunctionResult ChallengeResponse(byte[] challenge, byte[] macValue)
          {

               FunctionResult result = new FunctionResult();

               if (m_profile == null)
               {
                    result.AddToLog(string.Format("CR Authetication for user: {0}. Error. Profile is null.", m_user));
                    return result;
               }

               // получаем смещение ключа 
               int offset = m_profile[0] + 1;
               byte[] key = new byte[m_profile[offset]];

               // получаем ключ
               for (int i = 0; i < key.Length; ++i)
               {
                    key[i] = m_profile[offset + i + 1];
               }

               try
               {
                    // создаем генератор подписей
                    Cmac gen = new Cmac(key, Settings);
                    
                    // подписываем challenge
                    gen.Add(challenge);
                    
                    // генерируем response
                    byte[] sign = gen.Calc();

                    // cверяем response
                    if (sign.Length != macValue.Length)
                    {
                         result.AddToLog(string.Format("Challenge-Response authentication for user {0} failed! mac corrupt.", m_user));
                         return result;
                    }

                    for (int i = 0; i < sign.Length; ++i)
                    {
                         if (sign[i] != macValue[i])
                         {
                              result.AddToLog(string.Format("CR Authetication for user: {0} failed!", m_user));
                              return result;
                         }
                    }

                    result.AddToLog(string.Format("CR Authetication for user: {0} success!", m_user));
                    result.ResultOperation = true;

               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("CR Authetication for user: {0} failed! Error: {2}, code: 0x{1:x}", m_user, ex.Error, ex.Text));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("CR Authetication for user: {0} failed! Error: {1}", m_user, ex.Message));
               }

               return result;
          }

          /// <summary>
          /// Генерация случайного числа
          /// </summary>
          /// <param name="challenge">случайное число</param>
          /// <returns></returns>
          public FunctionResult GenerateChallenge(ref byte[] challenge)
          {
               KeyGenerator core = new KeyGenerator(Settings);
               return core.GenChallenge(m_user, ref challenge);
          }
     }
}
