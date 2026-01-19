using NLog;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace eu.advapay.core.hub.rr_log
{
    public class ReqRespLog
    {
        private static IModel channel;

        public static void Init(IModel channel)
        {
            ReqRespLog.channel = channel;
        }

        public static void Save(string sys, bool isReq, string ReqResp)
        {
            int doc;
            int.TryParse(MappedDiagnosticsLogicalContext.Get("doc"), out doc);
            Save(sys, DateTime.Now, MappedDiagnosticsLogicalContext.Get("req_id"), doc,
                MappedDiagnosticsLogicalContext.Get("req_type"), isReq, ReqResp);
        }

        public static void Save(string sys, DateTime theDate, string req_id, int doc, string req_type, bool isReq, string ReqResp)
        {
            IBasicProperties props = channel.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>();
            props.Headers["sys"] = sys;
            props.Headers["the_date"] = theDate.ToString("yyyyMMddHHmmss");
            props.Headers["req_id"] = req_id;
            props.Headers["doc"] = doc;
            props.Headers["req_type"] = req_type;
            props.Headers["is_req"] = isReq ? 1 : 0;


            channel.BasicPublish(exchange: "tasks",
                                    routingKey: "rr_log",
                                    basicProperties: props,
                                    body: Encoding.UTF8.GetBytes(ReqResp));

        }
    }
}
