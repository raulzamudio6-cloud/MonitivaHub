using System;
using System.Runtime.InteropServices;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    internal class UnManagedArray : IDisposable
     {
          public IntPtr buffer = IntPtr.Zero;
          public int Size = 0;

          public void Alloc(int size)
          {
               buffer = Marshal.AllocHGlobal((int)size);
               Size = size;
          }

          public void Release()
          {
               if (buffer != IntPtr.Zero)
               {
                    Marshal.FreeHGlobal(buffer);
               }

               buffer = IntPtr.Zero;
          }
          public void Dispose()
          {
               Release();
          }

          public byte[] CopyToArray()
          {
               byte[] copy = new byte[Size];
               Marshal.Copy(buffer, copy, 0, Size);
               return copy;
          }

          ~UnManagedArray()
          {
               Release();
          }
     }
}
