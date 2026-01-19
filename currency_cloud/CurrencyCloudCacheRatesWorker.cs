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
    public partial class CurrencyCloudCacheRatesWorker : BaseWorker
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
            log.Info("hideContacts = "+ cl.hideContacts);
            log.Info("dontCreateUpdateAccount = " + cl.dontCreateUpdateAccount);
            log.Info("makeTransfersToHouseAccount = " + cl.makeTransfersToHouseAccount);
            log.Info("deductTransferFeeToHouseAccount = " + cl.deductTransferFeeToHouseAccount);

            EnsureQueue("tasks", "currency-cloud", "currency-cloud");
            EnsureQueue("tasks", "rate-markup", "rate-markup");
            EnsureQueue("tasks", "rr_log", "rr_log");
            EnsureQueue("tasks", "results", "results");

            while(true)
            {
                cl.CacheAllRates(new JObject());
                Thread.Sleep(new TimeSpan(0, 0, 29));
            }
        }
    }
}
