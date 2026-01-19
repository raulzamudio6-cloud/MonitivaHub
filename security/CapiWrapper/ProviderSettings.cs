using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Настройки криптоядра
    /// </summary>
    public class ProviderSettings
     {
          /// <summary>
          /// Тип парольной защиты 
          /// </summary>
          public enum PinProtection
          {
               NONE = 0,
               PIN = 1,
               WinPassword = 2,
          };

          /// <summary>
          /// Имя криптоядра
          /// </summary>
          public String Name
          {
               get;
               protected set;
          }

          public uint Type
          {
               get;
               protected set;
          }

          public bool Exportable
          {
               get;
               protected set;
          }

          public CAPI.ALG_ID Chipher
          {
               get;
               protected set;
          }

          public CAPI.ALG_ID Hash
          {
               get;
               protected set;
          }

          public ProviderSettings.PinProtection Protection
          {
               get;
               protected set;
          }

          public string KeyName
          {
               get;
               protected set;
          }

          public string CompanyName
          {
               get;
               protected set;
          }

          public byte[] CA
          {
               get;
               set;
          }

          public ProviderSettings(string name, uint type, bool export, uint chipher, uint hash, uint protect, string keyName, string companyName, byte[] hashCA)
          {
               Name = name;
               Type = type;
               Exportable = export;
               Chipher = (CAPI.ALG_ID)chipher;
               Hash = (CAPI.ALG_ID)hash;
               CompanyName = companyName;
               KeyName = keyName;
               CA = hashCA;

               switch (protect)
               {
                    case 1: Protection = ProviderSettings.PinProtection.PIN; break;
                    case 2: Protection = ProviderSettings.PinProtection.WinPassword; break;
                    default: Protection = ProviderSettings.PinProtection.NONE; break;
               }
          }
     }
}
