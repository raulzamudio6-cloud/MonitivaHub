using System;
using System.Runtime.InteropServices;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    internal class Sha : IDisposable
     {
          ProvPtr prov = new ProvPtr();
          HashPtr hash = new HashPtr();
          private bool final = false;
          private byte[] val = null;

          public Sha()
          {
              if (!CAPI.CryptAcquireContext(ref prov.Prov, IntPtr.Zero, "Microsoft Strong Cryptographic Provider", 1, (uint)CAPI.ContextFlags.CRYPT_VERIFYCONTEXT | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               if (!CAPI.CryptCreateHash(prov.Prov, (uint)CAPI.ALG_ID.CALG_SHA, IntPtr.Zero, 0, ref hash.Hash))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }
          }
          public void Add(byte[] input)
          {
               if (final)
               {
                    throw new SystemException("Sha already finalize.");
               }

               IntPtr inputPtr = Marshal.AllocHGlobal(input.Length);

               try
               {
                    Marshal.Copy(input, 0, inputPtr, input.Length);
                    if (!CAPI.CryptHashData(hash.Hash, inputPtr, (uint)input.Length, 0))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }
               }
               finally
               {
                    Marshal.FreeHGlobal(inputPtr);
               }
          }
          public byte[] Calc()
          {
               if (final)
               {
                    return val;
               }

               final = true;

               IntPtr valuePtr = IntPtr.Zero;
               uint hashSize = 0;

               if (!CAPI.CryptGetHashParam(hash.Hash, (uint)CAPI.HashParam.HP_HASHVAL, valuePtr, ref hashSize, 0))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               try
               {
                    valuePtr = Marshal.AllocHGlobal((int)hashSize);

                    if (!CAPI.CryptGetHashParam(hash.Hash, (uint)CAPI.HashParam.HP_HASHVAL, valuePtr, ref hashSize, 0))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    val = new byte[hashSize];
                    Marshal.Copy(valuePtr, val, 0, (int)hashSize);
               }
               finally
               {
                    Marshal.FreeHGlobal(valuePtr);
               }

               return val;
          }

          public void Release()
          {
               hash.Release();
               prov.Release();
          }

          ~Sha()
          {
               Release();
          }
          public void Dispose()
          {
               Release();
          }

     }
}
