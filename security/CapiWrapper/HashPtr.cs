using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Указатель на хеш
    /// </summary>
    internal class HashPtr : IDisposable
     {
          /// <summary>
          /// Неуправляемый указатель на хеш
          /// </summary>
          public IntPtr Hash = IntPtr.Zero;

          /// <summary>
          /// Особождает ресурс
          /// </summary>
          public void Release()
          {
               if (Hash != IntPtr.Zero)
               {
                    CAPI.CryptDestroyHash(Hash);
                    Hash = IntPtr.Zero;
               }
          }

          ~HashPtr()
          {
               Release();
          }

          /// <summary>
          /// Особождает ресурс
          /// </summary>
          public void Dispose()
          {
               Release();
          }
     };
}
