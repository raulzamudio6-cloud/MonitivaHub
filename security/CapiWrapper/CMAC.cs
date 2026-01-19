using System;
using System.Runtime.InteropServices;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// MAC генератор
    /// </summary>
    internal class Cmac : IDisposable
     {
          /// <summary>
          /// Провайдер
          /// </summary>
          ProvPtr prov = new ProvPtr();
          
          /// <summary>
          /// MAC хеш
          /// </summary>
          HashPtr hash = new HashPtr();
          
          /// <summary>
          /// Секретный ключ генератора 
          /// </summary>
          KeyPtr key = new KeyPtr();

          /// <summary>
          /// Значение генератор вычесленно. Значение можно вычислять только один раз.
          /// </summary>
          private bool final = false;

          /// <summary>
          /// Значение генератор
          /// </summary>
          private byte[] val = null;

          /// <summary>
          /// MAC генератор
          /// </summary>
          /// <param name="keyBlob">Секретный ключ</param>
          public Cmac(byte[] keyBlob, ProviderSettings settings)
          {
               // поделючаемся к провайдеру
              if (!CAPI.CryptAcquireContext(ref prov.Prov, IntPtr.Zero, settings.Name, settings.Type, (uint)CAPI.ContextFlags.CRYPT_VERIFYCONTEXT | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               // задаем секретный ключ 
               if (!CAPI.CryptImportKey(prov.Prov, keyBlob, (uint)keyBlob.Length, IntPtr.Zero, 0, ref key.Key))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error); 
               }

               // создаем генератор 
               if (!CAPI.CryptCreateHash(prov.Prov, (uint)CAPI.ALG_ID.CALG_MAC, key.Key, 0, ref hash.Hash))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }
          }

          /// <summary>
          /// Добавляем поле с данными в генератор
          /// </summary>
          /// <param name="data">данные</param>
          public void Add(byte[] data)
          {
               // если значение генератор уже было вычесленно, то добовлять новое поле нельзя!
               if (final)
               {
                    throw new SystemException("CMAC already finalize.");
               }

               // добавляем поле 
               using (UnmanagedBlock buffer = new UnmanagedBlock(data, data.Length))
               {
                    if (!CAPI.CryptHashData(hash.Hash, buffer.Ptr, (uint)data.Length, 0))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }
               }
          }

          /// <summary>
          /// Вычисляем значение генератора
          /// </summary>
          /// <returns>Значение генератора</returns>
          public byte[] Calc()
          {
               // если значение уже вычислялось, то вернем его без изменений 
               if (final)
               {
                    return val;
               }


               // значение вычисляется один раз
               final = true;

               // высичляем значение
               IntPtr valuePtr = IntPtr.Zero;

               uint hashSize = 0;

               if (!CAPI.CryptGetHashParam(hash.Hash, (uint)CAPI.HashParam.HP_HASHVAL, valuePtr, ref hashSize, 0))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               using (UnmanagedBlock buffer = new UnmanagedBlock(hashSize))
               {
                    if (!CAPI.CryptGetHashParam(hash.Hash, (uint)CAPI.HashParam.HP_HASHVAL, buffer.Ptr, ref hashSize, 0))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    val = new byte[hashSize];
                    Marshal.Copy(buffer.Ptr, val, 0, (int)hashSize);
               }
               
               return val;
          }

          /// <summary>
          /// Освобождаем генератор
          /// </summary>
          public void Release()
          {
               hash.Release();
               key.Release();
               prov.Release();
          }

          /// <summary>
          /// Освобождаем генератор
          /// </summary>
          ~Cmac()
          {
               Release();
          }

          /// <summary>
          /// Освобождаем генератор
          /// </summary>
          public void Dispose()
          {
               Release();
          }
     }
}
