
using eu.advapay.core.hub.http_utils;
using eu.advapay.core.hub.rates;
using eu.advapay.core.hub.rr_log;
using eu.advapay.core.hub.security;
using eu.advapay.core.hub.western_union;
using eu.advapay.core.hub.currency_cloud;
using eu.advapay.core.hub.dias_instant;
using System;


namespace eu.advapay.core.hub 
{
    class Program
    {
        static int Main(string[] args)
        {
            string service_name = Environment.GetEnvironmentVariable("SERVICE_NAME");

            if (service_name == null) service_name = args.Length > 0 ? args[0] : ""; ;
            if (service_name.Length == 0) service_name = "TaskPublisher";

            Console.WriteLine("service_name=" + service_name);
            Console.WriteLine("BUILD=" + CiInfo.BuildTag);  // if it does not compile run build_csharp.cmd/sh to create CiInfo.cs

            if (service_name.Equals("TaskPublisher")) return new TaskPublisher().Main(args);
            else if (service_name.Equals("HttpUtilsWorker")) return new HttpUtilsWorker().Main(args);
            else if (service_name.Equals("TaskResultSaver")) return new TaskResultSaver().Main(args);
            else if (service_name.Equals("WesternUnionWorker")) return new WesternUnionWorker().Main(args);
            else if (service_name.Equals("CurrencyCloudWorker")) return new CurrencyCloudWorker().Main(args);
            else if (service_name.Equals("CurrencyCloudCacheRatesWorker")) return new CurrencyCloudCacheRatesWorker().Main(args);
            else if (service_name.Equals("SecurityWorker")) return new SecurityWorker().Main(args);
            else if (service_name.Equals("ExecProcWorker")) return new ExecProcWorker().Main(args);
            else if (service_name.Equals("RatesMarkupWorker")) return new RatesMarkupWorker().Main(args);
            else if (service_name.Equals("ReqRespLogWorker")) return new ReqRespLogWorker().Main(args);
            else if (service_name.Equals("DiasInstantWorker")) return new DiasInstantWorker().Main(args);

            return 1;
        }
    }
}
