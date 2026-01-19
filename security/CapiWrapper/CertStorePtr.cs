using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Безлрасный указатель на хранилище сертификатов
    /// </summary>
    internal class CertStorePtr : IDisposable
     {
          /// <summary>
          /// Неуправляемый указатель на хранилище сертификатов
          /// </summary>
          public IntPtr CertStore = IntPtr.Zero;

          /// <summary>
          /// Освободить хранилище сертификатов
          /// </summary>
          public void Release()
          {
               if (CertStore != IntPtr.Zero)
               {
                    CAPI.CertCloseStore(CertStore, 0);
                    CertStore = IntPtr.Zero;
               }
          }

          /// <summary>
          /// Деструктор
          /// </summary>
          ~CertStorePtr()
          {
               Release();
          }

          /// <summary>
          /// Освободить хранилище сертификатов
          /// </summary>
          public void Dispose()
          {
               Release();
          }
     };
}
