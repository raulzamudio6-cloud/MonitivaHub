using System;
using System.Runtime.InteropServices;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    internal class ManagedCapiBlob : IDisposable
     {
          /// <summary>
          /// Буфер содержащий имя в сертификате
          /// </summary>
          IntPtr buffer = IntPtr.Zero;

          /// <summary>
          /// Блоб с именем
          /// </summary>
          public CAPI.CRYPTOAPI_BLOB blob = new CAPI.CRYPTOAPI_BLOB();

          /// <summary>
          /// Имя в строкой форме
          /// </summary>
          /// <param name="subject"></param>
          public ManagedCapiBlob(byte[] data)
          {
               buffer = Marshal.AllocHGlobal(data.Length);
               Marshal.Copy(data, 0, buffer, data.Length);
               blob.cbData = (uint)data.Length;
               blob.pbData = buffer;
               
          }

          /// <summary>
          /// Освободить неуправляемые ресурсы
          /// </summary>
          public void Release()
          {
               if (buffer != IntPtr.Zero)
               {
                    Marshal.FreeHGlobal(buffer);
               }
               buffer = IntPtr.Zero;
          }

          ~ManagedCapiBlob()
          {
               Release();
          }

          /// <summary>
          /// Освободить неуправляемые ресурсы
          /// </summary>
          public void Dispose()
          {
               Release();
          }
     }
}
