using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Безопасный указатель на сертификат
    /// </summary>
    internal class CertPtr : IDisposable
     {
          /// <summary>
          /// Неуправляемый указатель на сертификат 
          /// </summary>
          public IntPtr Cert = IntPtr.Zero;

          /// <summary>
          /// Освободить сертификат
          /// </summary>
          public void Release()
          {
               if (Cert != IntPtr.Zero)
               {
                    CAPI.CertFreeCertificateContext(Cert);
                    Cert = IntPtr.Zero;
               }
          }

          /// <summary>
          /// Деструктор
          /// </summary>
          ~CertPtr()
          {
               Release();
          }

          /// <summary>
          /// Освободить сертификат
          /// </summary>
          public void Dispose()
          {
               Release();
          }
     };
}
