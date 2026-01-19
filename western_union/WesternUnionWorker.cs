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
using System.Collections.Concurrent;
using System.Threading;

namespace eu.advapay.core.hub.western_union
{
    public partial class WesternUnionWorker : BaseWorker
    {
        public int useStateOrProvince = 1;
        public WUParams wuParams;

        public override int Run(string[] args)
        {
            MessageQueue.ThreadSafe = true;

            string wuChannel = Environment.GetEnvironmentVariable("WU_CHANNEL");
            string wuUrl = Environment.GetEnvironmentVariable("WU_URL");
            string customerID = Environment.GetEnvironmentVariable("CUSTOMER_ID");
            string certPath = Environment.GetEnvironmentVariable("CERT");
            string certPassword = Environment.GetEnvironmentVariable("PASSWORD");
            string currencyPath = Environment.GetEnvironmentVariable("CURRENCY_HANBOOK");
            string achCountryCurrencyPath = Environment.GetEnvironmentVariable("ACH_COUNTRYCURRENCY");
            string wuErrors = Environment.GetEnvironmentVariable("WU_ERRORS");
            int.TryParse(Environment.GetEnvironmentVariable("USESTATEORPROVINCE"), out useStateOrProvince);

            if ("".Equals("" + wuChannel)) wuChannel = "western-union";


            log.Debug($"wuChannel='" + wuChannel + "'");
            log.Debug($"wuUrl='" + wuUrl + "'");
            log.Debug($"customerID='" + customerID + "'");
            //log.Debug($"certPath='" + certPath + "'");
            //log.Debug($"certPassword='" + certPassword + "'");
            //log.Debug($"currencyPath='" + currencyPath + "'");
            //log.Debug($"achCountryCurrencyPath='" + achCountryCurrencyPath + "'");
            //log.Debug($"wuErrors='" + wuErrors + "'");


            CurrencyMinorUnit.Init(currencyPath);
            ACHCountryCurrency.Init(achCountryCurrencyPath);

            ReqRespLog.Init(channel);

            wuParams = new WUParams(wuUrl, customerID, certPath, certPassword, wuErrors);

            EnsureQueue("tasks", wuChannel, wuChannel);
            EnsureQueue("tasks", "rate-markup", "rate-markup");
            EnsureQueue("tasks", "rr_log", "rr_log");
            EnsureQueue("tasks", "results", "results");

            channel.BasicQos(0, 1, false);


            CreateTaskProcessors(typeof(WUTaskProcessor));

            SetupConsumerCycle();

            channel.BasicConsume(queue: wuChannel,
                                    autoAck: false,
                                    consumer: consumer);

            MessageQueue.Send();

            return 0;
        }

        public class WUTaskProcessor : TaskProcessor
        {
            WesternUnionGate wuGate;
            WUParams wuParams;

            public override void Init(BaseWorker worker)
            {
                WesternUnionWorker wrk = (WesternUnionWorker)worker;
                wuParams = wrk.wuParams;
                wuGate = new WesternUnionGate(wuParams._url, wuParams._certPath, wuParams._password);
            }

            public override void Process(Msg ea)
            {
                long MbTaskID = 0;
                try
                {
                    //MemoryStream bodyStream = new MemoryStream(ea.Body.ToArray(), false);

                    string MbTaskParams = "";

                    if (ea.Headers.ContainsKey("MbTaskID"))
                        long.TryParse("" + ea.Headers["MbTaskID"], out MbTaskID);

                    if (ea.Headers.ContainsKey("MbTaskParams"))
                        MbTaskParams = Encoding.UTF8.GetString((byte[])ea.Headers["MbTaskParams"]);

                    MappedDiagnosticsLogicalContext.Set("task_id", "T#" + MbTaskID);

                    log.Info($"WUWorker processing request: {MbTaskParams}");


                    JObject taskParamsJson = JObject.Parse(MbTaskParams);

                    string encodingName = taskParamsJson.ContainsKey("X-Macrobank-Body-Encoding") ? taskParamsJson["X-Macrobank-Body-Encoding"].ToString() : "";

                    Encoding enc = encodingName.Equals("") ? Encoding.UTF8 : Encoding.GetEncoding(encodingName);

                    if (taskParamsJson.ContainsKey("X-Macrobank-Action") && !taskParamsJson.ContainsKey("action"))
                        taskParamsJson.Add("action", taskParamsJson["X-Macrobank-Action"].ToString());

                    string body = enc.GetString(ea.Body);

                    JObject bodyJson = null;
                    try
                    {
                        bodyJson = JObject.Parse(body);
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "can't parse request as JSON: " + body);
                    }

                    WesternUnion wu = new WesternUnion(wuGate, wuParams._customerID, wuParams._errorPath);

                    Task<(int, JArray, Stream)> a = wu.WUFunctionAsync(taskParamsJson, bodyJson, body);
                    (int code, JArray instructions, Stream resultStream) = a.Result;

                    MemoryStream resultMS = new MemoryStream();
                    if (resultStream != null) resultStream.CopyTo(resultMS);

                    byte[] responseData_bytes = Encoding.UTF8.GetBytes(instructions.ToString());

                    foreach (JObject instr in instructions)
                    {
                        //log.Debug("Process instr =" + instr.ToString());
                        Msg msg = new Msg();
                        msg.Exchange = "tasks";
                        msg.Headers["MbHookListenerCode"] = "integrations";
                        msg.Headers["MbResponseValue"] = instr["hook"]?.ToString();
                        msg.Headers["MbTaskID"] = MbTaskID;
                        msg.Headers["Worker"] = GetType().Name;
                        msg.Body = Encoding.UTF8.GetBytes(instr.ToString());

                        if ("integrations".Equals(instr["module"]?.ToString()))
                        {
                            msg.RoutingKey = "results";
                            MessageQueue.Add(msg);
                        }
                        else if ("rate".Equals(instr["module"]?.ToString()))
                        {
                            msg.RoutingKey = "rate-markup";
                            MessageQueue.Add(msg);
                        }
                        // todo implement task postponing
                    }

                    {
                        Msg msg = new Msg();
                        msg.Exchange = "tasks";
                        msg.RoutingKey = "results";
                        msg.Headers["MbTaskID"] = MbTaskID;
                        msg.Headers["MbTaskExecutionErrorCode"] = code;
                        msg.Headers["MbResponseValue"] = "OK";
                        msg.Headers["Worker"] = GetType().Name;
                        msg.Body = resultMS.ToArray();
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



    public class WUParams
    {
        public string _url;
        public string _certPath;
        public string _password;
        public string _customerID;
        public string _errorPath;
        public WUParams(string url, string customerID, string certPath, string password, string errorPath)
        {
            _certPath = certPath;
            _password = password;
            _customerID = customerID;
            _url = url;
            _errorPath = errorPath;
        }
        }
    }
