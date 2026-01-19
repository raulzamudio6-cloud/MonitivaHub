using Newtonsoft.Json.Linq;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eu.advapay.core.hub.rates
{
    class RatesMarkupWorker : BaseWorker
    {
        public static Dictionary<string, ExchangeProfits> profits = new Dictionary<string, ExchangeProfits>();
        protected SqlCommand saveQuoteCmd;
        protected SqlCommand getMarkupCmd;

        public RatesMarkupWorker() : base(true)
        {
        }

        public override int Run(string[] args)
        {
            EnsureQueue("tasks", "rate-markup", "rate-markup");
            EnsureQueue("tasks", "rate-api", "rate-api");

            channel.BasicQos(0, 1, false);

            saveQuoteCmd = new SqlCommand("integrations.sp_SaveRateQuote", conn) { CommandType = CommandType.StoredProcedure };
            saveQuoteCmd.Parameters.Add("@rateRef", SqlDbType.VarChar, -1);
            saveQuoteCmd.Parameters.Add("@quoteId", SqlDbType.VarChar, -1);
            saveQuoteCmd.Parameters.Add("@createdOn", SqlDbType.DateTime, -1);
            saveQuoteCmd.Parameters.Add("@expirationIntervalInSec", SqlDbType.Int, -1);
            saveQuoteCmd.Parameters.Add("@sellCurrency", SqlDbType.VarChar, -1);
            saveQuoteCmd.Parameters.Add("@sellAmount", SqlDbType.Money, -1);
            saveQuoteCmd.Parameters.Add("@buyCurrency", SqlDbType.VarChar, -1);
            saveQuoteCmd.Parameters.Add("@buyAmount", SqlDbType.Money, -1);
            saveQuoteCmd.Parameters.Add("@rate", SqlDbType.Float, -1);
            saveQuoteCmd.Parameters.Add("@isRateInverted", SqlDbType.Bit, -1);
            saveQuoteCmd.Parameters.Add("@markup", SqlDbType.VarChar, -1);
            saveQuoteCmd.Parameters.Add("@baserate", SqlDbType.Float, -1);

            getMarkupCmd = new SqlCommand("integrations.sp_GetClientExchangeMarkup", conn) { CommandType = CommandType.StoredProcedure };
            getMarkupCmd.Parameters.Add("@SessionID", SqlDbType.VarChar, -1).Direction = ParameterDirection.Input;
            getMarkupCmd.Parameters.Add("@MarkupTable", SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;
            getMarkupCmd.CommandTimeout = 0;


            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                IBasicProperties props = channel.CreateBasicProperties();
                props.Headers = new Dictionary<string, object>();
                props.Headers["Worker"] = GetType().Name;
                props.Headers.Add("MbTaskExecutionErrorCode", 0);
                props.Headers.Add("MbResponseValue", "OK");

                try
                {

                    var MbTaskAttachmentsBytes = ea.Body.ToArray();
                    var MbTaskAttachments = Encoding.UTF8.GetString(MbTaskAttachmentsBytes);

                     long MbTaskID = 0;
                    string MbTaskParams = "";

                    if (ea.BasicProperties.Headers.ContainsKey("MbTaskID"))
                        Int64.TryParse("" + ea.BasicProperties.Headers["MbTaskID"], out MbTaskID);

                    if (ea.BasicProperties.Headers.ContainsKey("MbTaskParams"))
                        MbTaskParams = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["MbTaskParams"]);

                    string body = Encoding.UTF8.GetString(ea.Body.ToArray());

                    MappedDiagnosticsLogicalContext.Set("task_id", "T#" + MbTaskID);
                    props.Headers["MbTaskID"] = MbTaskID;

                    log.Info($"processing request={MbTaskParams} bytes={ea.Body.Length}");



                    JObject req = null;
                    try
                    {
                        req = JObject.Parse(body);
                        log.Debug($"Parse quoteId='" + req["quoteId"].ToString() + ", rate =" + req["rate"].ToString() + "'");
                    }
                    catch (Exception e)
                    {
                        log.Debug(e, "quote request is not JSON");
                        channel.BasicAck(ea.DeliveryTag, false);
                        return; 
                    }

                    log.Debug("req = " + req.ToString());

                    if ("clearCache".Equals((string)req["action"]))
                    {
                        profits.Clear();
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    if (req.SelectToken("original-request") == null)
                    {
                        log.Debug("no original-request");
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }



                    double rate = (double)req["rate"];
                    if (!(bool)req["isDirectRate"]) rate = 1 / rate;

                    bool isSellAmount = (bool)req["original-request"]["isSellAmount"];

                    double amount = Double.Parse(req["original-request"]["amount"].ToString());


                    string sessionId = req["original-request"]["sessionId"].ToString();
                    if (!profits.ContainsKey(sessionId)) profits[sessionId] = ExchangeProfits.loadForSession(sessionId,getMarkupCmd);

                    log.Debug($"before amount='" + amount.ToString() + ", rate =" + rate.ToString() + "'");
                    Task<(double, double)> rateTask = profits[sessionId].getRate((string)req["sellCurrency"], (string)req["buyCurrency"], amount, isSellAmount, rate);
                    (double actiualRate, double markupPercent) = await rateTask;
                    actiualRate = Math.Round(actiualRate, 6);
                    log.Debug($"after amount='" + amount.ToString() + ", rate =" + rate.ToString() + ", actiualRate =" + actiualRate.ToString() + ", markupPercent =" + markupPercent.ToString() + "'");
                    req["markupPercent"] = ""+markupPercent;

                    int roundDigits = 2;
                    if (isSellAmount)
                    {
                        req["sellAmount"] = amount;
                        if ("JPY".Equals(((string)req["buyCurrency"]).ToUpper())) roundDigits = 0;
                        req["buyAmount"] = Math.Round(amount * actiualRate, roundDigits);
                    }
                    else
                    {
                        if ("JPY".Equals(((string)req["sellCurrency"]).ToUpper())) roundDigits = 0;
                        req["sellAmount"] = Math.Round(amount / actiualRate, roundDigits);
                        req["buyAmount"] = amount;
                    }

                    log.Debug($"after sellAmount='" + req["sellAmount"].ToString() + ", buyAmount =" + req["buyAmount"].ToString() + ", actiualRate =" + actiualRate.ToString() + ", rate =" + req["rate"].ToString() + "'");
                    bool isRateInverted;
                    //if (actiualRate < 1)
                    //{
                    //    isRateInverted = true;
                    //    actiualRate = 1 / actiualRate;
                    //}
                    //else
                    {
                        isRateInverted = false;
                    }

                    req["rate"] = actiualRate;
                    //req["baserate"]
                    rate = Math.Round(rate, 6); 
                    req["isRateInverted"] = isRateInverted;

                    req["rateRef"] = "RS" + Guid.NewGuid();



                    log.Debug($"quoteId='" + req["quoteId"].ToString() + ", rate =" + req["rate"].ToString() + "'");
                    saveQuoteCmd.Parameters["@rateRef"].Value = req["rateRef"];
                    saveQuoteCmd.Parameters["@quoteId"].Value = req["quoteId"];
                    saveQuoteCmd.Parameters["@createdOn"].Value = (DateTime)req["createdOn"];
                    saveQuoteCmd.Parameters["@expirationIntervalInSec"].Value = req["expirationIntervalInSec"];
                    saveQuoteCmd.Parameters["@sellCurrency"].Value = req["sellCurrency"];
                    saveQuoteCmd.Parameters["@sellAmount"].Value = req["sellAmount"];
                    saveQuoteCmd.Parameters["@buyCurrency"].Value = req["buyCurrency"];
                    saveQuoteCmd.Parameters["@buyAmount"].Value = req["buyAmount"];
                    log.Debug($"before execute rate'" + rate.ToString() + ", actiualRate =" + actiualRate.ToString() + "'");
                    saveQuoteCmd.Parameters["@rate"].Value = actiualRate;
                    saveQuoteCmd.Parameters["@isRateInverted"].Value = req["isRateInverted"];
                    saveQuoteCmd.Parameters["@markup"].Value = req["markupPercent"];
                    saveQuoteCmd.Parameters["@baserate"].Value = rate;
                    saveQuoteCmd.CommandTimeout = 0;
                    saveQuoteCmd.ExecuteNonQuery();


                    req.Remove("module");
                    req.Remove("integration");
                    req.Remove("quoteId");
                    req.Remove("isDirectRate");
                    //req.Remove("original-request");


                    channel.BasicPublish(exchange: "tasks",
                                            routingKey: "rate-api",
                                            basicProperties: props,
                                            body: Encoding.UTF8.GetBytes(req.ToString()));

                    MappedDiagnosticsLogicalContext.Set("task_id", null);
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

            channel.BasicConsume(queue: "rate-markup",
                                    autoAck: false,
                                    consumer: consumer);

            while (true)
                Thread.Sleep(new TimeSpan(2, 0, 0));

            return 0;
        }
    }

    public class ExchangeProfit
    {
        public string SessionID;
        public string sellCurrency;
        public string buyCurrency;
        public List<double> fromAmount = new List<double>();
        public List<double> fixes = new List<double>();
        public List<double> percents = new List<double>();


        public async Task<(double,double)> getRate(double amount, bool isSellAmount, double baseRate)
        {
            double fix = 0;
            double percent = 0;
            bool profitFound = false;

            for (int i = fromAmount.Count - 1; i >= 0; i--)
            {
                if (amount >= fromAmount[i])
                {
                    fix = fixes[i];
                    percent = percents[i];
                    profitFound = true;
                    break;
                }
            }

            if (!profitFound) return (baseRate, percent);

            return ((baseRate - fix) * (1 - percent / 100f), percent);
        }

    }

    public class ExchangeProfits
    {
        Dictionary<string, ExchangeProfit> profits = new Dictionary<string, ExchangeProfit>();
        public static ExchangeProfits loadForSession(string SessionID, SqlCommand getMarkupCmd)
        {
            ExchangeProfits profits = new ExchangeProfits();
            ILogger Log = LogManager.GetLogger("ExchangeProfit");

            try
            {
                getMarkupCmd.Parameters["@SessionID"].Value = SessionID;
                getMarkupCmd.ExecuteNonQuery();
                string MarkupTable = getMarkupCmd.Parameters["@MarkupTable"].Value.ToString();

                JArray markups = JArray.Parse(MarkupTable);
                foreach (JObject markup in markups)
                {
                    ExchangeProfit profit = new ExchangeProfit();
                    profit.SessionID = SessionID;
                    profit.sellCurrency = markup["SellCurrency"].ToString();
                    profit.buyCurrency = markup["BuyCurrency"].ToString();

                    profit.fromAmount.Add(0);
                    profit.fixes.Add(0);
                    profit.percents.Add((double)markup["ProfitValue"]);

                    for (int i = 2; i <= 7; i++)
                    {
                        if (!markup.ContainsKey("FromAmount" + i)) continue;
                        if ((double)markup["FromAmount" + i] > 0)
                        {
                            profit.fromAmount.Add((double)markup["FromAmount" + i]);
                            profit.fixes.Add(0);
                            profit.percents.Add((double)markup["ProfitValue" + i]);
                        }
                    }
                    profits.Add(profit);
                }
            }
            catch (SqlException e)
            {
                Log.Error(e, $"Error loading exchange profit at line:" + e.LineNumber + " proc:" + e.Procedure);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error loading exchange profit");
            }

            return profits;
        }


        public void Add(ExchangeProfit profit)
        {
            profits[profit.sellCurrency + "_" + profit.buyCurrency] = profit;

        }

        public async Task<(double,double)> getRate(string sellCurrency, string buyCurrency, double amount, bool isSellAmount, double baseRate)
        {
            string key = sellCurrency + "_" + buyCurrency;
            if (profits.ContainsKey(key)) return await profits[key].getRate(amount, isSellAmount, baseRate);

            key = sellCurrency + "_*";
            if (profits.ContainsKey(key)) return await profits[key].getRate(amount, isSellAmount, baseRate);

            key = "*_" + buyCurrency;
            if (profits.ContainsKey(key)) return await profits[key].getRate(amount, isSellAmount, baseRate);

            key = "*_*";
            if (profits.ContainsKey(key)) return await profits[key].getRate(amount, isSellAmount, baseRate);

            return (baseRate, 0.0);
        }

    }
}
