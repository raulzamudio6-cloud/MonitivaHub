using System;
using RabbitMQ.Client;
using System.Text;
using NLog;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Threading;

namespace eu.advapay.core.hub.security
{
    public class SecurityWorker : BaseWorker
    {
        public override int Run(string[] args)
        {
            MessageQueue.ThreadSafe = true;
            EnsureQueue("tasks", "security", "security");
            EnsureQueue("tasks", "results", "results");

            channel.BasicQos(0, 1, false);

            CreateTaskProcessors(typeof(SecurityTaskProcessor));

            SetupConsumerCycle();

            channel.BasicConsume(queue: "security",
                                    autoAck: false,
                                    consumer: consumer);

            MessageQueue.Send();

            return 0;
        }

    }

    public class SecurityTaskProcessor : TaskProcessor
    {
        public override void Process(Msg ea)
        {
            long MbTaskID = 0;
            try
            {
                var MbTaskAttachmentsBytes = ea.Body;
                var MbTaskAttachments = Encoding.UTF8.GetString(MbTaskAttachmentsBytes);

                string MbTaskParams = "";

                Int64.TryParse("" + ea.Headers["MbTaskID"], out MbTaskID);
                if (ea.Headers["MbTaskParams"] != null)
                    MbTaskParams = Encoding.UTF8.GetString((byte[])ea.Headers["MbTaskParams"]);

                MappedDiagnosticsLogicalContext.Set("task_id", "T#" + MbTaskID);


                log.Info($"{num} processing request: {MbTaskParams}");


                string ResponseBody = SecurityFunctions.ProcessTaskAndPrepareResponse(MbTaskParams, log);
                byte[] responseData_bytes = Encoding.UTF8.GetBytes(ResponseBody);


                {
                    Msg msg = new Msg();
                    msg.Exchange = "tasks";
                    msg.RoutingKey = "results";
                    msg.Headers["MbTaskID"] = MbTaskID;
                    msg.Headers["MbTaskExecutionErrorCode"] = 0;
                    msg.Headers["MbResponseValue"] = "OK";
                    msg.Headers["BodyIsString"] = 1;
                    msg.Headers["Worker"] = GetType().Name;
                    msg.Body = responseData_bytes;
                    MessageQueue.Add(msg);
                }

                MappedDiagnosticsLogicalContext.Set("task_id", null);
            }
            catch (Exception e)
            {
                log.Error(e, "error");

                Msg msg = new Msg();
                msg.Exchange = "tasks";
                msg.RoutingKey = "results";
                msg.Headers["MbTaskID"] = MbTaskID;
                msg.Headers["MbTaskExecutionErrorCode"] = 2;
                msg.Headers["MbResponseValue"] = Encoding.UTF8.GetBytes(Utils.ConvertExceptionToString(e));
                msg.Headers["Worker"] = GetType().Name;
                msg.Body = null;
                MessageQueue.Add(msg);
            }

        }
    }

}
