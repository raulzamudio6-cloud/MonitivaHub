using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
//using System.Security.Cryptography.X509Certificates.X509Certificate2;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Реализация криптографичечких функций
    /// </summary>
    internal class CryptoFactory
     {
          /// <summary>
          /// Создание самоподписанного сертификата
          /// </summary>
          /// <param name="provName">Имя провайдера</param>
          /// <param name="provType">Тип провайдера</param>
          /// <param name="keyName">Имя ключа</param>
          /// <param name="subject">Издатель</param>
          /// <returns>Самоподписанный сертификат</returns>
          public static CertPtr CreateSelfSignedCert(string provName, uint provType, string keyName, string subject, DateTime start, DateTime end, ref byte[] hashCert, ref byte[] certBlob, string storeName, bool local = false)
          {
               CertPtr cert = new CertPtr();

               using (ManagedCertNameBlob certName = new ManagedCertNameBlob(subject))
               {
                    CAPI.CRYPT_KEY_PROV_INFO pInfo = new CAPI.CRYPT_KEY_PROV_INFO();
                    pInfo.pwszContainerName = keyName;
                    pInfo.pwszProvName = provName;
                    pInfo.dwProvType = provType;
                    pInfo.dwFlags = (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET;
                    pInfo.cProvParam = 0;
                    pInfo.rgProvParam = IntPtr.Zero;
                    pInfo.dwKeySpec = (uint)CAPI.KeySpecifier.AT_KEYEXCHANGE;

                    CAPI.SystemTime startTime = new CAPI.SystemTime();
                    
                    startTime.Year = (short)(start.Year);
                    startTime.Month = (short)start.Month;
                    startTime.Day = (short)(start.Day-1);
                    startTime.Hour = (short)(start.Hour);
                    startTime.Minute = (short)start.Minute;
                    startTime.Second = (short)start.Second;

                    CAPI.SystemTime endTime = new CAPI.SystemTime();
                    endTime.Year = (short)end.Year;
                    endTime.Month = (short)end.Month;
                    endTime.Day = (short)end.Day;
                    endTime.Hour = (short)end.Hour;
                    endTime.Minute = (short)end.Minute;
                    endTime.Second = (short)end.Second;

                    IntPtr first = Marshal.AllocHGlobal(Marshal.SizeOf(startTime)); ;
                    IntPtr last = Marshal.AllocHGlobal(Marshal.SizeOf(endTime)); ;
                    
                    Marshal.StructureToPtr(startTime, first, false);
                    Marshal.StructureToPtr(endTime, last, false);

                    CAPI.CRYPT_ALGORITHM_IDENTIFIER signatureAlgo = new CAPI.CRYPT_ALGORITHM_IDENTIFIER()
                    {
                        pszObjId = CAPI.OID_RSA_SHA256RSA 
                    };

                    cert.Cert = CAPI.CertCreateSelfSignCertificate(IntPtr.Zero, ref certName.CertName, 0, ref pInfo, ref signatureAlgo, first, last, IntPtr.Zero);

                    if (cert.Cert == IntPtr.Zero)
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    using (UnManagedArray certPrint = new UnManagedArray())
                    {
                         uint hashCertLen = 0;

                         if (!CAPI.CertGetCertificateContextProperty(cert.Cert, CAPI.CertPropID.CERT_HASH_PROP_ID, certPrint.buffer, ref hashCertLen))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         certPrint.Alloc((int)hashCertLen);

                         if (!CAPI.CertGetCertificateContextProperty(cert.Cert, CAPI.CertPropID.CERT_HASH_PROP_ID, certPrint.buffer, ref hashCertLen))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         hashCert = certPrint.CopyToArray();

                         CAPI.CERT_CONTEXT certXontext = new CAPI.CERT_CONTEXT();
                         certXontext = (CAPI.CERT_CONTEXT)Marshal.PtrToStructure(cert.Cert, typeof(CAPI.CERT_CONTEXT));
                         uint certSize = certXontext.cbCertEncoded;

                         certBlob = new byte[certSize];
                         Marshal.Copy(certXontext.pbCertEncoded, certBlob, 0, (int)certSize); 
                    }

                    //CAPI.PCCRYPTUI_VIEWCERTIFICATE_STRUCT vcstruct = new CAPI.PCCRYPTUI_VIEWCERTIFICATE_STRUCT();
                    //vcstruct.dwSize = (uint)Marshal.SizeOf(vcstruct);
                    //vcstruct.pCertContext = cert.Cert;
                    //vcstruct.szTitle = "";
                    //vcstruct.nStartPage = 0;

                    //bool propschanged = false;
                    //if (!CAPI.CryptUIDlgViewCertificate(ref vcstruct, ref propschanged))
                    //{
                    //     int error = Marshal.GetLastWin32Error();
                    //     throw new CapiException(error);
                    //}

                    if (!string.IsNullOrEmpty(storeName))
                    {
                         X509Certificate2 certTmp = new X509Certificate2(cert.Cert);
                         X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    StorePermission sp = new StorePermission(PermissionState.Unrestricted);
                         sp.Flags = StorePermissionFlags.OpenStore;
                         sp.Assert();
                         //store.Open(OpenFlags.MaxAllowed);

                         store.Open(OpenFlags.MaxAllowed | OpenFlags.MaxAllowed);
                         store.Add(certTmp);
                         store.Close();
                         //AddCertToStore(new ProvPtr(), cert, storeName, local);

                         //CertPtr cc = FindCertificateByHash(new ProvPtr(), "Root", hashCert);

                         //vcstruct.pCertContext = cc.Cert;

                         //if (!CAPI.CryptUIDlgViewCertificate(ref vcstruct, ref propschanged))
                         //{
                         //     int error = Marshal.GetLastWin32Error();
                         //     throw new CapiException(error);
                         //}
                    }
               }

               return cert;
          }

          public static void GetTimeCertValid(CertPtr cert, ref DateTime start, ref DateTime end)
          {

               X509Certificate2 myCert = new X509Certificate2(cert.Cert);
               start = myCert.NotBefore;
               end = myCert.NotAfter;
          }

          public static void AddCertToStore(ProvPtr prov, CertPtr cert, string name, bool local)
          {
               Int32 X509_ASN_ENCODING = 0x00000001;

               Int32 PKCS_7_ASN_ENCODING = 0x00010000;

               Int32 MY_ENCODING_TYPE = PKCS_7_ASN_ENCODING | X509_ASN_ENCODING;

               Int32 CERT_SYSTEM_STORE_CURRENT_USER_ID = 1;
               Int32 CERT_SYSTEM_STORE_LOCAL_MACHINE_ID = 2;

               Int32 CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;

               Int32 CERT_SYSTEM_STORE_CURRENT_USER = (local ? CERT_SYSTEM_STORE_CURRENT_USER_ID : CERT_SYSTEM_STORE_LOCAL_MACHINE_ID) << CERT_SYSTEM_STORE_LOCATION_SHIFT;

               using(CertStorePtr store = new CertStorePtr())
               {
                    StringBuilder storeName = new StringBuilder(name);

                    store.CertStore = CAPI.CertOpenStore((uint)CAPI.CertStoreType.CERT_STORE_PROV_SYSTEM, (uint)MY_ENCODING_TYPE, prov.Prov, (uint)CERT_SYSTEM_STORE_CURRENT_USER, name);

                    if (store.CertStore == IntPtr.Zero)
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    } 

                    IntPtr zeroPtr = IntPtr.Zero;

                    if(!CAPI.CertAddCertificateContextToStore(store.CertStore, cert.Cert, CAPI.CertStoreDisposition.CERT_STORE_ADD_ALWAYS, ref zeroPtr))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    } 
               }
          }

          public static IntPtr GetCertContext(ProvPtr prov, string name, byte[] hash)
          {
               Int32 X509_ASN_ENCODING = 0x00000001;

               Int32 PKCS_7_ASN_ENCODING = 0x00010000;

               Int32 MY_ENCODING_TYPE = PKCS_7_ASN_ENCODING | X509_ASN_ENCODING;

               Int32 CERT_SYSTEM_STORE_CURRENT_USER_ID = 1;

               Int32 CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;

               Int32 CERT_SYSTEM_STORE_CURRENT_USER = CERT_SYSTEM_STORE_CURRENT_USER_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT;

               Int32 CERT_COMPARE_SHA1_HASH = 1;

               Int32 CERT_COMPARE_SHIFT = 16;

               Int32 CERT_FIND_SHA1_HASH = (CERT_COMPARE_SHA1_HASH << CERT_COMPARE_SHIFT);

               using (CertStorePtr store = new CertStorePtr())
               {
                    StringBuilder storeName = new StringBuilder(name);

                    store.CertStore = CAPI.CertOpenStore((uint)CAPI.CertStoreType.CERT_STORE_PROV_SYSTEM, (uint)MY_ENCODING_TYPE, prov.Prov, (uint)CERT_SYSTEM_STORE_CURRENT_USER, name);

                    if (store.CertStore == IntPtr.Zero)
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    using (ManagedCapiBlob blobHash = new ManagedCapiBlob(hash))
                    {
                         IntPtr hashPtr = Marshal.AllocHGlobal(Marshal.SizeOf(blobHash.blob));
                         Marshal.StructureToPtr(blobHash.blob, hashPtr, false);
                         IntPtr cert = CAPI.CertFindCertificateInStore(store.CertStore, (uint)MY_ENCODING_TYPE, 0, (uint)CERT_FIND_SHA1_HASH, hashPtr, IntPtr.Zero);

                         return cert;
                    }
               }
          }
          public static IntPtr FindCertificateByHash(ProvPtr prov, string name, byte[] hash, ref byte[] certBlob)
          {
               
               IntPtr cert = GetCertContext(prov, name, hash);

               if (cert == IntPtr.Zero)
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               CAPI.CERT_CONTEXT certContext = new CAPI.CERT_CONTEXT();
               certContext = (CAPI.CERT_CONTEXT)Marshal.PtrToStructure(cert, typeof(CAPI.CERT_CONTEXT));
               
               uint certSize = certContext.cbCertEncoded;

               byte[] certBlobTmp = new byte[certSize];
               Marshal.Copy(certContext.pbCertEncoded, certBlobTmp, 0, (int)certSize);

               X509Certificate2 certX509 = new X509Certificate2(certBlobTmp);
               certBlob = certX509.Export(X509ContentType.Cert);
               return cert;
          }

          public void GetCertTimePeriod(byte[] certHash, ref DateTime start, ref DateTime end, string store)
          {
               using (ProvPtr prov = new ProvPtr())
               {
                    using (CertPtr certPtr = new CertPtr())
                    {
                         byte[] cert = null;
                         certPtr.Cert = CryptoFactory.FindCertificateByHash(prov, store, certHash, ref cert);
                         if (certPtr != null && certPtr.Cert != IntPtr.Zero)
                         {
                              CryptoFactory.GetTimeCertValid(certPtr, ref start, ref end);
                         }
                    }
               }
          }

          /// <summary>
          /// Создание сертификата
          /// </summary>
          /// <param name="provName">Имя провайдера</param>
          /// <param name="provType">Тип провайдера</param>
          /// <param name="keyName">Имя ключа</param>
          /// <param name="subject">Subject</param>
          /// <returns>Сертификат</returns>
          public static byte[] CreateCert(string provName, uint provType, string request, DateTime start, DateTime end, byte[] ca, ref byte[] certHash)
          {
               return null;
          }

          /// <summary>
          /// Создание запроса на сертификат
          /// </summary>
          /// <param name="prov">Указатель на провайдер</param>
          /// <param name="subject">Subject</param>
          /// <returns>Запрос на сертификат base64</returns>
          public static string GetCertRequest(ProvPtr prov, string subject)
          {
               using (ManagedCertNameBlob reqSubj = new ManagedCertNameBlob(subject))
               {
                    IntPtr pubKey = IntPtr.Zero;
                    uint pubKeyLen = 0;
                    if (!CAPI.CryptExportPublicKeyInfo(prov.Prov, (uint)CAPI.KeySpecifier.AT_KEYEXCHANGE, (uint)CAPI.EncodingType.X509_ASN_ENCODING, pubKey, ref pubKeyLen))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    pubKey = Marshal.AllocHGlobal((int)pubKeyLen);

                    if (!CAPI.CryptExportPublicKeyInfo(prov.Prov, (uint)CAPI.KeySpecifier.AT_KEYEXCHANGE, (uint)CAPI.EncodingType.X509_ASN_ENCODING, pubKey, ref pubKeyLen))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    var info = (CAPI.CERT_PUBLIC_KEY_INFO)Marshal.PtrToStructure(pubKey, typeof(CAPI.CERT_PUBLIC_KEY_INFO));

                    try
                    {
                         CAPI.CERT_REQUEST_INFO req = new CAPI.CERT_REQUEST_INFO();
                         req.dwVersion = (uint)CAPI.CertVersion.CERT_V1;
                         req.cAttribute = 0;
                         req.rgAttribute = IntPtr.Zero;
                         req.Subject = reqSubj.CertName;
                         req.SubjectPublicKeyInfo = info;

                         CAPI.CRYPT_ALGORITHM_IDENTIFIER alg = new CAPI.CRYPT_ALGORITHM_IDENTIFIER();
                         alg.Parameters.cbData = 0;
                         alg.Parameters.pbData = IntPtr.Zero;
                         alg.pszObjId = CAPI.OID_RSA_SHA256RSA; ///*"1.3.14.3.2.29""1.2.840.113549.1.1.5"*/"1.2.840.113549.1.1.5";

                         IntPtr algInfo = Marshal.AllocHGlobal(Marshal.SizeOf(alg));
                         
                         Marshal.StructureToPtr(alg, algInfo, false);

                         IntPtr reqInfo = Marshal.AllocHGlobal(Marshal.SizeOf(req));
                         for(int i = 0; i < Marshal.SizeOf(req); ++i)
                         {
                              Marshal.WriteByte(reqInfo, i, 0);
                         }
                         
                         Marshal.StructureToPtr(req, reqInfo, false);

                         uint reqEncodingLen = 0;

                         byte[] reqEncoding = null;

                         IntPtr codingReqCertType = (IntPtr)4;
                         if (!CAPI.CryptSignAndEncodeCertificate(prov.Prov, (uint)CAPI.KeySpecifier.AT_KEYEXCHANGE, (uint)CAPI.EncodingType.X509_ASN_ENCODING, codingReqCertType, reqInfo, algInfo, IntPtr.Zero, reqEncoding, ref reqEncodingLen))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         reqEncoding = new byte[reqEncodingLen];

                         if (!CAPI.CryptSignAndEncodeCertificate(prov.Prov, (uint)CAPI.KeySpecifier.AT_KEYEXCHANGE, (uint)CAPI.EncodingType.X509_ASN_ENCODING, codingReqCertType, reqInfo, algInfo, IntPtr.Zero, reqEncoding, ref reqEncodingLen))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         StringBuilder reqStr = null;
                         uint reqStrLen = 0;
                         if (!CAPI.CryptBinaryToString(reqEncoding, (uint)reqEncoding.Length, (uint)0x00000003, reqStr, ref reqStrLen))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         reqStr = new StringBuilder((int)reqStrLen);
                         if (!CAPI.CryptBinaryToString(reqEncoding, (uint)reqEncoding.Length, (uint)0x00000003, reqStr, ref reqStrLen))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         return reqStr.ToString();
                    }
                    finally
                    {
                         if (pubKey != IntPtr.Zero)
                         {
                              Marshal.FreeHGlobal(pubKey);
                         }
                    }
               }
          }

          /// <summary>
          /// Возвращает запрос на сертификат 
          /// </summary>
          /// <param name="provName">Имя провайдер</param>
          /// <param name="provType">Тип провайдер</param>
          /// <param name="keyName">Имя ключа</param>
          /// <param name="subject">Subject</param>
          /// <returns>Запрос на сертификат в формате base64</returns>
          

          public static string GetCertRequest(string provName, uint provType, string keyName, string subject, byte[] pin)
          {
              using (ProvPtr prov = new ProvPtr())
              {
                  if (!CAPI.CryptAcquireContext(ref prov.Prov, keyName, provName, provType, (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                  {
                      int error = Marshal.GetLastWin32Error();
                      throw new CapiException(error);
                  }

                  if (pin != null && !CAPI.CryptSetProvParam(prov.Prov, CAPI.ProviderParamType.PP_SIGNATURE_PIN, pin, 0))
                  {
                      int error = Marshal.GetLastWin32Error();
                      throw new CapiException(error);
                  }

                  return GetCertRequest(prov, subject);
              }
          }

          /// <summary>
          /// Удаление ключа
          /// </summary>
          /// <param name="provName">Имя провайдера</param>
          /// <param name="provType">Тип провайдера</param>
          /// <param name="keyName">Имя ключа</param>
          public static void DeleteContainer(string provName, uint provType, string keyName)
          {
               using (ProvPtr prov = new ProvPtr())
               {
                   if (!CAPI.CryptAcquireContext(ref prov.Prov, keyName, provName, provType, (uint)CAPI.ContextFlags.CRYPT_DELETEKEYSET | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                    {
                         int error = Marshal.GetLastWin32Error();
                         if (error != -2146435060)
                         {
                              throw new CapiException(error);
                         }
                    }
               }
          }

          public static void OpenContainer(ProvPtr prov, string provName, uint provType, string keyName, byte[] pwd)
          {
              if (!CAPI.CryptAcquireContext(ref prov.Prov, keyName, provName, provType, (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
              {
                  int error = Marshal.GetLastWin32Error();
                  throw new CapiException(error);
              }

              if (pwd != null && !CAPI.CryptSetProvParam(prov.Prov, CAPI.ProviderParamType.PP_SIGNATURE_PIN, pwd, 0))
              {
                  int error = Marshal.GetLastWin32Error();
                  throw new CapiException(error);
              }
          }

          public static void OpenContainer(string provName, uint provType, string keyName, byte[] pwd)
          {
               using (ProvPtr prov = new ProvPtr())
               {
                   OpenContainer(prov, provName, provType, keyName, pwd);
               }
          }

          /// <summary>
          /// Генерация ключа в контейнере ключей
          /// </summary>
          /// <param name="prov">Провайдер</param>
          /// <param name="alg">Алгоритм ключа</param>
          /// <param name="pin">Пароль</param>
          /// <param name="exportable">Возможность экспорта ключа</param>
          /// <param name="protection">Тип парольной защиты ключа </param>
          protected static void GenKey(ProvPtr prov, uint alg, byte[] pin, bool shortKey, bool exportable, ProviderSettings.PinProtection protection = ProviderSettings.PinProtection.NONE)
          {
               using (KeyPtr key = new KeyPtr())
               {
                    uint flag = 0;

                    if (exportable)
                    {
                         flag |= (uint)CAPI.GenFlags.CRYPT_EXPORTABLE;
                    }

                    if (/*protection == ProviderSettings.PinProtection.PIN &&*/ (pin != null && pin.Length != 0))
                    {
                         if (!CAPI.CryptSetProvParam(prov.Prov, CAPI.ProviderParamType.PP_SIGNATURE_PIN, pin, 0))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }
                    }

                    if (protection == ProviderSettings.PinProtection.WinPassword)
                    {
                         flag |= (uint)CAPI.GenFlags.CRYPT_FORCE_KEY_PROTECTION_HIGH;
                    }

                    if (!shortKey)
                    {
                        flag |= 0x10000000;
                    }

                    if (!CAPI.CryptGenKey(prov.Prov, alg, flag, ref key.Key))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }
               }
          }

          /// <summary>
          /// Генерация ключа подписи
          /// </summary>
          /// <param name="prov">Провайдер</param>
          /// <param name="pin">Пароль</param>
          protected static void GenSignKey(ProvPtr prov, byte[] pin, bool shortKey)
          {
               GenKey(prov, (uint)CAPI.KeySpecifier.AT_SIGNATURE, pin, shortKey, true);
          }

          /// <summary>
          /// Генерация ключа экспорта
          /// </summary>
          /// <param name="prov">Провайдер</param>
          /// <param name="pin">Пароль</param>
          protected static void GenExchKey(ProvPtr prov, byte[] pin, bool shortKey)
          {
               GenKey(prov, (uint)CAPI.KeySpecifier.AT_KEYEXCHANGE, pin, shortKey, true);
          }

          /// <summary>
          /// Создать ключ
          /// </summary>
          /// <param name="provName">Имя провайдера</param>
          /// <param name="provType">Тип провайдера</param>
          /// <param name="keyName">Имя ключа</param>
          /// <param name="pin">Пароль</param>
          public static void CreateNewContainer(string provName, uint provType, string keyName, byte[] pin, bool shortKey)
          {
               using (ProvPtr prov = new ProvPtr())
               {
                   CreateNewContainer(prov, provName, provType, keyName, pin, shortKey);
               }
          }

         public static void CreateNewContainer(ProvPtr prov, string provName, uint provType, string keyName, byte[] pin, bool shortKey)
         {
             if (!CAPI.CryptAcquireContext(ref prov.Prov, keyName, provName, provType, (uint)CAPI.ContextFlags.CRYPT_NEWKEYSET | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    GenExchKey(prov, pin, shortKey);
                    GenSignKey(prov, pin, shortKey);
         }

          /// <summary>
          /// Вычислить хеш от массива данных
          /// </summary>
          /// <param name="prov">Имя провайдера</param>
          /// <param name="alg">Алгоритм хеша</param>
          /// <param name="key">Ключ</param>
          /// <param name="intputValue">Входные данные</param>
          /// <returns>Хеш от данных</returns>
          public static byte[] CreateHashValue(ProvPtr prov, uint alg, KeyPtr key, byte[] intputValue)
          {
               using(HashPtr hash = new HashPtr())
               {
                    if (!CAPI.CryptCreateHash(prov.Prov, alg, key.Key, 0, ref hash.Hash))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    IntPtr inputPtr = Marshal.AllocHGlobal(intputValue.Length);
                    try
                    {
                         Marshal.Copy(intputValue, 0, inputPtr, intputValue.Length);
                         if (!CAPI.CryptHashData(hash.Hash, inputPtr, (uint)intputValue.Length, 0))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }
                    }
                    finally
                    {
                         Marshal.FreeHGlobal(inputPtr);
                    }

                    IntPtr valuePtr = IntPtr.Zero;
                    byte[] hashVal = null;
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

                        hashVal = new byte[hashSize];
                        Marshal.Copy(valuePtr, hashVal, 0, (int)hashSize);
                    }
                    finally
                    {
                         Marshal.FreeHGlobal(valuePtr);
                    }
                    return hashVal;
               }
          }

          /// <summary>
          /// Вычислить хеш от массива данных
          /// </summary>
          /// <param name="provName">Имя провайдера</param>
          /// <param name="provType">Тип повайдера</param>
          /// <param name="hashAlg">Алгоритм хеширования</param>
          /// <param name="intputValue">Входные данные</param>
          /// <returns>Хеш от данных</returns>
          public static byte[] CreateHashValue(string provName, uint provType, string hashAlg, byte[] intputValue)
          {
               using (ProvPtr prov = new ProvPtr())
               {
                    KeyPtr key = new KeyPtr();

                    uint algId = CryptoSystemProperty.GetCAlgByName(provName, provType, hashAlg);

                    if (!CAPI.CryptAcquireContext(ref prov.Prov, IntPtr.Zero, provName, provType, (uint)CAPI.ContextFlags.CRYPT_VERIFYCONTEXT | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    return CreateHashValue(prov, algId, key, intputValue);
               }
          }

          /// <summary>
          /// Конвертирует массив байт в hex строку
          /// </summary>
          /// <param name="bytes">конвертируемые данные</param>
          /// <returns>Строковое представление</returns>
          public static string ByteToHexString(byte[] bytes)
          {
               char[] c = new char[bytes.Length * 2];
               int b;
               for (int i = 0; i < bytes.Length; i++)
               {
                    b = bytes[i] >> 4;
                    c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                    b = bytes[i] & 0xF;
                    c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
               }
               return new string(c);
          }

          public static byte[] GenCmacKey(string provName, uint provType)
          {
               byte[] keyBlob = null;
               
               using (ProvPtr prov = new ProvPtr())
               {
                    using (KeyPtr key = new KeyPtr())
                    {
                        if (!CAPI.CryptAcquireContext(ref prov.Prov, IntPtr.Zero, provName, provType, (uint)CAPI.ContextFlags.CRYPT_VERIFYCONTEXT | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         if (!CAPI.CryptGenKey(prov.Prov, (uint)CAPI.ALG_ID.CALG_AES_256, 1, ref key.Key))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         int blobLen = 44;

                         if (!CAPI.CryptExportKey(key.Key, IntPtr.Zero, 8, 0, IntPtr.Zero, ref blobLen))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         keyBlob = new byte[blobLen];
                         

                         if (!CAPI.CryptExportKey(key.Key, IntPtr.Zero, 8, 0, keyBlob, ref blobLen))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }
                    }
               }
               
               return keyBlob;
          }

          public static byte[] GetPublicKey(string provName, uint provType, string keyName, byte[] password)
          {
              byte[] keyBlob = null;

              using (ProvPtr prov = new ProvPtr())
              {
                  using (KeyPtr key = new KeyPtr())
                  {
                      if (!CAPI.CryptAcquireContext(ref prov.Prov, keyName, provName, provType, (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                      {
                          int error = Marshal.GetLastWin32Error();
                          throw new CapiException(error);
                      }

                      if (!CAPI.CryptSetProvParam(prov.Prov, CAPI.ProviderParamType.PP_SIGNATURE_PIN, password, 0))
                      {
                          int error = Marshal.GetLastWin32Error();
                          throw new CapiException(error);
                      }

                      if (!CAPI.CryptGetUserKey(prov.Prov, CAPI.KeySpecifier.AT_SIGNATURE, ref key.Key))
                      {
                          int error = Marshal.GetLastWin32Error();
                          throw new CapiException(error);
                      }

                      int blobLen = 0;

                      if (!CAPI.CryptExportKey(key.Key, IntPtr.Zero, 6, 0, null, ref blobLen))
                      {
                          int error = Marshal.GetLastWin32Error();
                          throw new CapiException(error);
                      }

                      keyBlob = new byte[blobLen];

                      if (!CAPI.CryptExportKey(key.Key, IntPtr.Zero, 6, 0, keyBlob, ref blobLen))
                      {
                          int error = Marshal.GetLastWin32Error();
                          throw new CapiException(error);
                      }
                  }
              }

              return keyBlob;
          }

          public static void AddCertToKey(string provName, uint provType, string keyName, byte[] pwd, byte[] cert)
          {
               using (ProvPtr prov = new ProvPtr())
               {
                    using (KeyPtr key = new KeyPtr())
                    {
                        if (!CAPI.CryptAcquireContext(ref prov.Prov, keyName, provName, provType, (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         if (pwd != null && !CAPI.CryptSetProvParam(prov.Prov, CAPI.ProviderParamType.PP_SIGNATURE_PIN, pwd, 0))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }
                         if (!CAPI.CryptGetUserKey(prov.Prov, CAPI.KeySpecifier.AT_KEYEXCHANGE, ref key.Key))
                         {
                             int error = Marshal.GetLastWin32Error();
                             throw new CapiException(error);
                         }
                        

                         if (!CAPI.CryptSetKeyParam(key.Key, (uint)CAPI.KeyParameter.KP_CERTIFICATE, cert, 0))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                   
                    }
               }
          }

          public static byte[] GenData(string provName, uint provType, uint dataLen)
          {
               byte[] data = new byte[dataLen];
               using (ProvPtr prov = new ProvPtr())
               {
                   if (!CAPI.CryptAcquireContext(ref prov.Prov, IntPtr.Zero, provName, provType, (uint)CAPI.ContextFlags.CRYPT_VERIFYCONTEXT | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    if (!CAPI.CryptGenRandom(prov.Prov, dataLen, data))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }
               }
               return data;
          }

          public static byte[] SignData(string provName, uint provType, string keyName, uint hashAlg, byte[] password, byte[] msg)
          {
               byte[] sign = null;
               
               using (ProvPtr prov = new ProvPtr())
               {
                   if (!CAPI.CryptAcquireContext(ref prov.Prov, keyName, provName, provType, (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                   
                    if (password != null && !CAPI.CryptSetProvParam(prov.Prov, CAPI.ProviderParamType.PP_SIGNATURE_PIN, password, 0))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    using (KeyPtr key = new KeyPtr())
                    { 
                         if (!CAPI.CryptGetUserKey(prov.Prov, CAPI.KeySpecifier.AT_SIGNATURE, ref key.Key))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         using( HashPtr hash = new HashPtr())
                         {
                              if (!CAPI.CryptCreateHash(prov.Prov, hashAlg, IntPtr.Zero, 0, ref hash.Hash))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   throw new CapiException(error);
                              }
                              
                              IntPtr inputPtr = Marshal.AllocHGlobal(msg.Length);
                              try
                              {
                                   Marshal.Copy(msg, 0, inputPtr, msg.Length);
                                   if (!CAPI.CryptHashData(hash.Hash, inputPtr, (uint)msg.Length, 0))
                                   {
                                        int error = Marshal.GetLastWin32Error();
                                        throw new CapiException(error);
                                   }
                              }
                              finally
                              {
                                   Marshal.FreeHGlobal(inputPtr);
                              }
                              
                              uint signLen = 0;
                              if(!CAPI.CryptSignHash(hash.Hash, (uint)CAPI.KeySpecifier.AT_SIGNATURE, IntPtr.Zero, 0, sign, ref signLen))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   throw new CapiException(error);
                              }

                              sign = new byte[signLen];
                              if (!CAPI.CryptSignHash(hash.Hash, (uint)CAPI.KeySpecifier.AT_SIGNATURE, IntPtr.Zero, 0, sign, ref signLen))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   throw new CapiException(error);
                              }
                         }
                    }
               }
               return sign;
          }

          public static bool VerifySignData(string provName, uint provType, uint hashAlg, byte[] publicKey, byte[] msg, byte[] sign)
          {
               using (ProvPtr prov = new ProvPtr())
               {
                   if (!CAPI.CryptAcquireContext(ref prov.Prov, IntPtr.Zero, provName, provType, (uint)CAPI.ContextFlags.CRYPT_VERIFYCONTEXT | (uint)CAPI.ContextFlags.CRYPT_MACHINE_KEYSET))
                    {
                         int error = Marshal.GetLastWin32Error();
                         throw new CapiException(error);
                    }

                    

                    using (KeyPtr key = new KeyPtr())
                    {
                         if (!CAPI.CryptImportKey(prov.Prov, publicKey, (uint)publicKey.Length, IntPtr.Zero, 0, ref key.Key))
                         {
                              int error = Marshal.GetLastWin32Error();
                              throw new CapiException(error);
                         }

                         using (HashPtr hash = new HashPtr())
                         {
                              if (!CAPI.CryptCreateHash(prov.Prov, hashAlg, IntPtr.Zero, 0, ref hash.Hash))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   throw new CapiException(error);
                              }

                              IntPtr inputPtr = Marshal.AllocHGlobal(msg.Length);
                              try
                              {
                                   Marshal.Copy(msg, 0, inputPtr, msg.Length);
                                   if (!CAPI.CryptHashData(hash.Hash, inputPtr, (uint)msg.Length, 0))
                                   {
                                        int error = Marshal.GetLastWin32Error();
                                        throw new CapiException(error);
                                   }
                              }
                              finally
                              {
                                   Marshal.FreeHGlobal(inputPtr);
                              }

                             
                              if (!CAPI.CryptVerifySignature(hash.Hash, sign, (uint)sign.Length, key.Key, IntPtr.Zero, 0))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   if (error == -2146893818)
                                   {
                                        return false;
                                   }
                                   throw new CapiException(error);
                              }

                              return true;
                         }
                    }
               }
          }
     }
}
