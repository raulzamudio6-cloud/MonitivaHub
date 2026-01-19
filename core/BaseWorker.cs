using System;
using RabbitMQ.Client;
using System.Data.SqlClient;
using NLog;
using System.Security;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Diagnostics;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;


namespace eu.advapay.core.hub
{
    public abstract class BaseWorker
    {
        bool needDB;
        protected bool needRabbit = true;

        protected string proc_name;
        protected IConnection connection;
        protected IModel channel;
        protected EventingBasicConsumer consumer;
        protected SqlConnection conn;

        protected BlockingCollection<Msg> msgsToProcess;


        protected ILogger log;

        Regex[] brokenConnectionRegexps = {
            new Regex(@"\bconnection\b.*\bbroken\b.*recovery\s+is\s+not\s+possible"),
            new Regex(@"\bPhysical\s+connection\s+is\s+not\s+usable\b"),
            new Regex(@"\bconnection.*\s+\bcurrent\s+state\s+is\s+closed\b")
        };


        public BaseWorker()
        {
            this.needDB = false;
        }

        public BaseWorker(bool needDB)
        {
            this.needDB = needDB;
        }

        public abstract int Run(string[] args);

        public virtual void OnException(Exception e)
        {

        }

        public int Main(string[] args)
        {
            log = LogManager.GetLogger(GetType().Name);

            proc_name = GetType().Name + "_" + Process.GetCurrentProcess().Id;
            MappedDiagnosticsLogicalContext.Set("proc_name", proc_name);
            Console.WriteLine("proc_name=" + proc_name);

            log.Info("Starting " + proc_name);
            try
            {
                string rabbit_host = Environment.GetEnvironmentVariable("RABBIT_HOST");
                string rabbit_user = Environment.GetEnvironmentVariable("RABBIT_USER");
                string rabbit_password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD");

                DateTime startConnectDT = DateTime.Now;
                while (needRabbit)
                {
                    try
                    {
                        log.Info("Try to connect to  MQ ConnectionFactory");
                        ConnectionFactory factory = new ConnectionFactory() { HostName = rabbit_host, Port = 5672, UserName = rabbit_user, Password = rabbit_password };
                        factory.AutomaticRecoveryEnabled = true;

                        log.Info("Try to connect to  MQ: rabbit_host=" + rabbit_host + ", rabbit_user="+ rabbit_user+ ", rabbit_password="+ rabbit_password);
                        connection = factory.CreateConnection(proc_name);
                        log.Info("Connected to  MQ:" + connection);
                        channel = connection.CreateModel();
                        break;
                    }
                    catch (Exception e)
                    {
                        if (DateTime.Now.Subtract(startConnectDT).CompareTo(TimeSpan.FromMinutes(5)) > 0)
                        {
                            log.Fatal(e,"Attemp to connect to RabbitMQ failed, Exiting");
                            return 3;
                        }
                        else
                        {
                            log.Warn("Attemp to connect to RabbitMQ failed, retrying in 10 seconds. ("+e.Message+")");
                            Thread.Sleep(10000);
                        }
                    }
                }

                MessageQueue.channel = channel;

                if (needDB)
                {
                    var SQLDB_CONNECTION = Utils.ReadEnvVariable("SQLDB_CONNECTION");

                    conn = new SqlConnection(SQLDB_CONNECTION);
                    if (Utils.ReadOptionalEnvVariable("SQLDB_CONNECTION_LOGIN", null) != null)
                    {
                        SecureString passwd = new NetworkCredential("", Utils.ReadEnvVariable("SQLDB_CONNECTION_PASSWORD")).SecurePassword;
                        passwd.MakeReadOnly();
                        SqlCredential cred = new SqlCredential(Utils.ReadEnvVariable("SQLDB_CONNECTION_LOGIN"), passwd);

                        conn.Credential = cred;
                    }

                    conn.Open();
                    SqlCommand SPIDCmd = conn.CreateCommand();
                    SPIDCmd.CommandText = "select @@SPID";
                    object SPID = SPIDCmd.ExecuteScalar();
                    log.Info("Connected to SQL, SPID=" + SPID);
                }

                return Run(args);
                /*int ret = Run(args);

                Thread.Sleep(new TimeSpan(2, 0, 0));

                return ret;*/
            }
            catch (Exception e)
            {
                log.Error(e, "exiting");
                OnException(e);
                return 2;
            }
        }
        public void EnsureQueue(string exchange, string queueName, string routingKey)
        {
            EnsureQueue(exchange, queueName, routingKey, null);
        }

