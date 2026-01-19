using System;
using System.Collections.Generic;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Криптографическое ядро
    /// </summary>
    public class CryptoSystem
     {
          /// <summary>
          /// Настройки ядра
          /// </summary>
          private ProviderSettings Settings = null;

          public CryptoSystem(ProviderSettings settings)
          {
               Settings = settings;
          }

          /// <summary>
          /// Создание ключа
          /// </summary>
          /// <param name="keyName">Имя ключа</param>
          /// <param name="pin">Пароль</param>
          public void CreateNewContainer(string keyName, byte[] pin, bool shortKey)
          {
               CryptoFactory.CreateNewContainer(Settings.Name, Settings.Type, keyName, pin, shortKey);        
          }

          public void CreateNewContainer(ProvPtr prov, string keyName, byte[] pin, bool shortKey)
          {
              CryptoFactory.CreateNewContainer(prov, Settings.Name, Settings.Type, keyName, pin, shortKey);
          }

          public void OpenContainer(ProvPtr prov, string keyName, byte[] pwd)
          {
              CryptoFactory.OpenContainer(prov, Settings.Name, Settings.Type, keyName, pwd);
          }

          /// <summary>
          /// Удалить ключ
          /// </summary>
          /// <param name="keyName">Имя ключа</param>
          public void DeleteContainer(string keyName)
          {
               CryptoFactory.DeleteContainer(Settings.Name, Settings.Type, keyName);
          }

          public void OpenContainer(string keyName, byte[] pwd)
          {
               CryptoFactory.OpenContainer(Settings.Name, Settings.Type, keyName, pwd);
          }

          public byte[] SignData(string keyName, byte[] data, byte[] pwd)
          {
               return CryptoFactory.SignData(Settings.Name, Settings.Type, keyName, (uint)Settings.Hash, pwd, data);
          }

          public bool VerifyData(byte[] pbKey, byte[] data, byte[] sign)
          {
               return CryptoFactory.VerifySignData(Settings.Name, Settings.Type, (uint)Settings.Hash, pbKey, data, sign);
          }

          /// <summary>
          /// Возвращает список ключей
          /// </summary>
          /// <returns></returns>
          public List<string> GetContainerList()
          {
               return CryptoSystemProperty.GetContainerList(Settings.Name, Settings.Type);
          }

          /// <summary>
          /// Возвращает список хеш алгоритмов
          /// </summary>
          /// <returns></returns>
          public List<String> GetHashList()
          {
               CapiWrapper.ProviderInfo info = CryptoSystemProperty.GetProvInfo(Settings.Name, Settings.Type);
               return info.HashList;
          }

          /// <summary>
          /// Хеширует данные
          /// </summary>
          /// <param name="alg">Алгоритм хеширования</param>
          /// <param name="input">Входные данные</param>
          /// <returns>Хеш</returns>
          public byte[] CreateHash(string alg, byte[] input)
          {
               return CryptoFactory.CreateHashValue(Settings.Name, Settings.Type, alg, input);
          }

          /// <summary>
          /// Хеширует данные
          /// </summary>
          /// <param name="alg">Алгоритм хеширования</param>
          /// <param name="input">Входные данные</param>
          /// <returns>строковое представление хеша</returns>
          public string CreateHashString(string alg, byte[] input)
          {
              return CryptoFactory.ByteToHexString(CreateHash(alg, input));
          }

          /// <summary>
          /// Создание самоподписанного сертификата
          /// </summary>
          /// <param name="keyName">Имя ключа</param>
          /// <param name="subject">Subject</param>
          public void CreateSelfSignedCert(string keyName, string subject)
          {
               byte[] hashCert = null;
               byte[] cert = null;
               DateTime start = DateTime.Now;
               DateTime end = start.AddYears(1);
               CryptoFactory.CreateSelfSignedCert(Settings.Name, Settings.Type, keyName, subject, start, end, ref hashCert, ref cert, "");
          }

          public byte[] CreateCert(string request, ref byte[] hashCert)
          {
               DateTime start = DateTime.Now;
               DateTime end = start.AddYears(1);
               return CryptoFactory.CreateCert(Settings.Name, Settings.Type, request, start, end, Settings.CA, ref hashCert);
               
          }

          /// <summary>
          /// Создание запроса на сертификата
          /// </summary>
          /// <param name="keyName">Имя ключа</param>
          /// <param name="subject">Subject</param>
          public string GetCertRequest(ProvPtr prov, string subject)
          {
               return CryptoFactory.GetCertRequest(prov, subject);
          }

          public string GetCertRequest(string keyName, string subject, byte[] password)
          {
              return CryptoFactory.GetCertRequest(Settings.Name, Settings.Type, keyName, subject, password);
          }

          public byte[] ExportPublicKey(string keyName, byte[] password)
          {
              return CryptoFactory.GetPublicKey(Settings.Name, Settings.Type, keyName, password);
          }

          public void AddCertificateToKey(string keyName, byte[] cert, byte[] pwd)
          {
               CryptoFactory.AddCertToKey(Settings.Name, Settings.Type, keyName, pwd, cert); 
          }
     }
}
