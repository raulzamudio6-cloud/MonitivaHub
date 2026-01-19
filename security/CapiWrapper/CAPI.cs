using System;
using System.Text;
using System.Runtime.InteropServices;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
     /// <summary>
     /// Описатель Native Crypto API функций
     /// </summary>
     public static class CAPI
     {
          #region general function
          
          [DllImport("kernel32.dll")]
          public static extern uint GetLastError();

          #endregion //general function

          #region base csp function

          //[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          //public static extern Boolean CryptAcquireContext(
          //    ref IntPtr hProv,
          //    string pszContainer,
          //    string pszProvider,
          //    UInt32 dwProvType,
          //    ContextFlags dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptAcquireContext(
              ref IntPtr hProv,
              string pszContainer,
              string pszProvider,
              UInt32 dwProvType,
              uint dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptAcquireContext(
              ref IntPtr hProv,
              IntPtr pszContainer,
              string pszProvider,
              UInt32 dwProvType,
              uint dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern Boolean CryptEnumProviders(
              UInt32 dwIndex,
              IntPtr pdwReserved,
              UInt32 dwFlags,
              ref UInt32 pdwProvType,
              StringBuilder pszProvName,
              ref UInt32 pcbProvName);

          [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern Boolean CryptEnumProviderTypes(
              UInt32 dwIndex,
              IntPtr pdwReserved,
              UInt32 dwFlags,
              ref UInt32 pdwProvType,
              StringBuilder pszTypeName,
              ref UInt32 pcbTypeName);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptGenRandom(
            IntPtr hProv,
            UInt32 dwLen,
            byte[] pbBuffer
          );

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptGetUserKey(
              IntPtr hProv,
              KeySpecifier dwKeySpec,
              ref IntPtr hKey);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptGetKeyParam(
              IntPtr hKey,
              KeyParameter dwParam,
              byte[] pbData,
              ref UInt32 pdwDataLen,
              UInt32 dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptGetKeyParam(
              IntPtr hKey,
              KeyParameter dwParam,
              ref UInt32 pbData,
              ref UInt32 pdwDataLen,
              UInt32 dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptSetKeyParam(
              IntPtr hKey,
              UInt32 dwParam,
              byte[] pbData,
              UInt32 dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern bool CryptCreateHash
               (
               IntPtr hProv,
               uint algId,
               IntPtr hKey,
               uint dwFlags,
               ref IntPtr phHash
               );
          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern bool CryptDestroyHash(
              IntPtr hHash
              );

          [DllImport("advapi32.dll", SetLastError = true)]
          public static extern bool CryptHashData(
               IntPtr hHash,
               IntPtr pbData,
               uint dataLen,
               uint flags);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern bool CryptGetHashParam(
               IntPtr hHash,
               uint dwParam,
               IntPtr pbData,
               ref uint pdwDataLen,
               uint dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern Boolean CryptGetProvParam(
              IntPtr hProv,
              ProviderParamType dwParam,
              ref IntPtr pbData,
              ref UInt32 pdwDataLen,
              UInt32 dwFlags);

          [DllImport("crypt32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern bool CryptBinaryToString(
               byte[] pbBinary,
               uint cbBinary,
               uint dwFlags,
               StringBuilder pszString,
               ref uint pcchString);

          [DllImport("crypt32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern bool CryptStringToBinary(
               string pszString,
               uint cchString,
               uint dwFlags,
               byte[] pbBinary,
               ref uint pcbBinary,
               uint pdwSkip, 
               uint pdwFlags);

          [DllImport("crypt32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern bool CryptDecodeObject(
            uint CertEncodingType,
            IntPtr lpszStructType,
            byte[] pbEncoded,
            uint cbEncoded,
            uint flags,
            IntPtr pvStructInfo,
            ref uint cbStructInfo);

          [DllImport("crypt32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern bool CryptEncodeObject(
            Int32 dwCertEncodingType,
            /*[MarshalAs(UnmanagedType.LPStr)] string*/ Int32 lpszStructType,
               /*IntPtr*/ ref CERT_ENHKEY_USAGE pvStructInfo,
            /*byte[]*/IntPtr pbEncoded,
            ref uint pcbEncoded);

          [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern Boolean CryptGetProvParam(
              IntPtr hProv,
              ProviderParamType dwParam,
              byte[] pbData,
              ref UInt32 pdwDataLen,
              UInt32 dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern Boolean CryptGetProvParam(
              IntPtr hProv,
              ProviderParamType dwParam,
              [MarshalAs(UnmanagedType.LPStr)]StringBuilder pbData,
              ref UInt32 pdwDataLen,
              UInt32 dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern Boolean CryptGetProvParam(
              IntPtr hProv,
              ProviderParamType dwParam,
              ref UInt32 pbData,
              ref UInt32 pdwDataLen,
              UInt32 dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern Boolean CryptGetProvParam(
              IntPtr hProv,
              ProviderParamType dwParam,
              [Out, MarshalAs(UnmanagedType.LPStruct)] PROV_ENUMALGS pbData,
              ref UInt32 pdwDataLen,
              UInt32 dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
          public static extern Boolean CryptGetProvParam(
              IntPtr hProv,
              ProviderParamType dwParam,
              [Out, MarshalAs(UnmanagedType.LPStruct)] PROV_ENUMALGS_EX pbData,
              ref UInt32 pdwDataLen,
              EnumFlags dwFlags);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptSetProvParam(
            IntPtr hProv,
            ProviderParamType dwParam,
            byte[] pbData,
            UInt32 dwFlags
          );

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptExportKey(
              IntPtr hKey,
              IntPtr hExpKey,
              UInt32 dwBlobType,
              UInt32 dwFlags,
              Byte[] pbData,
              ref Int32 pdwDataLen);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptExportKey(
              IntPtr hKey,
              IntPtr hExpKey,
              UInt32 dwBlobType,
              UInt32 dwFlags,
              IntPtr pbData,
              ref Int32 pdwDataLen);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptGenKey(
              IntPtr hProv,
              UInt32 Algid,
              UInt32 dwFlags,
              ref IntPtr phKey);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptImportKey(
              IntPtr hProv,
              byte[] pbKeyData,
              UInt32 dwDataLen,
              IntPtr hPubKey,
              UInt32 dwFlags,
              ref IntPtr hKey);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern bool CryptSignHash(
              IntPtr hHash,
              uint keySpec,
              IntPtr description,
              uint flags,
              byte[] signature,
              ref UInt32 signatureLen);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern bool CryptVerifySignature(
              IntPtr hHash,
              byte[] signature,
              uint signatureLen,
              IntPtr pubKey,
              IntPtr description,
              uint flags);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptEncrypt(
              IntPtr hKey,
              IntPtr hHash,
              Boolean Final,
              UInt32 dwFlags,
              byte[] pbData,
              ref UInt32 pdwDataLen,
              UInt32 dwBufLen);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptDecrypt(
              IntPtr hKey,
              IntPtr hHash,
              Boolean Final,
              UInt32 dwFlags,
              byte[] pbData,
              ref UInt32 pdwDataLen);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptDuplicateKey(
              IntPtr hKey,
              IntPtr pdwReserved,
              UInt32 dwFlags,
              ref IntPtr phKey);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptDestroyKey(
              IntPtr phKey);

          [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CryptReleaseContext(
              IntPtr hProv,
              UInt32 dwFlags);


          #endregion

          #region Certificate function

          


          public const string OID_RSA_SHA256RSA = "1.2.840.113549.1.1.11";

          [DllImport("Crypt32.dll", SetLastError = true, ExactSpelling = true)]
          public static extern IntPtr CertCreateSelfSignCertificate(
          IntPtr hProv,
          ref CERT_NAME_BLOB pSubjectIssuerBlob,
          uint dwFlagsm,
          ref CRYPT_KEY_PROV_INFO pKeyProvInfo,
          ref CRYPT_ALGORITHM_IDENTIFIER pSignatureAlgorithm,
          IntPtr pStartTime,
          IntPtr pEndTime,
          IntPtr other);

          [DllImport("cryptui.dll", SetLastError = true)]
          public static extern bool CryptUIDlgViewCertificate(
           ref PCCRYPTUI_VIEWCERTIFICATE_STRUCT pCertViewInfo,
           ref bool pfPropertiesChanged);




          [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
          public static extern bool CryptSignAndEncodeCertificate(
               IntPtr hProv,
               uint dwKeySpec,
               uint dwCertEncodingType,
               IntPtr lpszStructType,
               IntPtr pvStructInfo,
               IntPtr pSignatureAlgorithm,
               IntPtr pvHashAuxInfo,
               byte[] pbEncoded,
               ref uint pcbEncoded);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CertGetCertificateContextProperty(
             IntPtr pCertCtx,
             CertPropID dwPropID,
             [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] [In, Out]
           byte[] pbData,
             ref UInt32 pcbData);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CertGetCertificateContextProperty(
             IntPtr pCertCtx,
             CertPropID dwPropID,
             IntPtr pbData,
             ref uint pcbData);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CertSetCertificateContextProperty(
            IntPtr pCertCtx,
            CertPropID dwPropID,
            UInt32 dwFlags,
            IntPtr pbData);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CertAddCertificateContextToStore(
            IntPtr hCertStore,
            IntPtr pCertCtx,
            CertStoreDisposition dwAddDisposition,
            ref IntPtr ppStoreContext);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern Boolean CertFreeCertificateContext(
            IntPtr pCertCtx);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern IntPtr CertDuplicateCertificateContext(
               IntPtr pCertCtx);

          [DllImport("crypt32.dll", SetLastError = true)]
          public static extern bool CertStrToName(
               uint dwCertEncodingType,
               string pszX500,
               uint dwStrType,
               IntPtr pvReserved,
               IntPtr pbEncoded,
               ref uint pcbEncoded,
               StringBuilder ppszError);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern bool CertCloseStore(IntPtr storeProvider, uint flags);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern IntPtr CertOpenStore(
               uint storeProvider, uint encodingType,
             IntPtr hCryptProv, uint flags, String pvPara);

          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern IntPtr CertOpenSystemStore(
               uint storeProvider, uint encodingType,
             IntPtr hCryptProv, uint flags, String pvPara);


          [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          public static extern IntPtr CertFindCertificateInStore(
              IntPtr hCertStore,
              uint dwCertEncodingType,
              uint dwFindFlags,
              uint dwFindType,
              IntPtr pvFindPara,
              IntPtr pPrevCertContext
          );


          
          

          #endregion

          #region Public Key ImportExport

          [DllImport("crypt32.dll", SetLastError = true)]
          public static extern bool CryptExportPublicKeyInfoEx(
            IntPtr hProv,
            uint dwKeySpec,
            uint dwCertEncodingType,
            String pxzPublicKeyObjId,
            uint dwFlags,
            IntPtr pvAuxInfo,
            IntPtr pInfo,
            ref uint pcbInfo);

          [DllImport("crypt32.dll", SetLastError = true)]
          public static extern bool CryptExportPublicKeyInfo(
            IntPtr hProv,
            uint dwKeySpec,
            uint dwCertEncodingType,
            IntPtr pInfo,
            ref uint pcbInfo);

          #endregion

          #region HashParam

          public enum HashParam : uint
          {
               HP_ALGID = 0x0001,   // Hash algorithm
               HP_HASHVAL = 0x0002, // Hash value
               HP_HASHSIZE = 0x0004 // Hash value size
          }
          
          #endregion //HashParam 

          #region Blobs
          [StructLayout(LayoutKind.Sequential)]
          public struct CRYPTOAPI_BLOB
          {
               public uint cbData;
               public IntPtr pbData;
          }

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
          public struct CRYPT_BIT_BLOB
          {
               public uint cbData;
               public IntPtr pbData;
               public uint cUnusedBits;
          }

          #endregion //Blobs

          #region Certificate structure
          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
          public struct CERT_EXTENSIONS
          {
               public int cExtension;
               public IntPtr rgExtension;
          }

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
          public struct CERT_BASIC_CONSTRAINTS2_INFO
          {
               public Boolean fCA;
               public Boolean fPathLenConstraint;
               public int dwPathLenConstraint;
          }

          [StructLayout(LayoutKind.Sequential)]
          public struct CERT_NAME_BLOB
          {
               public uint cbData;
               public IntPtr pbData;
          }

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
          public struct CERT_PUBLIC_KEY_INFO
          {
               public CRYPT_ALGORITHM_IDENTIFIER Algorithm;
               public CRYPT_BIT_BLOB PublicKey;
          }

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
          public struct CRYPT_ALGORITHM_IDENTIFIER
          {
               [MarshalAs(UnmanagedType.LPStr)]
               public String pszObjId;
               public CRYPTOAPI_BLOB Parameters;
          }

          [StructLayout(LayoutKind.Sequential)]
          public struct PCCRYPTUI_VIEWCERTIFICATE_STRUCT
          {
               public uint dwSize;        //required
               public IntPtr hwndParent;
               public uint dwFlags;
               public String szTitle;
               public IntPtr pCertContext;    //required
               public IntPtr rgszPurposes;
               uint cPurposes;
               IntPtr hWVTStateData;
               bool fpCryptProviderDataTrustedUsage;
               uint idxSigner;
               uint idxCert;
               bool fCounterSigner;
               uint idxCounterSigner;
               uint cStores;
               IntPtr rghStores;
               uint cPropSheetPages;
               IntPtr rgPropSheetPages;
               public uint nStartPage;    //required
          }

          #endregion //Certificate structure

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
          public struct SystemTime
          {
               public short Year;
               public short Month;
               public short DayOfWeek;
               public short Day;
               public short Hour;
               public short Minute;
               public short Second;
               public short Milliseconds;
          }

          #region Provider base enums
          public enum ProviderType : uint
          {
               PROV_RSA_FULL = 1,
               PROV_RSA_SIG = 2,
               PROV_DSS = 3,
               PROV_FORTEZZA = 4,
               PROV_MS_EXCHANGE = 5,
               PROV_SSL = 6,
               PROV_RSA_SCHANNEL = 12,
               PROV_DSS_DH = 13,
               PROV_EC_ECDSA_SIG = 14,
               PROV_EC_ECNRA_SIG = 15,
               PROV_EC_ECDSA_FULL = 16,
               PROV_EC_ECNRA_FULL = 17,
               PROV_DH_SCHANNEL = 18,
               PROV_SPYRUS_LYNKS = 20,
               PROV_RNG = 21,
               PROV_INTEL_SEC = 22,
               PROV_REPLACE_OWF = 23,
               PROV_RSA_AES = 24,
          }

          [Flags]
          public enum CertStoreType : uint
          {
               CERT_STORE_PROV_SYSTEM = 10,
          }

          [Flags]
          public enum ProviderImplementationType : uint
          {
               CRYPT_IMPL_HARDWARE = 1,
               CRYPT_IMPL_SOFTWARE = 2,
               CRYPT_IMPL_MIXED = 3,
               CRYPT_IMPL_UNKNOWN = 4,
               CRYPT_IMPL_REMOVABLE = 8,
          }

          public enum ProviderParamType : uint
          {
               PP_CLIENT_HWND = 1,
               PP_ENUMALGS = 1,
               PP_ENUMCONTAINERS = 2,
               PP_IMPTYPE = 3,
               PP_NAME = 4,
               PP_VERSION = 5,
               PP_CONTAINER = 6,
               PP_CHANGE_PASSWORD = 7,
               PP_KEYSET_SEC_DESCR = 8,
               PP_CERTCHAIN = 9,
               PP_KEY_TYPE_SUBTYPE = 10,
               PP_CONTEXT_INFO = 11,
               PP_KEYEXCHANGE_KEYSIZE = 12,
               PP_SIGNATURE_KEYSIZE = 13,
               PP_KEYEXCHANGE_ALG = 14,
               PP_SIGNATURE_ALG = 15,
               PP_PROVTYPE = 16,
               PP_KEYSTORAGE = 17,
               PP_APPLI_CERT = 18,
               PP_SYM_KEYSIZE = 19,
               PP_SESSION_KEYSIZE = 20,
               PP_UI_PROMPT = 21,
               PP_ENUMALGS_EX = 22,
               PP_DELETEKEY = 24,
               PP_ENUMMANDROOTS = 25,
               PP_ENUMELECTROOTS = 26,
               PP_KEYSET_TYPE = 27,
               PP_ADMIN_PIN = 31,
               PP_KEYEXCHANGE_PIN = 32,
               PP_SIGNATURE_PIN = 33,
               PP_SIG_KEYSIZE_INC = 34,
               PP_KEYX_KEYSIZE_INC = 35,
               PP_UNIQUE_CONTAINER = 36,
               PP_SGC_INFO = 37,
               PP_USE_HARDWARE_RNG = 38,
               PP_KEYSPEC = 39,
               PP_ENUMEX_SIGNING_PROT = 40,
               PP_CRYPT_COUNT_KEY_USE = 41,
          }

          [Flags]
          public enum ProtocolFlags : uint
          {
               CRYPT_FLAG_PCT1 = 0x0001,
               CRYPT_FLAG_SSL2 = 0x0002,
               CRYPT_FLAG_SSL3 = 0x0004,
               CRYPT_FLAG_TLS1 = 0x0008,
               CRYPT_FLAG_IPSEC = 0x0010,
               CRYPT_FLAG_SIGNING = 0x0020,
          }

          [Flags]
          public enum ContextFlags : uint
          {
               CRYPT_VERIFYCONTEXT = 0xF0000000,
               CRYPT_NEWKEYSET = 0x00000008,
               CRYPT_DELETEKEYSET = 0x00000010,
               CRYPT_MACHINE_KEYSET = 0x00000020,
               CRYPT_SILENT = 0x00000040,
          }

          [Flags]
          public enum KeySpecifier : uint
          {
               AT_KEYEXCHANGE = 1,
               AT_SIGNATURE = 2,
          }

          public enum KeyParameter : uint
          {
               KP_IV = 1,
               KP_SALT = 2,
               KP_PADDING = 3,
               KP_MODE = 4,
               KP_MODE_BITS = 5,
               KP_PERMISSIONS = 6,
               KP_ALGID = 7,
               KP_BLOCKLEN = 8,
               KP_KEYLEN = 9,
               KP_SALT_EX = 10,
               KP_P = 11,
               KP_G = 12,
               KP_Q = 13,
               KP_X = 14,
               KP_Y = 15,
               KP_RA = 16,
               KP_RB = 17,
               KP_INFO = 18,
               KP_EFFECTIVE_KEYLEN = 19,
               KP_SCHANNEL_ALG = 20,
               KP_CLIENT_RANDOM = 21,
               KP_SERVER_RANDOM = 22,
               KP_RP = 23,
               KP_PRECOMP_MD5 = 24,
               KP_PRECOMP_SHA = 25,
               KP_CERTIFICATE = 26,
               KP_CLEAR_KEY = 27,
               KP_PUB_EX_LEN = 28,
               KP_PUB_EX_VAL = 29,
               KP_KEYVAL = 30,
               KP_ADMIN_PIN = 31,
               KP_KEYEXCHANGE_PIN = 32,
               KP_SIGNATURE_PIN = 33,
               KP_PREHASH = 34,
               KP_ROUNDS = 35,
               KP_OAEP_PARAMS = 36,
               KP_CMS_KEY_INFO = 37,
               KP_CMS_DH_KEY_INFO = 38,
               KP_PUB_PARAMS = 39,
               KP_VERIFY_PARAMS = 40,
               KP_HIGHEST_VERSION = 41,
               KP_GET_USE_COUNT = 42,
          }

          public enum KeyMode : uint
          {
               CRYPT_MODE_CBC = 1,
               CRYPT_MODE_ECB = 2,
               CRYPT_MODE_OFB = 3,
               CRYPT_MODE_CFB = 4,
               CRYPT_MODE_CTS = 5,
               CRYPT_MODE_CBCI = 6,
               CRYPT_MODE_CFBP = 7,
               CRYPT_MODE_OFBP = 8,
               CRYPT_MODE_CBCOFM = 9,
               CRYPT_MODE_CBCOFMI = 10,
          }

          public enum KeyPadding : uint
          {
               PKCS5_PADDING = 1,
               RANDOM_PADDING = 2,
               ZERO_PADDING = 3,
          }

          [Flags]
          public enum KeyPermissions : uint
          {
               CRYPT_ENCRYPT = 0x0001,
               CRYPT_DECRYPT = 0x0002,
               CRYPT_EXPORT = 0x0004,
               CRYPT_READ = 0x0008,
               CRYPT_WRITE = 0x0010,
               CRYPT_MAC = 0x0020,
               CRYPT_EXPORT_KEY = 0x0040,
               CRYPT_IMPORT_KEY = 0x0080,
               CRYPT_ARCHIVE = 0x0100,
          }

          [Flags]
          public enum GenFlags : uint
          {
               CRYPT_EXPORTABLE = 0x00000001,
               CRYPT_USER_PROTECTED = 0x00000002,
               CRYPT_CREATE_SALT = 0x00000004,
               CRYPT_UPDATE_KEY = 0x00000008,
               CRYPT_NO_SALT = 0x00000010,
               CRYPT_PREGEN = 0x00000040,
               CRYPT_RECIPIENT = 0x00000010,
               CRYPT_INITIATOR = 0x00000040,
               CRYPT_ONLINE = 0x00000080,
               CRYPT_SF = 0x00000100,
               CRYPT_CREATE_IV = 0x00000200,
               CRYPT_KEK = 0x00000400,
               CRYPT_DATA_KEY = 0x00000800,
               CRYPT_VOLATILE = 0x00001000,
               CRYPT_SGCKEY = 0x00002000,
               CRYPT_ARCHIVABLE = 0x00004000,
               CRYPT_FORCE_KEY_PROTECTION_HIGH = 0x00008000,
          }

          public enum EnumFlags : uint
          {
               CRYPT_FIRST = 1,
               CRYPT_NEXT = 2,
               CRYPT_SGC_ENUM = 4,
          }

          public enum AlgorithmClass : uint
          {
               ALG_CLASS_ANY = (0),
               ALG_CLASS_SIGNATURE = (1 << 13),
               ALG_CLASS_MSG_ENCRYPT = (2 << 13),
               ALG_CLASS_DATA_ENCRYPT = (3 << 13),
               ALG_CLASS_HASH = (4 << 13),
               ALG_CLASS_KEY_EXCHANGE = (5 << 13),
               ALG_CLASS_ALL = (7 << 13),
          }

          public enum AlgorithmType : uint
          {
               ALG_TYPE_ANY = (0),
               ALG_TYPE_DSS = (1 << 9),
               ALG_TYPE_RSA = (2 << 9),
               ALG_TYPE_BLOCK = (3 << 9),
               ALG_TYPE_STREAM = (4 << 9),
               ALG_TYPE_DH = (5 << 9),
               ALG_TYPE_SECURECHANNEL = (6 << 9),
          }

          public enum GenericSubID : uint
          {
               ALG_SID_ANY = 0,
          }

          public enum RSASubID : uint
          {
               ALG_SID_RSA_ANY = 0,
               ALG_SID_RSA_PKCS = 1,
               ALG_SID_RSA_MSATWORK = 2,
               ALG_SID_RSA_ENTRUST = 3,
               ALG_SID_RSA_PGP = 4,
          }

          public enum DSSSubID : uint
          {
               ALG_SID_DSS_ANY = 0,
               ALG_SID_DSS_PKCS = 1,
               ALG_SID_DSS_DMS = 2,
          }

          public enum BlockCipherSubID : uint
          {
               ALG_SID_DES = 1,
               ALG_SID_RC2 = 2,
               ALG_SID_3DES = 3,
               ALG_SID_DESX = 4,
               ALG_SID_IDEA = 5,
               ALG_SID_CAST = 6,
               ALG_SID_SAFERSK64 = 7,
               ALG_SID_SAFERSK128 = 8,
               ALG_SID_3DES_112 = 9,
               ALG_SID_SKIPJACK = 10,
               ALG_SID_TEK = 11,
               ALG_SID_CYLINK_MEK = 12,
               ALG_SID_RC5 = 13,
               ALG_SID_AES_128 = 14,
               ALG_SID_AES_192 = 15,
               ALG_SID_AES_256 = 16,
               ALG_SID_AES = 17,
          }

          public enum StreamCipherSubID : uint
          {
               ALG_SID_RC4 = 1,
               ALG_SID_SEAL = 2,
          }

          public enum DHSubID : uint
          {
               ALG_SID_DH_SANDF = 1,
               ALG_SID_DH_EPHEM = 2,
               ALG_SID_AGREED_KEY_ANY = 3,
               ALG_SID_KEA = 4,
          }

          public enum HashSubID : uint
          {
               ALG_SID_MD2 = 1,
               ALG_SID_MD4 = 2,
               ALG_SID_MD5 = 3,
               ALG_SID_SHA = 4,
               ALG_SID_SHA1 = 4,
               ALG_SID_MAC = 5,
               ALG_SID_RIPEMD = 6,
               ALG_SID_RIPEMD160 = 7,
               ALG_SID_SSL3SHAMD5 = 8,
               ALG_SID_HMAC = 9,
               ALG_SID_TLS1PRF = 10,
               ALG_SID_HASH_REPLACE_OWF = 11,
               ALG_SID_SHA_256 = 12,
               ALG_SID_SHA_384 = 13,
               ALG_SID_SHA_512 = 14,
          }

          public enum SChannelSubID : uint
          {
               ALG_SID_SSL3_MASTER = 1,
               ALG_SID_SCHANNEL_MASTER_HASH = 2,
               ALG_SID_SCHANNEL_MAC_KEY = 3,
               ALG_SID_PCT1_MASTER = 4,
               ALG_SID_SSL2_MASTER = 5,
               ALG_SID_TLS1_MASTER = 6,
               ALG_SID_SCHANNEL_ENC_KEY = 7,
          }

          public enum ALG_ID : uint
          {
               CALG_MD2 = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_MD2),
               CALG_MD4 = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_MD4),
               CALG_MD5 = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_MD5),
               CALG_SHA = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_SHA),
               CALG_SHA1 = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_SHA1),
               CALG_MAC = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_MAC),
               CALG_RSA_SIGN = (AlgorithmClass.ALG_CLASS_SIGNATURE | AlgorithmType.ALG_TYPE_RSA | RSASubID.ALG_SID_RSA_ANY),
               CALG_DSS_SIGN = (AlgorithmClass.ALG_CLASS_SIGNATURE | AlgorithmType.ALG_TYPE_DSS | DSSSubID.ALG_SID_DSS_ANY),
               CALG_NO_SIGN = (AlgorithmClass.ALG_CLASS_SIGNATURE | AlgorithmType.ALG_TYPE_ANY | GenericSubID.ALG_SID_ANY),
               CALG_RSA_KEYX = (AlgorithmClass.ALG_CLASS_KEY_EXCHANGE | AlgorithmType.ALG_TYPE_RSA | RSASubID.ALG_SID_RSA_ANY),
               CALG_DES = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_DES),
               CALG_3DES_112 = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_3DES_112),
               CALG_3DES = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_3DES),
               CALG_DESX = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_DESX),
               CALG_RC2 = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_RC2),
               CALG_RC4 = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_STREAM | StreamCipherSubID.ALG_SID_RC4),
               CALG_SEAL = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_STREAM | StreamCipherSubID.ALG_SID_SEAL),
               CALG_DH_SF = (AlgorithmClass.ALG_CLASS_KEY_EXCHANGE | AlgorithmType.ALG_TYPE_DH | DHSubID.ALG_SID_DH_SANDF),
               CALG_DH_EPHEM = (AlgorithmClass.ALG_CLASS_KEY_EXCHANGE | AlgorithmType.ALG_TYPE_DH | DHSubID.ALG_SID_DH_EPHEM),
               CALG_AGREEDKEY_ANY = (AlgorithmClass.ALG_CLASS_KEY_EXCHANGE | AlgorithmType.ALG_TYPE_DH | DHSubID.ALG_SID_AGREED_KEY_ANY),
               CALG_KEA_KEYX = (AlgorithmClass.ALG_CLASS_KEY_EXCHANGE | AlgorithmType.ALG_TYPE_DH | DHSubID.ALG_SID_KEA),
               CALG_HUGHES_MD5 = (AlgorithmClass.ALG_CLASS_KEY_EXCHANGE | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_MD5),
               CALG_SKIPJACK = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_SKIPJACK),
               CALG_TEK = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_TEK),
               CALG_CYLINK_MEK = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_CYLINK_MEK),
               CALG_SSL3_SHAMD5 = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_SSL3SHAMD5),
               CALG_SSL3_MASTER = (AlgorithmClass.ALG_CLASS_MSG_ENCRYPT | AlgorithmType.ALG_TYPE_SECURECHANNEL | SChannelSubID.ALG_SID_SSL3_MASTER),
               CALG_SCHANNEL_MASTER_HASH = (AlgorithmClass.ALG_CLASS_MSG_ENCRYPT | AlgorithmType.ALG_TYPE_SECURECHANNEL | SChannelSubID.ALG_SID_SCHANNEL_MASTER_HASH),
               CALG_SCHANNEL_MAC_KEY = (AlgorithmClass.ALG_CLASS_MSG_ENCRYPT | AlgorithmType.ALG_TYPE_SECURECHANNEL | SChannelSubID.ALG_SID_SCHANNEL_MAC_KEY),
               CALG_SCHANNEL_ENC_KEY = (AlgorithmClass.ALG_CLASS_MSG_ENCRYPT | AlgorithmType.ALG_TYPE_SECURECHANNEL | SChannelSubID.ALG_SID_SCHANNEL_ENC_KEY),
               CALG_PCT1_MASTER = (AlgorithmClass.ALG_CLASS_MSG_ENCRYPT | AlgorithmType.ALG_TYPE_SECURECHANNEL | SChannelSubID.ALG_SID_PCT1_MASTER),
               CALG_SSL2_MASTER = (AlgorithmClass.ALG_CLASS_MSG_ENCRYPT | AlgorithmType.ALG_TYPE_SECURECHANNEL | SChannelSubID.ALG_SID_SSL2_MASTER),
               CALG_TLS1_MASTER = (AlgorithmClass.ALG_CLASS_MSG_ENCRYPT | AlgorithmType.ALG_TYPE_SECURECHANNEL | SChannelSubID.ALG_SID_TLS1_MASTER),
               CALG_RC5 = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_RC5),
               CALG_HMAC = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_HMAC),
               CALG_TLS1PRF = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_TLS1PRF),
               CALG_HASH_REPLACE_OWF = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_HASH_REPLACE_OWF),
               CALG_AES_128 = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_AES_128),
               CALG_AES_192 = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_AES_192),
               CALG_AES_256 = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_AES_256),
               CALG_AES = (AlgorithmClass.ALG_CLASS_DATA_ENCRYPT | AlgorithmType.ALG_TYPE_BLOCK | BlockCipherSubID.ALG_SID_AES),
               CALG_SHA_256 = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_SHA_256),
               CALG_SHA_384 = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_SHA_384),
               CALG_SHA_512 = (AlgorithmClass.ALG_CLASS_HASH | AlgorithmType.ALG_TYPE_ANY | HashSubID.ALG_SID_SHA_512), s
          }

          #endregion


          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
          public class PROV_ENUMALGS
          {
               public ALG_ID aiAlgid;
               public UInt32 dwBitLen;
               public UInt32 dwNameLen;
               [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
               public string szName = new string(' ', 20);
               public static UInt32 dwSize = 32;
          }

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
          public class PROV_ENUMALGS_EX
          {
               public ALG_ID aiAlgid;
               public UInt32 dwDefaultLen;
               public UInt32 dwMinLen;
               public UInt32 dwMaxLen;
               [MarshalAs(UnmanagedType.U4)]
               public ProtocolFlags dwProtocols;
               public UInt32 dwNameLen;
               [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
               public string szName = new string(' ', 20);
               public UInt32 dwLongNameLen;
               [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
               public string szLongName = new string(' ', 40);
               public static UInt32 dwSize = 88;
          }

          public enum CertPropID : uint
          {
               CERT_KEY_PROV_HANDLE_PROP_ID = 1,
               CERT_KEY_PROV_INFO_PROP_ID = 2,
               CERT_SHA1_HASH_PROP_ID = 3,
               CERT_MD5_HASH_PROP_ID = 4,
               CERT_HASH_PROP_ID = CERT_SHA1_HASH_PROP_ID,
               CERT_KEY_CONTEXT_PROP_ID = 5,
               CERT_KEY_SPEC_PROP_ID = 6,
               CERT_IE30_RESERVED_PROP_ID = 7,
               CERT_PUBKEY_HASH_RESERVED_PROP_ID = 8,
               CERT_ENHKEY_USAGE_PROP_ID = 9,
               CERT_CTL_USAGE_PROP_ID = CERT_ENHKEY_USAGE_PROP_ID,
               CERT_NEXT_UPDATE_LOCATION_PROP_ID = 10,
               CERT_FRIENDLY_NAME_PROP_ID = 11,
               CERT_PVK_FILE_PROP_ID = 12,
               CERT_DESCRIPTION_PROP_ID = 13,
               CERT_ACCESS_STATE_PROP_ID = 14,
               CERT_SIGNATURE_HASH_PROP_ID = 15,
               CERT_SMART_CARD_DATA_PROP_ID = 16,
               CERT_EFS_PROP_ID = 17,
               CERT_FORTEZZA_DATA_PROP_ID = 18,
               CERT_ARCHIVED_PROP_ID = 19,
               CERT_KEY_IDENTIFIER_PROP_ID = 20,
               CERT_AUTO_ENROLL_PROP_ID = 21,
               CERT_PUBKEY_ALG_PARA_PROP_ID = 22,
               CERT_CROSS_CERT_DIST_POINTS_PROP_ID = 23,
               CERT_ISSUER_PUBLIC_KEY_MD5_HASH_PROP_ID = 24,
               CERT_SUBJECT_PUBLIC_KEY_MD5_HASH_PROP_ID = 25,
               CERT_ENROLLMENT_PROP_ID = 26,
               CERT_DATE_STAMP_PROP_ID = 27,
               CERT_ISSUER_SERIAL_NUMBER_MD5_HASH_PROP_ID = 28,
               CERT_SUBJECT_NAME_MD5_HASH_PROP_ID = 29,
               CERT_EXTENDED_ERROR_INFO_PROP_ID = 30,

               // Note, 32 - 35 are reserved for the CERT, CRL, CTL and KeyId file element IDs.
               //       36 - 63 are reserved for future element IDs.

               CERT_RENEWAL_PROP_ID = 64,
               CERT_ARCHIVED_KEY_HASH_PROP_ID = 65,
               CERT_AUTO_ENROLL_RETRY_PROP_ID = 66,
               CERT_AIA_URL_RETRIEVED_PROP_ID = 67,
               // Note, 68 - 70 are reserved for future use.
               CERT_REQUEST_ORIGINATOR_PROP_ID = 71,
               CERT_FIRST_RESERVED_PROP_ID = 72,

               CERT_LAST_RESERVED_PROP_ID = 0x00007FFF,
               CERT_FIRST_USER_PROP_ID = 0x00008000,
               CERT_LAST_USER_PROP_ID = 0x0000FFFF,
          }

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
          public struct CERT_ENHKEY_USAGE
          {
               public uint cUsageIdentifier;
               public IntPtr rgpszUsageIdentifier;
          }

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
          public struct CERT_EXTENSION
          {
               public string pszObjId;
               public bool fCritical;
               public CRYPTOAPI_BLOB Value;
          }

          public enum CertStoreDisposition : uint
          {
               CERT_STORE_ADD_NEW = 1,
               CERT_STORE_ADD_USE_EXISTING = 2,
               CERT_STORE_ADD_REPLACE_EXISTING = 3,
               CERT_STORE_ADD_ALWAYS = 4,
               CERT_STORE_ADD_REPLACE_EXISTING_INHERIT_PROPERTIES = 5,
               CERT_STORE_ADD_NEWER = 6,
               CERT_STORE_ADD_NEWER_INHERIT_PROPERTIES = 7
          }

          [Flags]
          public enum KeyFlags : uint
          {
               CERT_SET_KEY_PROV_HANDLE_PROP_ID = 1,
               CRYPT_MACHINE_KEYSET = 32,
               CRYPT_SILENT = 64,
          }

          [Flags]
          public enum EncodingType : uint
          {
               X509_ASN_ENCODING = 0x00000001,
               PKCS_7_ASN_ENCODING = 0x00010000,
          }

          [Flags]
          public enum CertNameStringType : uint 
          {
               CERT_X500_NAME_STR = 3,
          }

          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
          public class CRYPT_KEY_PROV_PARAM
          {
               public UInt32 dwParam;
               [MarshalAs(UnmanagedType.LPArray,
                     SizeParamIndex = 2,
                     ArraySubType = UnmanagedType.U1)]
               public byte[] pbData;
               public UInt32 cbData;
               public UInt32 dwFlags;
          }

          [StructLayout(LayoutKind.Sequential)]
          public struct CRYPT_KEY_PROV_INFO
          {
               [MarshalAs(UnmanagedType.LPWStr)]
               public String pwszContainerName;
               [MarshalAs(UnmanagedType.LPWStr)]
               public String pwszProvName;
               public uint dwProvType;
               public uint dwFlags;
               public uint cProvParam;
               public IntPtr rgProvParam;
               public uint dwKeySpec;
          }

          [StructLayout(LayoutKind.Sequential)]
          public struct CRYPT_ATTR_BLOB
          {
               public uint cbData;
               [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
               public byte[] pbData;
          }

          [StructLayout(LayoutKind.Sequential)]
          public struct CRYPT_ATTRIBUTE
          {
               [MarshalAs(UnmanagedType.LPStr)]
               public string pszObjId;
               public uint cValue;
               [MarshalAs(UnmanagedType.LPStruct)]
               public CRYPT_ATTR_BLOB rgValue;
          }

          [StructLayout(LayoutKind.Sequential)]
          public struct CERT_REQUEST_INFO
          {
               public const uint CERT_REQUEST_V1 = 0;
               public uint dwVersion;
               public CERT_NAME_BLOB Subject;
               public CERT_PUBLIC_KEY_INFO SubjectPublicKeyInfo;
               public uint cAttribute;
               public IntPtr rgAttribute;
          }

          [Flags]
          public enum CertVersion : uint
          {
               CERT_V1 = 0,
          }

          [StructLayout(LayoutKind.Sequential)]
          public struct CERT_INFO
          {
               public uint dwVersion;
               public CRYPTOAPI_BLOB SerialNumber;
               public CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
               public CERT_NAME_BLOB Issuer;
               public System.Runtime.InteropServices.ComTypes.FILETIME  NotBefore;
               public System.Runtime.InteropServices.ComTypes.FILETIME  NotAfter;
               public CERT_NAME_BLOB Subject;
               public CERT_PUBLIC_KEY_INFO SubjectPublicKeyInfo;
               public CRYPT_BIT_BLOB IssuerUniqueId;
               public CRYPT_BIT_BLOB SubjectUniqueId;
               public uint cExtension;
               public IntPtr rgExtension;
          }

          [StructLayout(LayoutKind.Sequential)]
          public struct CERT_CONTEXT
          {
               public uint dwCertEncodingType;
               public IntPtr pbCertEncoded;
               public uint cbCertEncoded;
               public IntPtr pCertInfo;
               public IntPtr hCertStore;
          }
     }
}