        public void EnsureQueue(string exchange, string queueName, string routingKey, Dictionary<String, Object> Qargs)
        {
            channel.ExchangeDeclare(exchange, ExchangeType.Direct, true, false);

            channel.QueueDeclare(queueName, true, false, false);

            if (Qargs == null) Qargs = new Dictionary<string, object>(1);
            if (!Qargs.ContainsKey("x-overflow")) Qargs.Add("x-overflow", "reject-publish");

            channel.QueueBind(queue: queueName,
                              exchange: exchange,
                              routingKey: routingKey,
                              arguments: Qargs);
        }

        public void SetupConsumerCycle()
        {
            consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                Msg msg = new Msg();
                foreach (KeyValuePair<string, object> kv in ea.BasicProperties.Headers)
                {
                    msg.Headers.Add(kv.Key, kv.Value);
                }
                msg.Body = ea.Body.ToArray();
                msgsToProcess.Add(msg);
                channel.BasicAck(ea.DeliveryTag, false);

            };
        }

        public void CreateTaskProcessors(Type processorClass)
        {
            if (!(processorClass.IsSubclassOf(typeof(TaskProcessor)))) throw new Exception("processorClass is not TaskProcessor");

            string multiTaskFactorStr = Environment.GetEnvironmentVariable("MULTI_TASK_FACTOR");


            long multiTaskFactor = 1;
            Int64.TryParse(multiTaskFactorStr, out multiTaskFactor);
            if (multiTaskFactor < 1) multiTaskFactor = 1;
            if (multiTaskFactor > 50) multiTaskFactor = 50;

            log.Debug($"multiTaskFactor=" + multiTaskFactor);

            msgsToProcess = new BlockingCollection<Msg>((int)multiTaskFactor);


            int taskID = 0;

            for (int i = 0; i < multiTaskFactor; i++)
            {
                var childTask = Task.Factory.StartNew(() =>
                {
                    int myTaskID = Interlocked.Increment(ref taskID);

                    try
                    {
                        var ctors = processorClass.GetConstructors();

                        // invoke the first public constructor with no parameters.
                        TaskProcessor proc = (TaskProcessor)ctors[0].Invoke(new object[] { });

                        proc.SetNum(myTaskID);
                        proc.SetLogger(log);
                        proc.Init(this);

                        log.Info($"Process {myTaskID} started");
                        while (!msgsToProcess.IsCompleted)
                        {
                            Msg msg = null;
                            try
                            {
                                msg = msgsToProcess.Take();
                            }
                            catch (InvalidOperationException) { }

                            if (msg != null)
                            {
                                //log.Trace($"Process {myTaskID} started processing message");
                                proc.Process(msg);
                                //log.Trace($"Process {myTaskID} finished processing message");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "error");
                    }

                    log.Info("Child task finished.");
                },TaskCreationOptions.LongRunning);
                //childTask.Start();
            }
        }

        public bool IsDBConnectionBroken(Exception e)
        {
            foreach (Regex re in brokenConnectionRegexps)
            {
                if (re.Matches(e.Message).Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

    }

    public class CustomData
    {
        public int num;
    }
    public abstract class TaskProcessor
    {
        protected ILogger log;
        protected int num;

        public virtual void Init(BaseWorker worker)
        {

        }
        public void SetLogger(ILogger log)
        {
            this.log = log;
        }
        public void SetNum(int num)
        {
            this.num = num;
        }

        public abstract void Process(Msg msg);
    }
}
