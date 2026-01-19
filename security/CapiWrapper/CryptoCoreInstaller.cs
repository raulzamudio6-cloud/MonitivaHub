using System;
using System.Collections.Generic;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Создает на основе настроек ключи и сертификат системы
    /// </summary>
    public class CryptoCoreInstaller
     {
          /// <summary>
          /// Настройки
          /// </summary>
          private ProviderSettings Settings = null;
          
          public CryptoCoreInstaller(ProviderSettings settings)
          {
               Settings = settings;
          }

          /// <summary>
          /// Создана ли система с данными параметрами
          /// </summary>
          /// <returns>Система существует</returns>
          public bool CkeckSystemExist()
          {
               List<string> keys = CryptoSystemProperty.GetContainerList(Settings.Name, Settings.Type);
               return keys.Contains(Settings.KeyName);
          }

          /// <summary>
          /// Проверяет корректность параметров настройки системы
          /// </summary>
          /// <param name="result">Лог с результатом прокреки</param>
          /// <returns>Верны ли настройки</returns>
          bool CheckSettings(FunctionResult result)
          {
               bool check = true;

               if (string.IsNullOrEmpty(Settings.CompanyName))
               {
                    result.AddToLog("App setting is incorect!. Company name is empty.");
                    check = false;
               }

               if (string.IsNullOrEmpty(Settings.KeyName))
               {
                    result.AddToLog("App setting is incorect!. Key name is empty.");
                    check = false;
               }

               if (string.IsNullOrEmpty(Settings.Name))
               {
                    result.AddToLog("App setting is incorect!. Provider name is empty.");
                    check = false;
               }

               if (Settings.Type == 0)
               {
                    result.AddToLog("App setting is incorect!. Provider type is empty.");
                    check = false;
               }

               return check;
          }

          /// <summary>
          /// Создать сертификат и ключи
          /// </summary>
          /// <returns>Результат выполнения операции</returns>
          public FunctionResult Install(ref byte[] certPrint)
          {
               FunctionResult result = new FunctionResult();
               result.AddToLog(string.Format("Start install crypto core for company\"{0}\" ;", Settings.CompanyName));
               byte[] certBlob = null;
               try
               {
                    if (!CheckSettings(result))
                    {
                         return result;
                    }

                    using (ProvPtr prov = new ProvPtr())
                    {
                         result.AddToLog(string.Format("Create new key: {0}", Settings.KeyName));
                         CryptoFactory.CreateNewContainer(Settings.Name, Settings.Type, Settings.KeyName, null, false);

                         result.AddToLog(string.Format("Create new certificate: {0}", Settings.CompanyName));
                         DateTime start = DateTime.Now;
                         DateTime end = start.AddYears(5);
                         CryptoFactory.CreateSelfSignedCert(Settings.Name, Settings.Type, Settings.KeyName, string.Format("CN = {0}", Settings.CompanyName), start, end, ref certPrint, ref certBlob, "ROOT");
                         result.AddToLog(string.Format("Certficate print: {0}", CryptoFactory.ByteToHexString(certPrint)));
                    }
               }
               catch (CapiException ex)
               {
                    result.AddToLog(ex);
                    return result;
               }
               catch (SystemException ex)
               {
                    result.AddToLog(String.Format("System ERROR! {0}; Source:{1}; StackTrace: {2}.", ex.Message, ex.Source, ex.StackTrace));
                    return result;
               }

               result.AddToLog(string.Format("Sucessful install crypto core for company\"{0}\" ;", Settings.CompanyName));
               result.ResultOperation = true;
               
               return result;
          }
     }
}
