using System;
using System.Collections.Generic;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    /// <summary>
    /// Результат выполнения операции
    /// </summary>
    public class FunctionResult
     {
          /// <summary>
          /// Ход выполнения операции
          /// </summary>
          private List<string> m_log = new List<string>();
          
          /// <summary>
          /// Результат операции
          /// </summary>
          public bool ResultOperation = false;

          /// <summary>
          /// Добавить информацию о ходе выполнения операции
          /// </summary>
          /// <param name="entity">Информация</param>
          public void AddToLog(string entity)
          {
               m_log.Add(string.Format("{0} {1}", DateTime.UtcNow.ToString(), entity));
          }

          public void AddToLog(CapiException ex)
          {
               AddToLog(String.Format("CAPI ERROR! \"{1}\"Code: 0x{0:x} ; Source:{2}; StackTrace: {3}.", ex.Error, ex.Text, ex.Source, ex.StackTrace));
          }
           
          /// <summary>
          /// Возвращает информацию о ходе выполнения операции одной строкой
          /// </summary>
          /// <returns>Информация</returns>
          public string Log()
          {
               string log = "";

               foreach (string logEntity in m_log)
               {
                    log = string.Format("{0};{1}", log, logEntity);
               }

               return log;
          }

          /// <summary>
          /// Возвращает информацию о ходе выполнения операции в виде списка
          /// </summary>
          /// <returns>Информация</returns>
          public List<string> LogList()
          {
               return m_log;
          }

          /// <summary>
          /// Добавляет результат выполнения другой операции
          /// </summary>
          /// <param name="over">Результат выполнения другой операции</param>
          public void Add(FunctionResult over)
          {
               ResultOperation = ResultOperation & over.ResultOperation;
               
               foreach (string logEntity in over.m_log)
               {
                    m_log.Add(logEntity);
               }
          }
     }
}
