using System;
using RabbitMQ.Client;
using System.Text;
using NLog;
using System.Reflection;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using eu.advapay.core.hub.rr_log;
using System.Threading;

namespace eu.advapay.core.hub.currency_cloud
{
    public partial class CurrencyCloudWorker : BaseWorker
    {
        public override int Run(string[] args)
        {
            string wuUrl = Environment.GetEnvironmentVariable("CURRENCY_CLOUD_URL");
            string loginId = Environment.GetEnvironmentVariable("LOGIN_ID");
            string apiKey = Environment.GetEnvironmentVariable("API_KEY");
            string redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
            string redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");

            if ("".Equals("" + redisPort)) redisPort = "6379";

            ReqRespLog.Init(channel);

            CurrencyCloud cl = new CurrencyCloud(wuUrl, loginId, apiKey, redisHost, redisPort);
            cl.hideContacts = "true".Equals(Environment.GetEnvironmentVariable("HIDE_CONTACTS"));
            cl.dontCreateUpdateAccount = "true".Equals(Environment.GetEnvironmentVariable("DO_NOT_CREATE_UPDATE_ACCOUNT"));
            cl.makeTransfersToHouseAccount = "true".Equals(Environment.GetEnvironmentVariable("MAKE_TRANSFERS_TO_HOUSE_ACCOUNT"));
            cl.deductTransferFeeToHouseAccount = "true".Equals(Environment.GetEnvironmentVariable("DEDUCT_TRANSFER_FEE_TO_HOUSE_ACCOUNT"));
            log.Info("hideContacts = " + cl.hideContacts);
            log.Info("dontCreateUpdateAccount = " + cl.dontCreateUpdateAccount);
            log.Info("makeTransfersToHouseAccount = " + cl.makeTransfersToHouseAccount);
            log.Info("deductTransferFeeToHouseAccount = " + cl.deductTransferFeeToHouseAccount);

            EnsureQueue("tasks", "currency-cloud", "currency-cloud");
            EnsureQueue("tasks", "rate-markup", "rate-markup");
            EnsureQueue("tasks", "rr_log", "rr_log");
            EnsureQueue("tasks", "results", "results");

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                IBasicProperties props = channel.CreateBasicProperties();
                props.Headers = new Dictionary<string, object>();
                props.Headers["Worker"] = GetType().Name;

                try
                {

                    MemoryStream bodyStream = new MemoryStream(ea.Body.ToArray(), false);


                    long MbTaskID = 0;
                    string MbTaskParams = "";

                    if (ea.BasicProperties.Headers.ContainsKey("MbTaskID"))
                        long.TryParse(""+ea.BasicProperties.Headers["MbTaskID"], out MbTaskID);

                    if (ea.BasicProperties.Headers.ContainsKey("MbTaskParams"))
                        MbTaskParams = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["MbTaskParams"]);

                    MappedDiagnosticsLogicalContext.Set("task_id", "T#" + MbTaskID);
                    props.Headers["MbTaskID"] = MbTaskID;

                    log.Info($"processing request: {MbTaskParams}");


                    JObject taskParamsJson = JObject.Parse(MbTaskParams);

                    string encodingName = taskParamsJson.ContainsKey("X-Macrobank-Body-Encoding") ? taskParamsJson["X-Macrobank-Body-Encoding"].ToString() : "";

                    Encoding enc = encodingName.Equals("") ? Encoding.UTF8 : Encoding.GetEncoding(encodingName);

                    if (taskParamsJson.ContainsKey("X-Macrobank-Action") && !taskParamsJson.ContainsKey("action"))
                        taskParamsJson.Add("action", taskParamsJson["X-Macrobank-Action"].ToString());

                    string body = new StreamReader(bodyStream, enc).ReadToEnd();

                    JObject bodyJson = null;
                    try
                    {
                        bodyJson = JObject.Parse(body);
                    }
                    catch (Exception e)
                    {
                        log.Warn(e, "can't parse request as JSON");
                    }


                    Task<(int, JArray, Stream)> a = cl.CLFunctionAsync(taskParamsJson, bodyJson, body);
                    (int code, JArray instructions, Stream resultStream) = a.Result;

                    MemoryStream resultMS = new MemoryStream();
                    if (resultStream != null) resultStream.CopyTo(resultMS);

                    byte[] responseData_bytes = Encoding.UTF8.GetBytes(instructions.ToString());


                    foreach (JObject instr in instructions)
                    {
                        if ("integrations".Equals(instr["module"]?.ToString())) {

                            props.Headers["MbHookListenerCode"] = "integrations";
                            props.Headers["MbResponseValue"] = instr["hook"]?.ToString();

                            channel.BasicPublish(exchange: "tasks",
                                                    routingKey: "results",
                                                    basicProperties: props,
                                                    body: Encoding.UTF8.GetBytes(instr.ToString()));

                        }
                        else if ("rate".Equals(instr["module"]?.ToString()))
                        {
                            if ("error".Equals((string)instr["hook"]))
                            {
                                instr.Remove("module");
                                instr.Remove("hook");
                                channel.BasicPublish(exchange: "tasks",
                                                     routingKey: "rate-api",
                                                     basicProperties: props,
                                                     body: Encoding.UTF8.GetBytes(instr.ToString()));
                            }
                            else
                            {
                                channel.BasicPublish(exchange: "tasks",
                                                     routingKey: "rate-markup",
                                                     basicProperties: props,
                                                     body: Encoding.UTF8.GetBytes(instr.ToString()));
                            }

                        }
                        // todo implement task postponing
                    }


                    props.Headers.Remove("MbHookListenerCode");
                    props.Headers.Remove("hook");
                    props.Headers["MbTaskExecutionErrorCode"] = code;
                    props.Headers["MbResponseValue"] = "OK";

                    channel.BasicPublish(exchange: "tasks",
                                            routingKey: "results",
                                            basicProperties: props,
                                            body: resultMS.ToArray());

                    MappedDiagnosticsLogicalContext.Set("task_id", null);
                }
                catch(Exception e)
                {
                    log.Error(e, "error");

                    props.Headers["MbTaskExecutionErrorCode"] = 2;
                    props.Headers["MbResponseValue"] = Encoding.UTF8.GetBytes(Utils.ConvertExceptionToString(e));

                    string txt = Utils.ConvertExceptionToString(e);
                    channel.BasicPublish(exchange: "tasks",
                                            routingKey: "results",
                                            basicProperties: props,
                                            body: null);
                }
            };

            channel.BasicConsume(queue: "currency-cloud",
                                    autoAck: true,
                                    consumer: consumer);

            Thread.Sleep(new TimeSpan(2, 0, 0));

            return 0;
        }
    }
}
