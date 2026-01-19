using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Генератор OTP пароля
    /// </summary>
    internal class OtpGenerator : IDisposable
     {
          /// <summary>
          /// секретный ключ
          /// </summary>
          byte[] m_key = null;

          /// <summary>
          /// счетчик
          /// </summary>
          uint m_counter;

          /// <summary>
          /// Увеличивает счетчик на 1
          /// </summary>
          /// <param name="counter">Счетчик</param>
          void IncrementCounter(byte[] counter)
          {
               for (int i = 0; i < 8; ++i)
               {
                    if (counter[8 - i - 1] != 0xff)
                    {
                         ++counter[8 - i - 1];
                         break;
                    }

                    counter[8 - i - 1] = 0;
               }
          }

          /// <summary>
          /// Генерирует значение OTP
          /// </summary>
          /// <param name="counter">счетчик</param>
          /// <returns>значение OTP</returns>
          uint CreateValue(uint counter)
          {
               byte[] tmpCounter = new byte[8];

               tmpCounter[7] = (byte)(counter & 0x00ff);
               tmpCounter[6] = (byte)(counter >> 8);

               return CreateValue(tmpCounter);
          }

          /// <summary>
          /// Генерирует значение OTP
          /// </summary>
          /// <param name="counter">счетчик</param>
          /// <returns>значение OTP</returns>
          uint CreateValue(byte[] counter)
          {
               Hmac hmac = new Hmac();
               hmac.SetKey(m_key);
               hmac.Add(counter);

               byte[] val = new byte[20];
               val = hmac.GetValue();
               byte offset = (byte)(val[19] & 0x0f);
               uint dbc = (uint)((val[offset] & 0x7f) << 24 | val[offset + 1] << 16 | val[offset + 2] << 8 | val[offset + 3]);
               dbc = dbc % 1000000;

               return dbc;
          }

          /// <summary>
          /// Возвращает следующее значение счетчика по значение генератора от преведущего значения счетчика
          /// </summary>
          /// <param name="otpVal"></param>
          /// <returns></returns>
          uint GetCounterIndex(uint otpVal)
          {
               byte[] counter = new byte[8];
               for (int i = 0; i < 8; ++i)
               {
                    counter[i] = 0;
               }

               for (uint i = 0; i < 14000; ++i)
               {
                    IncrementCounter(counter);
                    uint curVal = CreateValue(counter);
                    if (curVal == otpVal)
                    {
                         return i + 1;
                    }
               }
               throw new SystemException("OTP value not found.");
          }

          /// <summary>
          /// Генератор
          /// </summary>
          /// <param name="key">Секретный ключ</param>
          public OtpGenerator(byte[] key)
          {
               m_key = new byte[key.Length];
               for (int i = 0; i < key.Length; ++i)
               {
                    m_key[i] = key[i];
               }
               m_counter = 0;
          }

          /// <summary>
          /// Генератор
          /// </summary>
          /// <param name="key">Секретный ключ</param>
          public OtpGenerator(byte[] key, uint counter)
          {
               m_key = new byte[key.Length];
               for (int i = 0; i < key.Length; ++i)
               {
                    m_key[i] = key[i];
               }
               m_counter = counter;
          }

          /// <summary>
          /// Синхронизирует генератор по двум последовательным значениям генератора
          /// </summary>
          /// <param name="valFst">Первое значение генератора</param>
          /// <param name="valSnd">Второе значение генератора</param>
          /// <returns>Удалось ли синхронизировать</returns>
          public bool SenchronizationCounter(uint valFst, uint valSnd)
          {
               uint fistValPos = GetCounterIndex(valFst);
               uint nextVal = CreateValue(fistValPos + 1);
               if (nextVal == valSnd)
               {
                    m_counter = fistValPos + 2;
                    return true;
               }
               return false;
          }

          /// <summary>
          /// Проверить значение генератора, если значение верное счетчик увеличится на 1
          /// </summary>
          /// <param name="val">Значение генератора</param>
          /// <returns>Верное ли значение</returns>
          public bool Check(uint val)
          {
               uint checkVal = CreateValue(m_counter);
               if (checkVal != val)
               {
                    return false;
               }
               ++m_counter;
               return true;
          }

          /// <summary>
          /// Генерирует следующее значение генератор
          /// </summary>
          /// <returns>Следующее значение генератор</returns>
          public uint NextValue()
          {
               uint val = CreateValue(m_counter);
               ++m_counter;
               return val;
          }

          public void Dispose()
          {
               //метод пустой, сделан для единого интрерфейса
          }
     }
}
