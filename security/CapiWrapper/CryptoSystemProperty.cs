using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
     /// <summary>
     /// Параметры криптоядра
     /// </summary>
     internal class CryptoSystemProperty
     {
          /// <summary>
          /// Возвращает список ключей
          /// </summary>
          /// <param name="nameProv">Имя провайдера</param>
          /// <param name="typeProv">Тип провайдера</param>
          /// <returns>Список ключей</returns>
          public static List<string> GetContainerList(string nameProv, uint typeProv)
          {
               List<string> containerList = new List<string>();
               using (ProvPtr prov = new ProvPtr())
               {
                    if (!CAPI.CryptAcquireContext(ref prov.Prov, IntPtr.Zero, nameProv, typeProv, (uint)CAPI.ContextFlags.CRYPT_VERIFYCONTEXT | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    uint flag = (uint)CAPI.EnumFlags.CRYPT_FIRST;
                    for (; ; )
                    {
                         uint bufferSize = 0;
                         StringBuilder buffer = null;
                         if (!CAPI.CryptGetProvParam(prov.Prov, CAPI.ProviderParamType.PP_ENUMCONTAINERS, buffer, ref bufferSize, flag))
                         {
                              int error = Marshal.GetLastWin32Error();

                              if (error == 0x00000103)
                              {
                                   break;
                              }

                              throw new CapiException(error);
                         }

                         buffer = new StringBuilder((int)bufferSize);

                         if (!CAPI.CryptGetProvParam(prov.Prov, CAPI.ProviderParamType.PP_ENUMCONTAINERS, buffer, ref bufferSize, flag))
                         {
                              int error = Marshal.GetLastWin32Error();

                              if (error == 0x00000103)
                              {
                                   break;
                              }

                              throw new CapiException(error);
                         }

                         if (!String.IsNullOrEmpty(buffer.ToString()))
                         {
                              containerList.Add(buffer.ToString());
                         }

                         flag = (uint)CAPI.EnumFlags.CRYPT_NEXT;
                    }
               }
               return containerList;
          }

          /// <summary>
          /// Возвращает список алгоритмов
          /// </summary>
          /// <param name="provName">Имя провайдера</param>
          /// <param name="provType">Тип провайдера</param>
          /// <returns>Список ключей</returns>
          private static List<KeyValuePair<CAPI.ALG_ID, string>> GetAllProvAlgs(string provName, uint provType)
          {
               List<KeyValuePair<CAPI.ALG_ID, string>> algs = new List<KeyValuePair<CAPI.ALG_ID, string>>();

               using (ProvPtr prov = new ProvPtr())
               {
                   if (!CAPI.CryptAcquireContext(ref prov.Prov, IntPtr.Zero, provName, provType, (uint)CAPI.ContextFlags.CRYPT_VERIFYCONTEXT | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    uint flag = (uint)CAPI.EnumFlags.CRYPT_FIRST;
                    for (; ; )
                    {
                         uint bufferSize = 0;
                         CAPI.PROV_ENUMALGS buffer = null;
                         if (!CAPI.CryptGetProvParam(prov.Prov, CAPI.ProviderParamType.PP_ENUMALGS, buffer, ref bufferSize, flag))
                         {
                              int error = Marshal.GetLastWin32Error();

                              if (error == 0x00000103)
                              {
                                   break;
                              }

                              throw new CapiException(error);
                         }

                         buffer = new CAPI.PROV_ENUMALGS();

                         if (!CAPI.CryptGetProvParam(prov.Prov, CAPI.ProviderParamType.PP_ENUMALGS, buffer, ref bufferSize, flag))
                         {
                              int error = Marshal.GetLastWin32Error();

                              if (error == 0x00000103)
                              {
                                   break;
                              }

                              throw new CapiException(error);
                         }
                         flag = (uint)CAPI.EnumFlags.CRYPT_NEXT;

                         string name = buffer.szName;
                         algs.Add(new KeyValuePair<CAPI.ALG_ID, string>(buffer.aiAlgid, name));
                    }
               }

               return algs;
          }

          /// <summary>
          /// Возвращает типы криптопровайдеров
          /// </summary>
          /// <returns>Типы криптопровайдеров</returns>
          public static List<KeyValuePair<uint, string>> GetAllTypes()
          {
               List<KeyValuePair<uint, string>> types = new List<KeyValuePair<uint, string>>();
               for (uint index = 0; ; ++index)
               {
                    uint provType = 0;
                    uint typeNameLen = 0;
                    StringBuilder typeName = null;

                    if (!CAPI.CryptEnumProviderTypes(index, IntPtr.Zero, 0, ref provType, typeName, ref typeNameLen))
                    {
                         int error = Marshal.GetLastWin32Error();

                         if (error == 0x00000103)
                         {
                              break;
                         }

                         throw new CapiException(error);
                    }

                    typeName = new StringBuilder((int)typeNameLen);

                    if (!CAPI.CryptEnumProviderTypes(index, IntPtr.Zero, 0, ref provType, typeName, ref typeNameLen))
                    {
                         int error = Marshal.GetLastWin32Error();

                         if (error == 0x00000103)
                         {
                              break;
                         }

                         throw new CapiException(error);
                    }

                    types.Add(new KeyValuePair<uint, string>(provType, typeName.ToString()));
               }

               return types;
          }

          /// <summary>
          /// Возвращает имя типа провайдера
          /// </summary>
          /// <param name="list">Список типов провайдера</param>
          /// <param name="type">Номер типа провайдера</param>
          /// <returns>Имя типа провайдера</returns>
          public static string GetTypeName(List<KeyValuePair<uint, string>> list, uint type)
          {
               foreach (KeyValuePair<uint, string> entity in list)
               {
                    if (entity.Key == type)
                    {
                         return entity.Value;
                    }
               }

               throw new ArgumentOutOfRangeException("Name of provider type is not found.");
          }

          /// <summary>
          /// Возвращает номер типа провайдера
          /// </summary>
          /// <param name="list">Список типов провайдера</param>
          /// <param name="name">Имя типа провайдера</param>
          /// <returns>Номер типа провайдера</returns>
          public static uint GetProvType(List<KeyValuePair<string, uint>> list, string name)
          {
               foreach (KeyValuePair<string, uint> entity in list)
               {
                    if (entity.Key == name)
                    {
                         return entity.Value;
                    }
               }

               throw new ArgumentOutOfRangeException("Name of provider type is not found.");
          }

          /// <summary>
          /// Возвращает список типов провайдеров
          /// </summary>
          /// <returns>Типы провайдеров имя и номер типа</returns>
          public static List<KeyValuePair<string, uint>> GetAllProves()
          {
               List<KeyValuePair<string, uint>> provList = new List<KeyValuePair<string, uint>>();
               for (uint index = 0; ; ++index)
               {
                    ProviderInfo info = new ProviderInfo();
                    uint provType = 0;
                    StringBuilder name = null;
                    uint nameLen = 0;

                    if (!CAPI.CryptEnumProviders(index, IntPtr.Zero, 0, ref provType, name, ref nameLen))
                    {
                         int error = Marshal.GetLastWin32Error();

                         if (error == 0x00000103)
                         {
                              break;
                         }

                         throw new CapiException(error);
                    }

                    name = new StringBuilder((int)nameLen);

                    if (!CAPI.CryptEnumProviders(index, IntPtr.Zero, 0, ref provType, name, ref nameLen))
                    {
                         int error = Marshal.GetLastWin32Error();

                         if (error == 0x00000103)
                         {
                              break;
                         }

                         throw new CapiException(error);
                    }

                    provList.Add(new KeyValuePair<string, uint>(name.ToString(), provType));
               }

               return provList;
          }

          /// <summary>
          /// Возвращает информацию о всех криптопровайдуров установленных в системе
          /// </summary>
          /// <returns>Список провайдеров</returns>
          internal static List<ProviderInfo> GetAllInfo()
          {
               List<ProviderInfo> info = new List<ProviderInfo>();
               List<KeyValuePair<uint, string>> types = GetAllTypes();


               foreach (KeyValuePair<string, uint> name in GetAllProves())
               {
                    info.Add(GetProvInfo(name.Key, name.Value));
               }

               return info;
          }

          /// <summary>
          /// Возвращает номер алгоритма по имени алгоритма
          /// </summary>
          /// <param name="provName">Имя провайдера</param>
          /// <param name="provType">Тип провайдера</param>
          /// <param name="alg">Имя алгоритма</param>
          /// <returns>Номер алгоритма</returns>
          public static uint GetCAlgByName(string provName, uint provType, string alg)
          {
               foreach (KeyValuePair<CAPI.ALG_ID, string> algInfo in GetAllProvAlgs(provName, provType))
               {
                    if (algInfo.Value == alg)
                    {
                         return (uint)algInfo.Key;
                    }
               }

               throw new IndexOutOfRangeException(string.Format("Hash alg \"{0}\" not found! in provider \"{1}\" !", alg, provName)); 
          }

          /// <summary>
          /// Возвращает информацию о криптопровайдере
          /// </summary>
          /// <param name="provName">Имя провайдера</param>
          /// <param name="provType">Тип провайдера</param>
          /// <returns>Информация о криптопровайдере</returns>
          internal static ProviderInfo GetProvInfo(string provName, uint provType)
          {
               ProviderInfo info = new ProviderInfo();

               info.Name = provName;
               info.Type = provType;
               info.HashList = new List<string>();
               info.SignList = new List<string>();
               info.ChipherList = new List<string>();
               info.ContainerList = new List<string>(GetContainerList(info.Name, info.Type));

               foreach (KeyValuePair<CAPI.ALG_ID, string> algInfo in GetAllProvAlgs(info.Name, info.Type))
               {
                    uint typeAlg = ((uint)algInfo.Key >> 13) << 13;
                    if (typeAlg == (uint)CAPI.AlgorithmClass.ALG_CLASS_HASH)
                    {
                         info.HashList.Add(algInfo.Value);
                    }

                    if (typeAlg == (uint)CAPI.AlgorithmClass.ALG_CLASS_SIGNATURE)
                    {
                         info.SignList.Add(algInfo.Value);
                    }

                    if (typeAlg == (uint)CAPI.AlgorithmClass.ALG_CLASS_DATA_ENCRYPT)
                    {
                         info.ChipherList.Add(algInfo.Value);
                    }
               }

               return info;
          }
     }
}
