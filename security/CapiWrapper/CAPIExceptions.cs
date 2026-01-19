using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Класс исключений генерируемых Crypto API функциями
    /// </summary>
    public class CapiException : SystemException
     {
          /// <summary>
          /// Код ошибки
          /// </summary>
          public int Error
          {
               get;
               private set;

          }
          /// <summary>
          /// Конструктор исключений
          /// </summary>
          /// <param name="error">Код ошибки</param>
          public CapiException(int error)
          {
               Error = error;
          }

          /// <summary>
          /// Сообщение об ошибке
          /// </summary>
          public string Text
          {
               get 
               {
                    return new System.ComponentModel.Win32Exception(Error).Message;
               }
          }
     };
}
