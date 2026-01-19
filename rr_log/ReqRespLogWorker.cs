using System;
using RabbitMQ.Client;
using System.Text;
using System.Data.SqlClient;
using NLog;
using System.Security;
using System.Net;
using System.Data;
using System.Reflection;
using RabbitMQ.Client.Events;
using System.Threading;
using System.Globalization;

namespace eu.advapay.core.hub.rr_log
{
    class ReqRespLogWorker : BaseWorker
    {
        public ReqRespLogWorker() : base(true)
        {
        }

        public override int Run(string[] args) 
        {
            EnsureQueue("tasks", "rr_log", "rr_log");

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);


            SqlCommand cmd = new SqlCommand("integrations.sp_SaveLog", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@sys", SqlDbType.NVarChar, 32);
            cmd.Parameters.Add("@thedate", SqlDbType.DateTime);
            cmd.Parameters.Add("@req_id", SqlDbType.NVarChar, 32);
            cmd.Parameters.Add("@doc", SqlDbType.Int);
            cmd.Parameters.Add("@req_type", SqlDbType.NVarChar, 32);
            cmd.Parameters.Add("@req", SqlDbType.NVarChar);
            cmd.Parameters.Add("@resp", SqlDbType.NVarChar);
            cmd.CommandTimeout = 0;



            consumer.Received += (model, ea) =>
            {
                try
                {
                    string sys = "";
                    string thedateStr = "";
                    DateTime the_date;
                    string req_id = "";
                    int doc = 0;
                    string req_type = "";
                    int is_req = 0;


                    if (ea.BasicProperties.Headers.ContainsKey("sys"))
                        sys = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["sys"]);

                    if (ea.BasicProperties.Headers.ContainsKey("the_date"))
                        thedateStr = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["the_date"]);
                    the_date = DateTime.ParseExact(thedateStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                    if (ea.BasicProperties.Headers.ContainsKey("req_id"))
                        req_id = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["req_id"]);

                    if (ea.BasicProperties.Headers.ContainsKey("doc"))
                        int.TryParse("" + ea.BasicProperties.Headers["doc"], out doc);

                    if (ea.BasicProperties.Headers.ContainsKey("req_type"))
                        req_type = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["req_type"]);

                    if (ea.BasicProperties.Headers.ContainsKey("is_req"))
                        int.TryParse("" + ea.BasicProperties.Headers["is_req"], out is_req);


                    string BodyString = Encoding.UTF8.GetString(ea.Body.ToArray());

                    if(log.IsDebugEnabled) log.Debug($"processing rr_log: sys={sys} req_id={req_id} req_type={req_type} bytes={ea.Body.Length}");



                    cmd.Parameters["@sys"].Value = sys;
                    cmd.Parameters["@thedate"].Value = the_date;
                    cmd.Parameters["@req_id"].Value = req_id;
                    
                    if (doc > 0) { cmd.Parameters["@doc"].Value = doc; }
                    else { cmd.Parameters["@doc"].Value = DBNull.Value; }

                    cmd.Parameters["@req_type"].Value = req_type;
                    
                    if (is_req == 1) { cmd.Parameters["@req"].Value = BodyString; }
                    else { cmd.Parameters["@req"].Value = DBNull.Value; }

                    if (is_req == 0) { cmd.Parameters["@resp"].Value = BodyString; }
                    else { cmd.Parameters["@resp"].Value = DBNull.Value; }
                    

                    cmd.ExecuteNonQuery();

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception e)
                {
                    log.Error(e, "error");
                    bool noDb = IsDBConnectionBroken(e);
                    channel.BasicReject(ea.DeliveryTag, noDb);
                    if (noDb)
                    {
                        log.Fatal("Database disconnected, exiting.");
                        Environment.Exit(5);
                    }
                }
            };

            channel.BasicConsume(queue: "rr_log",
                                    autoAck: false,
                                    consumer: consumer);

            while (true)
                Thread.Sleep(new TimeSpan(2, 0, 0)); 

            return 0;
        }
    }
}
