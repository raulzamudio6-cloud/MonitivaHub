using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;


/*
    Изменено 05.07.2016 для реализации
    http://jira.canopus.ru:8888/browse/CIT-327
 */

namespace eu.advapay.core.hub.http_utils
{
    public class HTTPUtils
    {
        private static string encoding;
        private static int timeout;

        // разбираем строку параметров
        private static void ParseParamStr(string strToParse)
        {
            string encodingPat = @"\bencoding\s*=\s*(\S+)";
            string timeoutPat = @"\btimeout\s*=\s*(\d+)";

            encoding = "windows-1251"; // по умолчанию

            Regex r = new Regex(encodingPat, RegexOptions.IgnoreCase);

            foreach (Match match in r.Matches((String)strToParse))
                encoding = match.Groups[1].Value;

            timeout = 0;

            r = new Regex(timeoutPat, RegexOptions.IgnoreCase);
            foreach (Match match in r.Matches((String)strToParse))
                timeout = int.Parse(match.Groups[1].Value) * 1000;

        }

        public static string HttpGetRequest(string URL, string paramStr)
        {
            // чтобы не материлось изза сертификатов
            ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);

            ParseParamStr((String)paramStr);

            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create((String)URL);
            WebReq.Method = "GET";
            if (timeout > 0)
                WebReq.Timeout = timeout;
            try
            {
                using HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
                using StreamReader reader = new StreamReader(WebResp.GetResponseStream(), Encoding.GetEncoding((String)encoding));
                return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string HttpPostRequest(string URL, string postData, string paramStr)
        {
            // чтобы не материлось изза сертификатов
            ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | (SecurityProtocolType)768 | (SecurityProtocolType)3072;

            ParseParamStr((String)paramStr);

            byte[] postByteArray = Encoding.GetEncoding((String)encoding).GetBytes(Convert.ToString(postData));

            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create((String)URL);
            WebReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            WebReq.Method = "POST";
            if (timeout > 0)
                WebReq.Timeout = timeout;
            WebReq.ContentType = "application/x-www-form-urlencoded";
            WebReq.ContentLength = postByteArray.Length;

            Stream dataStream = WebReq.GetRequestStream();
            dataStream.Write(postByteArray, 0, postByteArray.Length);
            dataStream.Close();

            try
            {
                using HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
                using StreamReader reader = new StreamReader(WebResp.GetResponseStream(), Encoding.GetEncoding((String)encoding));
                return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string Urlencode(string convert)
        {
            return Uri.EscapeUriString(convert.ToString());
        }

        public static void HttpAnyRequest(string optionsXML, string headersXML, string bodyData, out string responseCode, out string responseData, out byte[] responseData_binary, out string responseHeader)
        {

            responseData_binary = null;

            // чтобы не материлось из-за сертификатов
            ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | (SecurityProtocolType)768 | (SecurityProtocolType)3072;

            // реквест создаем в любом случае

            responseCode = "";
            responseData = "";
            responseHeader = "";

            string requestType, url;
            string requestEncoding, responseEncoding, contentType;
            int requestTimeout;

            // значения параметров по умолчанию
            contentType = "application/x-www-form-urlencoded";
            requestEncoding = "windows-1251";
            responseEncoding = requestEncoding;
            requestTimeout = 10000;
            requestType = "";
            url = "";

            if (optionsXML == null)
            {
                responseCode = "Request options not found.";
                responseData = "";
                responseHeader = "";
                return;
            }

            string certificateName = null;
            string certificateStore = null;

            // парсим значения опций запроса
            try
            {
                XmlDocument xOptions = new XmlDocument();
                xOptions.LoadXml((String)optionsXML);
                foreach (XmlNode xmlNode in xOptions.FirstChild.ChildNodes)
                {
                    switch (xmlNode.Name)
                    {
                        case "url":
                            url = xmlNode.InnerText;
                            break;

                        case "method":
                            requestType = xmlNode.InnerText;
                            break;

                        case "content_type":
                            contentType = xmlNode.InnerText;
                            break;

                        case "request_encoding":
                            requestEncoding = xmlNode.InnerText;
                            break;

                        case "response_encoding":
                            responseEncoding = xmlNode.InnerText;
                            break;

                        case "timeout":
                            requestTimeout = Convert.ToInt32(xmlNode.InnerText);
                            break;
                        //запрос должен быть создан с помощью сертификата
                        case "certificate_store":
                            certificateStore = xmlNode.InnerText;
                            break;

                        //запрос должен быть создан с помощью сертификата
                        case "certificate_name":
                            certificateName = xmlNode.InnerText;
                            break;
                    }
                }
            }
            // фатальная ошибка разбора XML
            catch (Exception e)
            {
                responseCode = (string)e.Message;
                responseData = "";
                responseHeader = "";
                return;
            }

            //создаем реквест
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
            // Добавляем сертификат если надо
            if (certificateName != null)
            {
                try
                {
                    X509Store store;
                    if ((certificateStore != null) && (certificateStore.Trim().Length > 0))
                    {
                        store = new X509Store(certificateStore, StoreLocation.LocalMachine);
                    }
                    else
                    {
                        store = new X509Store(StoreLocation.LocalMachine);
                    }
                    store.Open(OpenFlags.ReadOnly);

                    X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, certificateName, false);
                    if (certificates.Count <= 0)
                    {
                        responseCode = "Certificate not found.";
                        responseData = "";
                        responseHeader = "";
                        store.Close();
                        return;
                    }
                    X509Certificate2 certificate = certificates[0];
                    store.Close();
                    WebReq.ClientCertificates.Add(certificate);
                }
                catch (Exception e)
                {
                    responseCode = (string)e.Message;
                    responseData = "";
                    responseHeader = "";
                    return;
                }
            }

            // обрабатываем значения хидеров (если они есть)
            if ((headersXML != null) && (headersXML.ToString().Trim().Length > 0))
            {
                try
                {
                    XmlDocument xHeaders = new XmlDocument();
                    xHeaders.LoadXml((String)headersXML);
                    foreach (XmlNode xmlNode in xHeaders.FirstChild.ChildNodes)
                    {
                        if (xmlNode.Name == "Accept")
                        {
                            WebReq.Accept = xmlNode.InnerText;
                        }
                        else
                            WebReq.Headers.Add(xmlNode.Name, xmlNode.InnerText);
                    }
                }
                // фатальная ошибка разбора XML
                catch (Exception e)
                {
                    responseCode = (string)e.Message;
                    responseData = "";
                    responseHeader = "";
                    return;
                }
            }

            byte[] postByteArray = null;

            // проставляем общие опции для всех типов запросов
            WebReq.Timeout = requestTimeout;
            WebReq.Method = requestType;

            // Здесь будет выбор действий в зависимости от типа реквеста
            switch (requestType)
            {
                case "GET":
                    try
                    {
                        HttpWebResponse resp = (HttpWebResponse)WebReq.GetResponse();

                        responseCode = resp.StatusCode.ToString();

                        Stream responseStream = resp.GetResponseStream();
                        var memoryStream = new MemoryStream();
                        responseStream.CopyTo(memoryStream);
                        responseData_binary = memoryStream.ToArray();

                        StreamReader reader = new StreamReader(memoryStream);
                        responseData = (string)reader.ReadToEnd();
                        responseHeader = HeadersToJsonString(resp.Headers);
                    }
                    // код выполнения не 200
                    catch (WebException we)
                    {
                        responseCode = (string)we.Message;
                        responseData = "";
                        if (we.Response != null)
                            using (StreamReader rd = new StreamReader(we.Response.GetResponseStream(), Encoding.UTF8))
                                responseData = rd.ReadToEnd();
                                responseHeader = HeadersToJsonString(we.Response.Headers);
                        return;
                    }
                    // фатальная ошибка
                    catch (Exception e)
                    {
                        responseCode = (string)e.Message;
                        responseData = "";
                        responseHeader = "";
                        return;
                    }

                    break;

                case "DELETE":
                    try
                    {
                        {
                            HttpWebResponse resp = (HttpWebResponse)WebReq.GetResponse();
                            StreamReader reader = new StreamReader(resp.GetResponseStream());
                            responseCode = resp.StatusCode.ToString();
                            responseData = (string)reader.ReadToEnd();
                        }
                    }
                    // код выполнения не 200
                    catch (WebException we)
                    {
                        responseCode = (string)we.Message;
                        responseData = (string)new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                    }
                    // фатальная ошибка
                    catch (Exception e)
                    {
                        responseCode = (string)e.Message;
                        responseData = "";
                        responseHeader = "";
                        return;
                    }

                    break;

                default: // POST PUT PATCH - обрабатываем одинаково
                    if (bodyData != null)
                    {
                        postByteArray = Encoding.GetEncoding(requestEncoding).GetBytes((String)bodyData);
                        WebReq.ContentLength = postByteArray.Length;
                    }
                    else
                    {
                        WebReq.ContentLength = 0;
                    }

                    WebReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    WebReq.ContentType = contentType;

                    if (WebReq.ContentLength > 0)
                    {
                        using Stream dataStream = WebReq.GetRequestStream();
                        dataStream.Write(postByteArray, 0, postByteArray.Length);
                        dataStream.Close();
                    }

                    try
                    {
                        {
                            HttpWebResponse resp = (HttpWebResponse)WebReq.GetResponse();
                            StreamReader reader = new StreamReader(resp.GetResponseStream());
                            responseCode = resp.StatusCode.ToString();
                            responseData = (string)reader.ReadToEnd();
                        }
                    }
                    // код выполнения не 200
                    catch (WebException we)
                    {
                        responseCode = (string)we.Message;
                        if (we.Response != null)
                        {
                            responseData = (string)new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                        }
                    }
                    // фатальная ошибка
                    catch (Exception e)
                    {
                        responseCode = (string)e.Message;
                        responseData = "";
                        responseHeader = "";
                        return;
                    }

                    break;
            }
        }
        public static Dictionary<string, string> WebHeaderCollectionToDictionary(WebHeaderCollection whc)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (string header in whc)
            {
                headers.Add(header, whc[header]);
            }
            return headers;
        }

        public static string DictionaryToJsonString(Dictionary<string, string> headers)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(headers);
        }

        public static string HeadersToJsonString(WebHeaderCollection whc)
        {
            return DictionaryToJsonString(WebHeaderCollectionToDictionary(whc));
        }

    }
}