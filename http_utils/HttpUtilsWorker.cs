using System;
using RabbitMQ.Client;
using System.Text;
using NLog;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace eu.advapay.core.hub.http_utils
{
    public partial class HttpUtilsWorker : BaseWorker
    {
        public override int Run(string[] args)
        {
            MessageQueue.ThreadSafe = true;

            EnsureQueue("tasks", "http-utils", "http-utils");
            EnsureQueue("tasks", "results", "results");

            channel.BasicQos(0, 1, false);


            CreateTaskProcessors(typeof(HttpTaskProcessor));

            SetupConsumerCycle();

            channel.BasicConsume(queue: "http-utils",
                                    autoAck: false,
                                    consumer: consumer);

            MessageQueue.Send();

            return 0;
        }

    }

    public class HttpTaskProcessor : TaskProcessor
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

                log.Info($"{num} processing request={MbTaskParams} bytes={ea.Body.Length}");

                var httpUtilsParams = Utils.ParseJsonToOneLevelDictionary(MbTaskParams);
                string HttpUtilsRequestType = httpUtilsParams["HttpUtilsRequestType"];

                string responseCode = "";
                byte[] responseData_bytes = null;
                string responseHeader = "";
                Int32.TryParse(httpUtilsParams["bodyIsString"], out int bodyIsString);

                switch (HttpUtilsRequestType)
                {
                    case "httpGetRequest":
                        HandleGetRequest(httpUtilsParams, out responseCode, out responseData_bytes);
                        break;
                    case "httpPostRequest":
                        HttpPostRequest(httpUtilsParams, out responseCode, out responseData_bytes);
                        break;
                    case "httpAnyRequest":
                        HttpAnyRequest(httpUtilsParams, out responseCode, out responseData_bytes, out responseHeader);
                        break;
                    default:
                        Utils.Assert(false, string.Format("Fatal: Unsupported HttpUtils request Type={0}", HttpUtilsRequestType));
                        break;
                }

                {
                    Msg msg = new Msg();
                    msg.Exchange = "tasks";
                    msg.RoutingKey = "results";
                    msg.Headers["MbTaskID"] = MbTaskID;
                    msg.Headers["MbTaskExecutionErrorCode"] = 0;
                    msg.Headers["MbResponseValue"] = responseCode;
                    msg.Headers["Worker"] = GetType().Name;
                    msg.Headers["ResponseHeader"] = responseHeader;
                    msg.Headers["BodyIsString"] = bodyIsString;
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
                msg.Headers["ResponseHeader"] = "";
                msg.Body = null;
                MessageQueue.Add(msg);
            }
        }

        /*
         *  Utility Methods. Those read params from Json and pass to HTTPUtils
         */

        private void HttpAnyRequest(Dictionary<string, string> httpUtilsRequestParams, out string responseCode, out byte[] responseData_bytes, out string responseHeader)
        {
            string optionsXML, headersXML, bodyData;

            optionsXML = httpUtilsRequestParams["optionsXML"];
            headersXML = httpUtilsRequestParams["headersXML"];
            bodyData = httpUtilsRequestParams["bodyData"];

            HTTPUtils.HttpAnyRequest(optionsXML, headersXML, bodyData, out responseCode, out string responseData, out byte[] responseData_binary, out string responseHead);
            responseData_bytes = responseData_binary ?? Encoding.UTF8.GetBytes(responseData);
            responseHeader = responseHead;
        }

        private static void HttpPostRequest(Dictionary<string, string> httpUtilsRequestParams, out string responseCode, out byte[] responseData_bytes)
        {
            string URL, postData, param;

            URL = httpUtilsRequestParams["URL"];
            postData = httpUtilsRequestParams["postData"];
            param = httpUtilsRequestParams["param"];


            responseCode = "OK"; // TODO - take from call 
            string response = HTTPUtils.HttpPostRequest(URL, postData, param);
            responseData_bytes = Encoding.UTF8.GetBytes(response);
        }

        private static void HandleGetRequest(Dictionary<string, string> httpUtilsRequestParams, out string responseCode, out byte[] responseData_bytes)
        {
            string URL, param;

            URL = httpUtilsRequestParams["URL"];
            param = httpUtilsRequestParams["param"];

            responseCode = "OK"; // TODO - take from call 
            string response = HTTPUtils.HttpGetRequest(URL, param);
            responseData_bytes = Encoding.UTF8.GetBytes(response);
        }
    }
}
