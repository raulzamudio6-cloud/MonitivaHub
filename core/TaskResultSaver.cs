using System;
using RabbitMQ.Client;
using System.Text;
using System.Data.SqlClient;
using NLog;
using System.Data;
using RabbitMQ.Client.Events;
using System.Threading;

namespace eu.advapay.core.hub
{
    class TaskResultSaver : BaseWorker
    {

        public TaskResultSaver() : base(true)
        {
        }
        public override int Run(string[] args) 
        {
            EnsureQueue("tasks", "results", "results");

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            

            SqlCommand cmd = new SqlCommand("integrations.saveTaskResults", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@MbTaskID", SqlDbType.BigInt);
            cmd.Parameters.Add("@MbHookListenerCode", SqlDbType.NVarChar);
            cmd.Parameters.Add("@MbTaskExecutionErrorCode", SqlDbType.Int);
            cmd.Parameters.Add("@MbResponseValue", SqlDbType.NVarChar);
            cmd.Parameters.Add("@MbResponseAttachments", SqlDbType.Binary);
            cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 128);
            cmd.Parameters.Add("@ResponseWriterInvocationId", SqlDbType.NVarChar, 64);
            cmd.Parameters.Add("@ResponseHeader", SqlDbType.NVarChar);
            cmd.CommandTimeout = 0;


            consumer.Received += (model, ea) =>
            {
                long MbTaskID = 0;
                try
                {
                    string Worker = "";
                    int MbTaskExecutionErrorCode = 0;
                    string MbResponseValue = "";
                    string MbHookListenerCode = "";
                    string ResponseHeader = "";
                    int BodyIsString = 1;

                    if (ea.BasicProperties.Headers.ContainsKey("MbTaskID"))
                        long.TryParse("" + ea.BasicProperties.Headers["MbTaskID"], out MbTaskID);

                    if (ea.BasicProperties.Headers.ContainsKey("MbTaskExecutionErrorCode"))
                        int.TryParse("" + ea.BasicProperties.Headers["MbTaskExecutionErrorCode"], out MbTaskExecutionErrorCode);
                    
                    if (ea.BasicProperties.Headers.ContainsKey("MbResponseValue")) 
                        MbResponseValue = Encoding.UTF8.GetString( (byte[])ea.BasicProperties.Headers["MbResponseValue"] );
                    
                    if (ea.BasicProperties.Headers.ContainsKey("MbHookListenerCode"))
                        MbHookListenerCode = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["MbHookListenerCode"]);

                    if (ea.BasicProperties.Headers.ContainsKey("Worker"))
                        Worker = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["Worker"]);
                    else 
                        Worker = "mq.TaskResultSaver";

                    if (ea.BasicProperties.Headers.ContainsKey("BodyIsString"))
                        int.TryParse("" + ea.BasicProperties.Headers["BodyIsString"], out BodyIsString);
                    
                    if (ea.BasicProperties.Headers.ContainsKey("ResponseHeader"))
                        ResponseHeader = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["ResponseHeader"]);

                    byte[] BodyBytes = ea.Body.ToArray();

                    if (BodyIsString == 1 && BodyBytes != null)
                    {
                        string BodyString = Encoding.UTF8.GetString(BodyBytes);
                        BodyBytes = Encoding.Unicode.GetBytes(BodyString);
                    }

                    MappedDiagnosticsLogicalContext.Set("task_id", "T#" + MbTaskID);

                    log.Info($"processing task result: T#{MbTaskID} proc={MbHookListenerCode} code={MbTaskExecutionErrorCode} value={MbResponseValue} bytes={ea.Body.Length}");

                    cmd.Parameters["@MbTaskID"].Value = MbTaskID;
                    cmd.Parameters["@MbHookListenerCode"].Value = MbHookListenerCode;
                    cmd.Parameters["@MbTaskExecutionErrorCode"].Value = MbTaskExecutionErrorCode;
                    cmd.Parameters["@MbResponseValue"].Value = MbResponseValue;
                    cmd.Parameters["@MbResponseAttachments"].Value = BodyBytes;
                    cmd.Parameters["@CreatedBy"].Value = Worker;
                    cmd.Parameters["@ResponseWriterInvocationId"].Value = proc_name;
                    cmd.Parameters["@ResponseHeader"].Value = ResponseHeader;

                    cmd.ExecuteNonQuery();

                    MappedDiagnosticsLogicalContext.Set("task_id", null);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception e)
                {
                    log.Error(e, $"T#{MbTaskID} error");
                    bool noDb = IsDBConnectionBroken(e);
                    channel.BasicReject(ea.DeliveryTag, noDb);
                    if (noDb)
                    {
                        log.Fatal("Database disconnected, exiting.");
                        Environment.Exit(5);
                    }
                }
            };

            channel.BasicConsume(queue: "results",
                                    autoAck: false,
                                    consumer: consumer);


            while (true)
                Thread.Sleep(new TimeSpan(2, 0, 0));

            return 0;
        }
    }
}
