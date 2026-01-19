using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Создает сертификаты пользователей
    /// </summary>
    public class CertFactory
     {
          private ProviderSettings Settings = null;
          public CertFactory(ProviderSettings settings)
          {
               Settings = settings;
          }

          void DecodeCSR(string req, ref CAPI.CERT_REQUEST_INFO decodeReq)
          {
               uint decReqLen = 0;
               byte[] decReq = null;
               if (!CAPI.CryptStringToBinary(req, (uint)req.Length, (uint)0x00000003 /*CRYPT_STRING_BASE64REQUESTHEADER*/, decReq, ref decReqLen, 0, 0))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               decReq = new byte[decReqLen];

               if (!CAPI.CryptStringToBinary(req, (uint)req.Length, (uint)0x00000003 /*CRYPT_STRING_BASE64REQUESTHEADER*/, decReq, ref decReqLen, 0, 0))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               uint csrLen = 0;
               IntPtr csr = IntPtr.Zero;
               IntPtr codingReqCertType = (IntPtr)4;

               if (!CAPI.CryptDecodeObject(1/*X509_ASN_ENCODING*/, codingReqCertType/*X509_CERT_REQUEST_TO_BE_SIGNED*/, decReq, decReqLen, 0, csr, ref csrLen))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               csr = Marshal.AllocHGlobal((int)csrLen);

               if (!CAPI.CryptDecodeObject(1/*X509_ASN_ENCODING*/, codingReqCertType/*X509_CERT_REQUEST_TO_BE_SIGNED*/, decReq, decReqLen, 0, csr, ref csrLen))
               {
                    int error = Marshal.GetLastWin32Error();
                    throw new CapiException(error);
               }

               decodeReq = (CAPI.CERT_REQUEST_INFO)Marshal.PtrToStructure(csr, typeof(CAPI.CERT_REQUEST_INFO));    

               
          }

          public FunctionResult CreateCertReq(Int32 userID, string keyName, string subject, byte[] pin, ref string request)
          {
               FunctionResult result = new FunctionResult();
               result.AddToLog(String.Format("Generated Certificate Request for user {0}.", userID));
               request = CryptoFactory.GetCertRequest(Settings.Name, Settings.Type, keyName, subject, pin);
               result.AddToLog(String.Format("Generated Certificate Request for user {0} success!: {1}", userID, request));
               result.ResultOperation = true;
               return result;
          }

          public FunctionResult SetKeyInfo(byte[] caCertHash, CAPI.CRYPT_KEY_PROV_INFO prop)
          {
              FunctionResult result = new FunctionResult();
              result.AddToLog(String.Format("Set CaCertKeyInfo"));
              try 
              {
                  using (ProvPtr caProv = new ProvPtr())
                  {
                      using (CertPtr certCaPtr = new CertPtr())
                      {
                          using (ProvPtr prov = new ProvPtr())
                          {

                              certCaPtr.Cert = CryptoFactory.GetCertContext(prov, "ROOT", caCertHash);

                              if (certCaPtr.Cert == IntPtr.Zero)
                              {
                                  int error = Marshal.GetLastWin32Error();
                                  //if (error != 0)
                                  {
                                      throw new CapiException(error);
                                  }
                              }

                              IntPtr keyProvInfo = Marshal.AllocHGlobal(Marshal.SizeOf(prop));

                              Marshal.StructureToPtr(prop, keyProvInfo, false);

                              if (!CAPI.CertSetCertificateContextProperty(certCaPtr.Cert, CAPI.CertPropID.CERT_KEY_PROV_INFO_PROP_ID, 0, keyProvInfo))
                              {
                                  int error = Marshal.GetLastWin32Error();
                                  throw new CapiException(error);
                              }

                              result.ResultOperation = true;
                              result.AddToLog("Success.");
                          }
                      }
                  }
              }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("Set key info fo cert failed! Error: {1}, code: 0x{0:x}, source: {2}, stack trace: {3} ", ex.Error, new Win32Exception(ex.Error).Message, ex.Source, ex.StackTrace) );
               }
               catch (SystemException ex)
               {
                   result.AddToLog(String.Format("Set key info fo cert failed! Error: {0}, Source: {1}", ex.Message, ex.StackTrace));
               }

               return result;
          }

          public FunctionResult GetKeyInfo(byte[] caCertHash, ref CAPI.CRYPT_KEY_PROV_INFO propRes)
          {
              FunctionResult result = new FunctionResult();
              result.AddToLog(String.Format("Set CaCertKeyInfo"));
              try
              {
                  using (ProvPtr caProv = new ProvPtr())
                  {
                      using (CertPtr certCaPtr = new CertPtr())
                      {
                          using (ProvPtr prov = new ProvPtr())
                          {

                              certCaPtr.Cert = CryptoFactory.GetCertContext(prov, "ROOT", caCertHash);

                              if (certCaPtr.Cert == IntPtr.Zero)
                              {
                                  int error = Marshal.GetLastWin32Error();
                                  //if (error != 0)
                                  {
                                      throw new CapiException(error);
                                  }
                              }

                              IntPtr keyProvInfo = IntPtr.Zero;
                              uint keyProvInfoSize = 0;
                              if (!CAPI.CertGetCertificateContextProperty(certCaPtr.Cert, CAPI.CertPropID.CERT_KEY_PROV_INFO_PROP_ID, keyProvInfo, ref keyProvInfoSize))
                              {
                                  int error = Marshal.GetLastWin32Error();
                                  if (error != 0)
                                  {
                                      throw new SystemException("AAA");
                                      throw new CapiException(error);
                                  }

                              }

                              keyProvInfo = Marshal.AllocHGlobal((int)keyProvInfoSize);

                              if (keyProvInfoSize == 0)
                              {
                                  throw new SystemException("keyProvInfoSize is null");
                              }

                              if (!CAPI.CertGetCertificateContextProperty(certCaPtr.Cert, CAPI.CertPropID.CERT_KEY_PROV_INFO_PROP_ID, keyProvInfo, ref keyProvInfoSize))
                              {
                                  int error = Marshal.GetLastWin32Error();
                                  if (error != 0)
                                  {
                                      throw new CapiException(error);
                                  }
                              }

                              CAPI.CRYPT_KEY_PROV_INFO prop = new CAPI.CRYPT_KEY_PROV_INFO();
                              prop = (CAPI.CRYPT_KEY_PROV_INFO)Marshal.PtrToStructure(keyProvInfo, typeof(CAPI.CRYPT_KEY_PROV_INFO));

                              propRes = prop;
                              result.ResultOperation = true;
                              result.AddToLog("Success.");

                          }
                      }
                  }
              }
              catch (CapiException ex)
              {
                  result.AddToLog(String.Format("Set key info fo cert failed! Error: {1}, code: 0x{0:x}, source: {2}, stack trace: {3} ", ex.Error, new Win32Exception(ex.Error).Message, ex.Source, ex.StackTrace));
              }
              catch (SystemException ex)
              {
                  result.AddToLog(String.Format("Set key info fo cert failed! Error: {0}, Source: {1}", ex.Message, ex.StackTrace));
              }

              return result;
          }          /// <summary>
          /// Создает сертификат пользователя на основе запроса
          /// </summary>
          /// <param name="userID">ID пользователя</param>
          /// <param name="certRequest">Запрос сертификата</param>
          /// <param name="cert">Сертификат</param>
          /// <param name="certHash">Hash сертификата</param>
          /// <returns>Результат выполнения операции</returns>
          public FunctionResult CreateCert(Int32 userID, string certRequest, DateTime start, DateTime end, ref byte[] cert, ref byte[] certHash , byte[] caCertHash)
          {
               FunctionResult result = new FunctionResult();

               try
               {
                    if (string.IsNullOrEmpty(certRequest))
                    {
                         throw new ArgumentException("CSR is empty string or null");
                    }

                    if(caCertHash == null)
                    {
                         throw new ArgumentException("Hash of ca serificate is null");
                    }
                    using (ProvPtr caProv = new ProvPtr())
                    {
                         using (CertPtr certCaPtr = new CertPtr())
                         {

                              result.AddToLog(String.Format("Generated Certificate for user {0}", userID));

                              //string request = CryptoFactory.GetCertRequest(Settings.Name, Settings.Type, Settings.KeyName, "CN=AAA");

                              CAPI.CERT_REQUEST_INFO decReq = new CAPI.CERT_REQUEST_INFO();
                              DecodeCSR(certRequest/*request*/, ref decReq);

                              using (ProvPtr prov = new ProvPtr())
                              {

                                   certCaPtr.Cert = CryptoFactory.GetCertContext(prov, "ROOT", caCertHash);

                                   if (certCaPtr.Cert == IntPtr.Zero)
                                   {
                                        int error = Marshal.GetLastWin32Error();
                                        //if (error != 0)
                                        {
                                            throw new CapiException(error);
                                        }
                                   }

                                   IntPtr keyProvInfo = IntPtr.Zero;
                                   uint keyProvInfoSize = 0;
                                   if (!CAPI.CertGetCertificateContextProperty(certCaPtr.Cert, CAPI.CertPropID.CERT_KEY_PROV_INFO_PROP_ID, keyProvInfo, ref keyProvInfoSize))
                                   {
                                        int error = Marshal.GetLastWin32Error();
                                        if (error != 0)
                                        {
                                            throw new CapiException(error);
                                        } 
                                       
                                   }

                                   keyProvInfo = Marshal.AllocHGlobal((int)keyProvInfoSize);

                                   if (keyProvInfoSize == 0)
                                   {
                                       throw new SystemException("keyProvInfoSize is null");
                                   }

                                   if (!CAPI.CertGetCertificateContextProperty(certCaPtr.Cert, CAPI.CertPropID.CERT_KEY_PROV_INFO_PROP_ID, keyProvInfo, ref keyProvInfoSize))
                                   {
                                        int error = Marshal.GetLastWin32Error();
                                        if (error != 0)
                                        {
                                            throw new CapiException(error);
                                        }
                                   }

                                   CAPI.CRYPT_KEY_PROV_INFO prop = new CAPI.CRYPT_KEY_PROV_INFO();
                                   prop = (CAPI.CRYPT_KEY_PROV_INFO)Marshal.PtrToStructure(keyProvInfo, typeof(CAPI.CRYPT_KEY_PROV_INFO));


                                   if (!CAPI.CryptAcquireContext(ref caProv.Prov, prop.pwszContainerName, prop.pwszProvName, prop.dwProvType, prop.dwFlags/*CAPI.ContextFlags.CRYPT_MACHINE_KEYSET*/))
                                   {
                                        int error = Marshal.GetLastWin32Error();
                                        if (error != 0)
                                        {
                                            throw new CapiException(error);
                                        }
                                   }



                              }

                              string[] oids = { /*szOID_PKIX_KP_CLIENT_AUTH*/ "1.3.6.1.5.5.7.3.2", /*szOID_PKIX_KP_EMAIL_PROTECTION*/  "1.3.6.1.5.5.7.3.4" };
                              CAPI.CRYPTOAPI_BLOB zeroBlob = new CAPI.CRYPTOAPI_BLOB();
                              zeroBlob.cbData = 0;
                              zeroBlob.pbData = IntPtr.Zero;
                              CAPI.CERT_EXTENSION ext = new CAPI.CERT_EXTENSION();
                              ext.fCritical = true;
                              ext.pszObjId = /*szOID_ENHANCED_KEY_USAGE*/ "2.5.29.37";
                              ext.Value = zeroBlob;
                              CAPI.CERT_EXTENSION[] pExtensions = { ext };

                              CAPI.CERT_ENHKEY_USAGE usage = new CAPI.CERT_ENHKEY_USAGE();
                              usage.cUsageIdentifier = 2;

                              IntPtr OID = Marshal.StringToHGlobalAnsi(oids[0]);
                              IntPtr OID2 = Marshal.StringToHGlobalAnsi(oids[1]);
                              usage.rgpszUsageIdentifier = Marshal.AllocHGlobal(Marshal.SizeOf(OID) + Marshal.SizeOf(OID2));

                              Marshal.WriteIntPtr(usage.rgpszUsageIdentifier, 0, OID);
                              Marshal.WriteIntPtr(usage.rgpszUsageIdentifier, Marshal.SizeOf(OID), OID2);

                              uint usageDecodeSize = 0;
                              IntPtr usageDecode = IntPtr.Zero;

                              Int32 X509_ASN_ENCODING = 0x00000001;

                              Int32 PKCS_7_ASN_ENCODING = 0x00010000;

                              Int32 MY_ENCODING_TYPE = PKCS_7_ASN_ENCODING | X509_ASN_ENCODING;

                              Int32 X509_ENHANCED_KEY_USAGE = 36;

                              if (!CAPI.CryptEncodeObject(MY_ENCODING_TYPE, X509_ENHANCED_KEY_USAGE, ref usage, IntPtr.Zero, ref usageDecodeSize))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   if (error != 0)
                                   {
                                       throw new CapiException(error);
                                   }
                              }

                              IntPtr buff = Marshal.AllocHGlobal((int)usageDecodeSize);

                              if (!CAPI.CryptEncodeObject(MY_ENCODING_TYPE, X509_ENHANCED_KEY_USAGE, ref usage, buff, ref usageDecodeSize))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   if (error != 0)
                                   {
                                       throw new CapiException(error);
                                   }
                              }

                              pExtensions[0].Value.cbData = usageDecodeSize;
                              pExtensions[0].Value.pbData = buff;

                              CAPI.CERT_INFO certInfo = new CAPI.CERT_INFO();
                              certInfo.dwVersion = 2;

                              long time = DateTime.Now.Ticks;
                              byte[] serial = BitConverter.GetBytes(time);
                               
                              certInfo.SerialNumber.cbData = (uint)serial.Length;
                              certInfo.SerialNumber.pbData = Marshal.AllocHGlobal(serial.Length);
                              Marshal.Copy(serial, 0, certInfo.SerialNumber.pbData, serial.Length);


                              certInfo.SignatureAlgorithm.Parameters.cbData = 0;
                              certInfo.SignatureAlgorithm.Parameters.pbData = IntPtr.Zero;
                              certInfo.SignatureAlgorithm.pszObjId = /*CAPI.OID_RSA_SHA256RSA;*/ "1.2.840.113549.1.1.5";

                              CAPI.CRYPT_ALGORITHM_IDENTIFIER alg = new CAPI.CRYPT_ALGORITHM_IDENTIFIER();
                              alg.Parameters.cbData = 0;
                              alg.Parameters.pbData = IntPtr.Zero;
                              alg.pszObjId = /*CAPI.OID_RSA_SHA256RSA;*/ "1.2.840.113549.1.1.5";
                              IntPtr algInfo = Marshal.AllocHGlobal(Marshal.SizeOf(alg));
                              Marshal.StructureToPtr(alg, algInfo, false);

                              CAPI.CERT_CONTEXT certXontext = new CAPI.CERT_CONTEXT();
                              certXontext = (CAPI.CERT_CONTEXT)Marshal.PtrToStructure(certCaPtr.Cert, typeof(CAPI.CERT_CONTEXT));

                              CAPI.CERT_INFO certCaInfo = new CAPI.CERT_INFO();
                              certCaInfo = (CAPI.CERT_INFO)Marshal.PtrToStructure(certXontext.pCertInfo, typeof(CAPI.CERT_INFO));

                              certInfo.Issuer = certCaInfo.Issuer;

                              CAPI.SystemTime startTime = new CAPI.SystemTime();


                              startTime.Year = (short)start.Year;
                              startTime.Month = (short)start.Month;
                              startTime.Day = (short)start.Day;
                              startTime.Hour = (short)start.Hour;
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

                              long hFT1 = end.ToFileTimeUtc();
                              long hFT2 = start.ToFileTimeUtc();
                              certInfo.NotAfter.dwLowDateTime = (int)(hFT1 & 0xFFFFFFFF);
                              certInfo.NotAfter.dwHighDateTime = (int)(hFT1 >> 32);
                              certInfo.NotBefore.dwLowDateTime = (int)(hFT2 & 0xFFFFFFFF);
                              certInfo.NotBefore.dwHighDateTime = (int)(hFT2 >> 32);
                              //certInfo.NotAfter = start.ToFileTime();
                              //certInfo.NotBefore = last;

                              certInfo.Subject = decReq.Subject;

                              certInfo.SubjectPublicKeyInfo = decReq.SubjectPublicKeyInfo;
                              certInfo.cExtension = 0/*1*/;
                              certInfo.rgExtension = IntPtr.Zero;

                              IntPtr reqInfo = Marshal.AllocHGlobal(Marshal.SizeOf(certInfo));
                              for (int i = 0; i < Marshal.SizeOf(certInfo); ++i)
                              {
                                   Marshal.WriteByte(reqInfo, i, 0);
                              }

                              Marshal.StructureToPtr(certInfo, reqInfo, false);

                              byte[] reqEncoding = null;
                              uint reqEncodingLen = 0;
                              IntPtr codingReqCertType = (IntPtr)2;
                              if (!CAPI.CryptSignAndEncodeCertificate(caProv.Prov, (uint)CAPI.KeySpecifier.AT_KEYEXCHANGE, (uint)CAPI.EncodingType.X509_ASN_ENCODING, codingReqCertType, reqInfo, algInfo, IntPtr.Zero, reqEncoding, ref reqEncodingLen))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   if (error != 0)
                                   {
                                       throw new CapiException(error);
                                   }
                              }

                              reqEncoding = new byte[reqEncodingLen];

                              if (!CAPI.CryptSignAndEncodeCertificate(caProv.Prov, (uint)CAPI.KeySpecifier.AT_KEYEXCHANGE, (uint)CAPI.EncodingType.X509_ASN_ENCODING, codingReqCertType, reqInfo, algInfo, IntPtr.Zero, reqEncoding, ref reqEncodingLen))
                              {
                                   int error = Marshal.GetLastWin32Error();
                                   if (error != 0)
                                   {
                                       throw new CapiException(error);
                                   }
                              }

                              X509Certificate2 cert2 = new X509Certificate2(reqEncoding);
                              certHash = cert2.GetCertHash();

                              X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                              store.Open(OpenFlags.ReadWrite);
                              store.Add(cert2);
                              store.Close();
                              cert2.Reset();

                              cert = reqEncoding;
                              
                              result.AddToLog(String.Format("Generated Certificate for user {0} success! Certificate hash: {1}", userID, CryptoFactory.ByteToHexString(certHash)));
                              result.ResultOperation = true;
                         }
                    }
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("Generated Generated Certificate user {0} failed! Error: {2}, code: 0x{1:x}, source: {3}, stack trace: {4} ", userID, ex.Error, new Win32Exception(ex.Error).Message, ex.Source, ex.StackTrace) );
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("Generated Certificate for user {0} failed! Error: {1}, Source: {2}", userID, ex.Message, ex.StackTrace));
               }

               return result;
          }

          public FunctionResult GetCert(Int32 userID, byte[] certHash, ref byte[] cert)
          {
               FunctionResult result = new FunctionResult();

               try
               {
                    result.AddToLog(String.Format("Get Certificate for user {0}", userID));

                    if (certHash == null)
                    {
                         throw new ArgumentException("Hash CA certificate is NULL");

                    }

                    string hex = BitConverter.ToString(certHash).Replace("-", string.Empty);

                    X509Store store = null;
                    try
                    {
                         store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                         store.Open(OpenFlags.ReadOnly);

                         X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByThumbprint, hex, false);

                         if (certificates.Count == 0)
                         {
                             throw new Exception(string.Format("Certificate not found. FindByThumbprint: {0}", hex));
                         }

                         X509Certificate certificatex = certificates[0];

                         cert = certificatex.Export(X509ContentType.Cert);
                    }
                    catch (SystemException)
                    {
                         throw;
                    }
                    finally
                    {
                         if (store != null)
                         {
                              store.Close();
                         }
                    }

                    result.AddToLog(String.Format("Get Certificate for user {0} success! Certificate hash: {1}", userID, CryptoFactory.ByteToHexString(certHash)));
                    result.ResultOperation = true;
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("Get Certificate user {0} failed! Error: {2}, code: 0x{1:x}", userID, ex.Error, new Win32Exception(ex.Error).Message));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("Get Certificate for user {0} failed! Error: {1}", userID, ex.Message));
               }

               return result;
          }

          public FunctionResult GetRootCert(Int32 userID, byte[] certHash, ref byte[] cert)
          {
               FunctionResult result = new FunctionResult();

               try
               {
                    result.AddToLog(String.Format("Get Root Certificate for user {0}", userID));

                    if (certHash == null)
                    {
                         throw new ArgumentException("Hash CA certificate is NULL");
 
                    }

                    string hex = BitConverter.ToString(certHash).Replace("-", string.Empty);

                    X509Store store = null;
                    try
                    {
                         store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                         store.Open(OpenFlags.ReadOnly);

                         X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByThumbprint, hex, false);

                         if (certificates.Count == 0)
                         {
                             throw new Exception(string.Format("Certificate not found. FindByThumbprint: {0}", hex));
                         }

                         X509Certificate certificatex = certificates[0];

                         cert = certificatex.Export(X509ContentType.Cert);
                    }
                    finally
                    {
                         if (store != null)
                         {
                              store.Close();
                         }
                    }

                    result.AddToLog(String.Format("Get Root Certificate for user {0} success! Certificate hash: {1}", userID, CryptoFactory.ByteToHexString(certHash)));
                    result.ResultOperation = true;
               }
               catch (CapiException ex)
               {
                    result.AddToLog(String.Format("Get Root Certificate for user {0} failed! Error: {2}, code: 0x{1:x}, source: {3}, stack trace: {4} ", userID, ex.Error, new Win32Exception(ex.Error).Message, ex.Source, ex.StackTrace));
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("Get Root Certificate for user {0} failed! Error: {1}", userID, ex.Message));
               }

               return result;
          }

          public FunctionResult GetCertTimePeriod(Int32 userID, byte[] certHash, ref DateTime start, ref DateTime end, string store = "MY")
          {
               FunctionResult result = new FunctionResult();

               try
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

                    result.AddToLog(String.Format("Get date using certificate for user {0} success! Certificate hash: {1}, Not Before: {2}, Not After {3}", userID, CryptoFactory.ByteToHexString(certHash), start, end));
                    result.ResultOperation = true;
               }
               catch (CapiException ex)
               {
                   result.AddToLog(String.Format("Get date using certificate user {0} failed! Error: {2}, code: 0x{1:x}, src: {3}, trace: {4}", userID, ex.Error, new Win32Exception(ex.Error).Message, ex.Source, ex.StackTrace));
               }
               catch (SystemException ex)
               {
                   result.AddToLog(String.Format("Get date using certificate for user {0} failed! Error: {1}, , src: {2}, trace: {3}", userID, ex.Message, ex.Source, ex.StackTrace));
               }

               return result;
          }
     }
}
