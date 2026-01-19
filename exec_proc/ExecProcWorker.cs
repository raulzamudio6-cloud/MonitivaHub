using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace eu.advapay.core.hub
{
    class ExecProcWorker : BaseWorker
    {
        public override int Run(string[] args)
        {
            EnsureQueue("tasks", "exec-proc", "exec-proc");
            EnsureQueue("tasks", "results", "results");

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                IBasicProperties props = channel.CreateBasicProperties();
                props.Headers = new Dictionary<string, object>();

                long MbTaskID = 0;
                string MbTaskParams = "";

                if (ea.BasicProperties.Headers.ContainsKey("MbTaskID"))
                    long.TryParse("" + ea.BasicProperties.Headers["MbTaskID"], out MbTaskID);

                if (ea.BasicProperties.Headers.ContainsKey("MbTaskParams"))
                    MbTaskParams = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["MbTaskParams"]);

                MappedDiagnosticsLogicalContext.Set("task_id", "T#" + MbTaskID);

                log.Info($"redirecting proc={MbTaskParams} bytes={ea.Body.Length}");

                ea.BasicProperties.Headers["MbHookListenerCode"] = MbTaskParams;
                ea.BasicProperties.Headers["Worker"] = GetType().Name;

                channel.BasicPublish(exchange: "tasks",
                                        routingKey: "results",
                                        basicProperties: ea.BasicProperties,
                                        body: ea.Body.ToArray());

                MappedDiagnosticsLogicalContext.Set("task_id", null);
            };

            channel.BasicConsume(queue: "exec-proc",
                                    autoAck: true,
                                    consumer: consumer);
            
            while (true) 
                Thread.Sleep(new TimeSpan(2, 0, 0)); 
            
            return 0;
        }

    }
}
