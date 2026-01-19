using System;
using System.Collections.Generic;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Параметры криптоядра
    /// </summary>
    internal class ProviderInfo
     {
          /// <summary>
          /// Имя криптоппровайдера
          /// </summary>
          public string Name;
          /// <summary>
          /// Тип криптопровайдера
          /// </summary>
          public uint Type;
          /// <summary>
          /// Алгоритмы хеширования
          /// </summary>
          public List<String> HashList;
          /// <summary>
          /// Алгоритмы шифрования
          /// </summary>
          public List<String> ChipherList;
          /// <summary>
          /// Алгоритмы подписи
          /// </summary>
          public List<String> SignList;
          /// <summary>
          /// Ключи
          /// </summary>
          public List<String> ContainerList;
          /// <summary>
          /// Информация о криптопровайдере
          /// </summary>
          public ProviderInfo()
          { 
          }
     }
}
