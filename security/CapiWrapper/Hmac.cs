using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// HMAC хеш алгоритм
    /// </summary>
    public class Hmac : IDisposable
     {
          /// <summary>
          /// вычисление hmac закончено
          /// </summary>
          bool final = false;

          /// <summary>
          /// размер блока алгортма
          /// </summary>
          const int blockSize = 64;

          /// <summary>
          /// первый хеш объект
          /// </summary>
          Sha m_ipad = new Sha();

          /// <summary>
          /// второй хеш объект
          /// </summary>
          Sha m_opad = new Sha();

          /// <summary>
          /// значение hmac
          /// </summary>
          byte[] val = new byte[blockSize];

          /// <summary>
          /// Устанавливает секретный ключ
          /// </summary>
          /// <param name="key">Секретный ключ</param>
          public void SetKey(byte[] key)
          {
               byte[] K = new byte[blockSize];

               for (int i = 0; i < blockSize; ++i)
               {
                    K[i] = 0;
               }

               if (key.Length > blockSize)
               {
                    using (Sha hash = new Sha())
                    {
                         hash.Add(key);
                         byte[] temp = hash.Calc();

                         for (int i = 0; i < temp.Length; ++i)
                         {
                              K[i] = temp[i];
                         }
                    }
               }
               else 
               {
                    for (int i = 0; i < key.Length; ++i)
                    {
                         K[i] = key[i];
                    }

                    if (blockSize != key.Length)
                    {
                         for(int i = key.Length; i < blockSize; ++i)
                         {
                              K[i] = 0;
                         }
                    }
               }

               byte[] Sipad = new byte[blockSize];
               byte[] Sopad = new byte[blockSize];

               for (int i = 0; i < blockSize; ++i)
               {
                    Sipad[i] = (byte)(K[i] ^ 0x36);
                    Sopad[i] = (byte)(K[i] ^ 0x5c);
               }

               m_ipad.Add(Sipad);
               m_opad.Add(Sopad);
          }

          /// <summary>
          /// Добавляет данные к хешу
          /// </summary>
          /// <param name="password"></param>
          public void Add(byte[] password)
          {
               m_ipad.Add(password);
          }

          /// <summary>
          /// Вычисляет хеш
          /// </summary>
          void Calc()
          {
               byte[] tmp = m_ipad.Calc();
               m_opad.Add(tmp);
               val = m_opad.Calc();
          }

          /// <summary>
          /// Возвращает значение hmac
          /// </summary>
          /// <returns>HMAC</returns>
          public byte[] GetValue()
          {
               if (!final)
               {
                    Calc();
               }

               final = true;

               return val;
          }

          /// <summary>
          /// Освободить ресурсы
          /// </summary>
          public void Release()
          {
               m_ipad.Release();
               m_opad.Release();
          }

          ~Hmac()
          {
               Release();
          }

          /// <summary>
          /// Освободить ресурсы
          /// </summary>
          public void Dispose()
          {
               Release();
          }
     }     
}
