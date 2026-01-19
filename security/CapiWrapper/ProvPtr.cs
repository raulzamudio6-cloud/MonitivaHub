using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    public class ProvPtr : IDisposable
     {
          public IntPtr Prov = IntPtr.Zero;

          public void Release()
          {
               if (Prov != IntPtr.Zero)
               {
                    CAPI.CryptReleaseContext(Prov, 0);
                    Prov = IntPtr.Zero;
               }
          }

          ~ProvPtr()
          {
               Release();
          }

          public void Dispose()
          {
               Release();
          }
     };
}
