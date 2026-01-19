using System;
using System.Configuration;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    public class SettingsReader
     {
          private static string nameSection = "Name";
          private static string typeSection = "Type";
          private static string exportableKeySection = "ExportableKey";
          private static string chipherAlgSection = "ChipherAlg";
          private static string hashAlgSection = "HashAlg";
          private static string protectionSection = "Protection";
          private static string companyNameSection = "CompanyName";
          private static string keyNameSection = "KeyName";
          private static string caSection = "CA";

          public static byte[] ConvertHexStringToByte(string hex)
          {
               Convert.ToByte(hex, 16);
               if (hex.Length % 2 == 1)
               {
                    throw new Exception("The binary key cannot have an odd number of digits");
               }

               byte[] arr = new byte[hex.Length >> 1];

               for (int i = 0; i < hex.Length >> 1; ++i)
               {
                    arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
               }

               return arr;
          }

          public static int GetHexVal(char hex) 
          {
               int val = (int)hex;
               //For uppercase A-F letters:
               return val - (val < 58 ? 48 : 55);
               //For lowercase a-f letters:
               //return val - (val < 58 ? 48 : 87);
               //Or the two combined, but a bit slower:
               //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
          }

          public static ProviderSettings ReadAppSettings()
          {
               string name = ConfigurationManager.AppSettings[nameSection];
               uint type = Convert.ToUInt32(ConfigurationManager.AppSettings[typeSection]);
               bool exportable = Convert.ToBoolean(ConfigurationManager.AppSettings[exportableKeySection]);
               uint chipher = Convert.ToUInt32(ConfigurationManager.AppSettings[chipherAlgSection], 16);
               uint hash = Convert.ToUInt32(ConfigurationManager.AppSettings[hashAlgSection], 16);
               uint protect = Convert.ToUInt32(ConfigurationManager.AppSettings[protectionSection]);
               string companyName = ConfigurationManager.AppSettings[companyNameSection];
               string keyName = ConfigurationManager.AppSettings[keyNameSection];
               //byte[] ca = { 0xad, 0xbd, 0x9b, 0x10, 0xff, 0xbc, 0x75, 0x0d, 0xc8, 0x5f, 0xb4, 0x0c, 0x0b, 0xc3, 0x76, 0x82, 0x35, 0x79, 0x16, 0x7e };
               string caHashStr = ConfigurationManager.AppSettings[caSection].Length%2 == 0 ? ConfigurationManager.AppSettings[caSection] : string.Format("0{0}", ConfigurationManager.AppSettings[caSection]);

               byte[] ca = new byte[caHashStr.Length/2];
               for(int i = 0; i < caHashStr.Length/2; ++i)
               {
                    ca[i] = Convert.ToByte(string.Format("{0}{1}", ConfigurationManager.AppSettings[caSection][i*2],ConfigurationManager.AppSettings[caSection][i*2+1]), 16);
               }
               
               

               return new ProviderSettings(name, type, exportable, chipher, hash, protect, keyName, companyName, ca);
          }
     }
}
