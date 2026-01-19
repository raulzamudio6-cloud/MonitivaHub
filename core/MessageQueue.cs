using System;
using RabbitMQ.Client;
using System.Data.SqlClient;
using NLog;
using System.Security;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace eu.advapay.core.hub
{
    public class Msg
    {
        public string Exchange;
        public string RoutingKey;
        public Dictionary<string, object> Headers = new Dictionary<string, object>();
        public byte[] Body;
    }

    public class MessageQueue
    {
        private static BlockingCollection<Msg> msgsToSend = new BlockingCollection<Msg>(10);

        public static bool ThreadSafe = false;
        public static IModel channel;

        public static void Add(Msg msg)
        {
            if (ThreadSafe) { msgsToSend.Add(msg); }
            else {
                IBasicProperties props = channel.CreateBasicProperties();
                props.Headers = new Dictionary<string, object>(msg.Headers);

                channel.BasicPublish(exchange: msg.Exchange,
                                        routingKey: msg.RoutingKey,
                                        basicProperties: props,
                                        body: msg.Body);
            }
        }

        public static void Send()
        {
            while (!msgsToSend.IsCompleted)
            {
                Msg msg = null;
                try
                {
                    msg = msgsToSend.Take();
                }
                catch (InvalidOperationException) { }

                if (msg != null)
                {
                    IBasicProperties props = channel.CreateBasicProperties();
                    props.Headers = new Dictionary<string, object>(msg.Headers);

                    channel.BasicPublish(exchange: msg.Exchange,
                                            routingKey: msg.RoutingKey,
                                            basicProperties: props,
                                            body: msg.Body);
                }
            }


        }

        public static bool TrySend()
        {
            Msg msg;
            
            bool res = msgsToSend.TryTake(out msg, 100);
            
            if(res)
            {
                IBasicProperties props = channel.CreateBasicProperties();
                props.Headers = new Dictionary<string,object>(msg.Headers);

                channel.BasicPublish(exchange: msg.Exchange,
                                        routingKey: msg.RoutingKey,
                                        basicProperties: props,
                                        body: msg.Body);

            }

            return res;
        }

    }
}
