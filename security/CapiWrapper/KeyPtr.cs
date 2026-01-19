using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Указатель на ключ
    /// </summary>
    internal class KeyPtr : IDisposable
     {
          /// <summary>
          /// Неуправляемый указатель на ключ
          /// </summary>
          public IntPtr Key = IntPtr.Zero;

          /// <summary>
          /// Освободить ресурс
          /// </summary>
          public void Release()
          {
               if (Key != IntPtr.Zero)
               {
                    CAPI.CryptDestroyKey(Key);
                    Key = IntPtr.Zero;
               }
          }

          ~KeyPtr()
          {
               Release();
          }

          /// <summary>
          /// Освободить ресурс
          /// </summary>
          public void Dispose()
          {
               Release();
          }
     };
}