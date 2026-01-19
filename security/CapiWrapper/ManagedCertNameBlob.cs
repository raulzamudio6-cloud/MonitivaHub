using System;
using System.Runtime.InteropServices;
using System.Text;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Имя в сертификате
    /// </summary>
    internal class ManagedCertNameBlob : IDisposable
     {
          /// <summary>
          /// Буфер содержащий имя в сертификате
          /// </summary>
          IntPtr buffer = IntPtr.Zero;

          /// <summary>
          /// Блоб с именем
          /// </summary>
          public CAPI.CERT_NAME_BLOB CertName = new CAPI.CERT_NAME_BLOB();

          /// <summary>
          /// Имя в строкой форме
          /// </summary>
          /// <param name="subject"></param>
          public ManagedCertNameBlob(string subject)
          {
               uint bufferSize = 0;
               StringBuilder errorStr = null; //todo: возможно это плохо, т.к. здесь могло быть сообщнгие об ошибке но неизвестно какого размера

               if (!CAPI.CertStrToName(1, subject, 0, IntPtr.Zero, buffer, ref bufferSize, errorStr))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               buffer = Marshal.AllocHGlobal((int)bufferSize);

               if (!CAPI.CertStrToName(1, subject, 0, IntPtr.Zero, buffer, ref bufferSize, errorStr))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               CertName.cbData = bufferSize;
               CertName.pbData = buffer;
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

          ~ManagedCertNameBlob()
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
