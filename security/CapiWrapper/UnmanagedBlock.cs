using System;
using System.Runtime.InteropServices;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Блок неуправляемой памяти
    /// </summary>
    internal class UnmanagedBlock : IDisposable
     {
          /// <summary>
          /// Указатель на память
          /// </summary>
          IntPtr buffer = IntPtr.Zero;

          /// <summary>
          /// Размер блока
          /// </summary>
          uint len = 0;

          /// <summary>
          /// Выделить блок нужного размера
          /// </summary>
          /// <param name="size">Размер</param>
          public UnmanagedBlock(int size) 
          {
               len = (uint)size;
               buffer = Marshal.AllocHGlobal(size);
          }

          /// <summary>
          /// Выделить блок нужного размера
          /// </summary>
          /// <param name="size">Размер</param>
          public UnmanagedBlock(uint size)
          {
               len = size;
               buffer = Marshal.AllocHGlobal((int)size);
          }

          /// <summary>
          /// Скопировать блок в неуправляемую память
          /// </summary>
          /// <param name="data">Блок</param>
          /// <param name="size">Размер блока</param>
          public UnmanagedBlock(byte[] data, int size)
          {
               len = (uint)size;
               buffer = Marshal.AllocHGlobal(size);
               Marshal.Copy(data, 0, buffer, data.Length);
          }

          /// <summary>
          /// Указатель на блок
          /// </summary>
          public IntPtr Ptr
          {
               get 
               {
                    return buffer;
               }
               private set
               {
                    buffer = value;
               }
          }

          /// <summary>
          /// Размер блока
          /// </summary>
          /// <returns></returns>
          public uint Size()
          {
               return len;
          }

          /// <summary>
          /// Освободить указатель на блок
          /// </summary>
          public void Release()
          {
               if (buffer != IntPtr.Zero)
               {
                    Marshal.FreeHGlobal(buffer);
                    buffer = IntPtr.Zero;
               }
          }

          /// <summary>
          /// Освободить указатель на блок
          /// </summary>
          ~UnmanagedBlock()
          {
               Release();
          }

          /// <summary>
          /// Освободить указатель на блок
          /// </summary>
          public void Dispose()
          {
               Release();
          }
     }
}
