using System;
using RabbitMQ.Client;
using System.Text;
using System.Data.SqlClient;
using NLog;
using System.Security;
using System.Net;
using System.Data;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using RabbitMQ.Client.Exceptions;

namespace eu.advapay.core.hub
{
    public class TaskPublisher : BaseWorker
    {
        HashSet<string> declaredQueues = new HashSet<string>();
        List<long> PublishedIDs = new List<long>(); 
        long CurrentTaskToPublish = 0;
        SqlCommand getCmd;
        SqlCommand updateCmd;
        string tag;

        public TaskPublisher() : base(true)
        {
        }

        public override int Run(string[] args)
        {
            tag = Environment.GetEnvironmentVariable("PUBLISHER_TAG");
            if (tag == null) tag = "";
            log.Info($"Publisher tag={tag}");

            EnsureQueue("tasks", "results", "results");



            getCmd = new SqlCommand("exec integrations.selectNextTaskToBeProcessed @TaskHandlerInvocationId, @Tag", conn);
            getCmd.CommandType = CommandType.Text;
            getCmd.CommandTimeout = 0;
            {
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@TaskHandlerInvocationId";
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 64;
                param.Value = connection.ClientProvidedName;
                getCmd.Parameters.Add(param);
            }
            {
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Tag";
                param.SqlDbType = SqlDbType.VarChar;
                param.Size = 5;
                param.Value = tag;
                getCmd.Parameters.Add(param);
            }

            updateCmd = new SqlCommand("exec integrations.UpdateTasksInProcess @IDs, @Status", conn);
            updateCmd.CommandType = CommandType.Text;
            updateCmd.CommandTimeout = 0;
            {
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@IDs";
                param.SqlDbType = SqlDbType.VarChar;
                param.Value = "";
                updateCmd.Parameters.Add(param);
            }
            {
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Status";
                param.SqlDbType = SqlDbType.Int;
                param.Size = 64;
                param.Value = 1;
                updateCmd.Parameters.Add(param);
            }

            bool hasRows = false;

            int cnt = 0;
            DateTime startDt = DateTime.Now;

            DateTime startedAt = DateTime.Now;
            while (true)
            {
                SqlDataReader reader = getCmd.ExecuteReader();
                try
                {
                    if (!hasRows)
                    {
                        cnt = 0;
                        startDt = DateTime.Now;
                    }
                    hasRows = reader.HasRows;
                    PublishedIDs.Clear();

                    while (reader.Read())
                    {
                        long MbTaskID = reader.GetInt64(reader.GetOrdinal("MbTaskID"));
                        string MbTaskCode = reader.GetString(reader.GetOrdinal("MbTaskCode"));
                        string MbTaskParams = reader.GetString(reader.GetOrdinal("MbTaskParams"));
                        Stream MbTaskAttachments = reader.GetStream(reader.GetOrdinal("MbTaskAttachments"));
                        bool MbAttachmentsIsString = reader.GetBoolean(reader.GetOrdinal("MbAttachmentsIsString"));
                        long ttl = reader.GetInt64(reader.GetOrdinal("TTL"));

                        MappedDiagnosticsLogicalContext.Set("task_id", "T#"+MbTaskID);

                        cnt++;
                        log.Trace($"Publishing task T#{MbTaskID} for execution. TaskCode={MbTaskCode}. TaskParams={MbTaskParams}.");
                        CurrentTaskToPublish = MbTaskID;

                        if (!declaredQueues.Contains(MbTaskCode))
                        {
                            Dictionary<String, Object> Qargs = new Dictionary<String, Object>();
                            Qargs.Add("x-overflow", "reject-publish");
                            EnsureQueue("tasks", MbTaskCode, MbTaskCode, Qargs);
                            declaredQueues.Add(MbTaskCode);
                        }


                        MemoryStream body = new MemoryStream();
                        if(MbTaskAttachments!=null) MbTaskAttachments.CopyTo(body);

                        byte[] responseData_bytes = body.GetBuffer();
                        int responseData_Length = (int)body.Length;

                        if (MbAttachmentsIsString)
                        {
                            responseData_bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8,responseData_bytes, 0, (int)body.Length);
                            responseData_Length = responseData_bytes.Length;
                        }

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.Persistent = true;
                        props.Expiration = ""+ttl;
                        props.Headers = new Dictionary<string, object>();
                        props.Headers.Add("MbTaskID", MbTaskID);
                        props.Headers.Add("MbTaskParams", Encoding.UTF8.GetBytes(MbTaskParams));

                        channel.BasicPublish(exchange: "tasks",
                                                    routingKey: MbTaskCode,
                                                    basicProperties: props,
                                                    body: new ReadOnlyMemory<byte>(responseData_bytes, 0, responseData_Length));

                        PublishedIDs.Add(CurrentTaskToPublish);
                        CurrentTaskToPublish = 0;
                        MappedDiagnosticsLogicalContext.Set("task_id", null);
                    }


                    if (PublishedIDs.Count > 0)
                    {
                        log.Info($"Set task status=1 for {PublishedIDs.Count} tasks");
                        updateCmd.Parameters["@Status"].Value = 1;
                        updateCmd.Parameters["@IDs"].Value = string.Join(",", PublishedIDs.ToArray());
                        updateCmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    reader.Close();
                }
                if (!hasRows)
                {
                    if (cnt > 0) log.Debug($"Published {cnt} tasks in " + DateTime.Now.Subtract(startDt).TotalMilliseconds + "ms");
                    Thread.Sleep(1000);
                }
/*                if(DateTime.Now.Subtract(startedAt)>TimeSpan.FromHours(2))
                {
                    log.Info($"Service exiting after 2 hours of work");
                    return 1;
                }*/
            }
        }
        public override void OnException(Exception e)
        {
            try
            {
                if (CurrentTaskToPublish > 0)
                {
                    int status = 2;
                    if (e is AlreadyClosedException)
                    {
                        status = -1000;
                        log.Info($"Clear MBTasks_in_process for task T#{CurrentTaskToPublish}");
                    }
                    else
                    {
                        log.Info($"Set task status={status} for task T#{CurrentTaskToPublish}");
                    }
                    updateCmd.Parameters["@Status"].Value = status;
                    updateCmd.Parameters["@IDs"].Value = CurrentTaskToPublish;// string.Join(",", PublishedIDs.ToArray());
                    updateCmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                log.Error(e, "update tasks in process");
            }
        }


    }
}

