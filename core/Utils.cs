using NLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
using System.Collections.Specialized;

namespace eu.advapay.core.hub
{
    public static class Utils
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public class NotAuthorizedException : Exception
        {
            public NotAuthorizedException(string message) : base(message) { }
        }

        internal class AssertFailedException : Exception
        {
            public AssertFailedException(string message) : base(message)
            {
            }
        }
        public static void Assert(bool statement, string explanation)
        {
            if (!statement) throw new AssertFailedException(explanation);
        }





        public static Dictionary<string, string> ParseJsonToOneLevelDictionary(string json_str)
        {


            // log.Debug(string.Format("*** Parsing JSON: {0}", json_str));


            var jsonObj = JObject.Parse(json_str);
            var response = new Dictionary<string, string>();

            // parsing JSON and converting to Dictionary<string, string> 
            foreach (var child_entry in jsonObj)
            {
                response.Add(child_entry.Key, child_entry.Value.ToString());
            }
            return response;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Encode(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }
        public static byte[] Base64Decode(string base64EncodedData)
        {
            return System.Convert.FromBase64String(base64EncodedData);
        }
        public static string Base64DecodeStr(string base64EncodedData)
        {
            return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(base64EncodedData));
        }

        public static string ConvertExceptionToString(Exception exception)
        {

            PropertyInfo[] properties = exception.GetType().GetProperties();
            string Message = "";
            string StackTrace = "";
            List<string> fields = new List<string>();
            string PropertyToString(PropertyInfo property)
            {
                return $"{property.Name} = {property.GetValue(exception, null) ?? String.Empty}";
            }

            foreach (PropertyInfo p in properties)
            {
                if ("Message".Equals(p.Name))
                {
                    Message = PropertyToString(p);
                }
                else if ("StackTrace".Equals(p.Name))
                {
                    StackTrace = PropertyToString(p);
                }
                else
                {
                    fields.Add(PropertyToString(p));
                }
            }
            return $"{Message}\n----------\n{StackTrace}\n----------\n{string.Join("\n", fields)}";
        }

        private static readonly Dictionary<string, string> EnvironmentVariablesOverrides;

        static Utils()
        {
            // reading env variables override file from HOME folder.
            string ENV_VARIABLES_OVERRIDE_FILE = Environment.GetEnvironmentVariable("ENV_VARIABLES_OVERRIDES_FILE");
            if (ENV_VARIABLES_OVERRIDE_FILE != null)
            {
                string homeFolderPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                            $"{Environment.GetEnvironmentVariable("HOMEDRIVE")}{Environment.GetEnvironmentVariable("HOMEPATH")}" :
                            $"{Environment.GetEnvironmentVariable("HOME")}";
                string fileName = ENV_VARIABLES_OVERRIDE_FILE.Replace("${HOME}", homeFolderPath);

                Utils.Assert(File.Exists(fileName), $"[Fatal] File with Env Variables overrides [{fileName}] does not exists");

                EnvironmentVariablesOverrides = Utils.ParseJsonToOneLevelDictionary(File.ReadAllText(fileName));

                var warningNotice = $" WARNING: ENV VARIABLES OVERRIDES FILE [{fileName}] USED ";

                log.Info(warningNotice);
            }
            else
            {
                EnvironmentVariablesOverrides = new Dictionary<string, string>();
            }
        }


        public static string ReadDictionaryValue(Dictionary<string, string> d, string key, string dictionaryBusinessName)
        {
            Utils.Assert(d.ContainsKey(key), $"Fatal: Dictionary [{dictionaryBusinessName}] does not contain key [{key}]. Which is expected by application");
            return d[key];
        }
        public static long ReadDictionaryValueLong(Dictionary<string, string> d, string key, string dictionaryBusinessName)
        {
            var strVal = ReadDictionaryValue(d, key, dictionaryBusinessName);
            Utils.Assert(long.TryParse(strVal, out long r), $"Fatal: Dictionary Value path=[{dictionaryBusinessName} / {key}] value=[{strVal}] is cannot be converted to LONG"); 
            return r;
        }
        public static int ReadDictionaryValueInt(Dictionary<string, string> d, string key, string dictionaryBusinessName)
        {
            var strVal = ReadDictionaryValue(d, key, dictionaryBusinessName);
            Utils.Assert(int.TryParse(strVal, out int r), $"Fatal: Dictionary Value path=[{dictionaryBusinessName} / {key}] value=[{strVal}] is cannot be converted to INT");
            return r;
        }
        /// Reads env variables with names starting with given string
        public static Dictionary<string, string> ListEnvVariablesWithNamesStartingWith(string startStr)
        {
            var ans = new Dictionary<string, string>();
            foreach (DictionaryEntry r in Environment.GetEnvironmentVariables())
            {
                if (((string) r.Key).StartsWith(startStr))
                {
                    ans.Add((string)r.Key, (string)r.Value);
                }
            }
            return ans
                    .Concat(EnvironmentVariablesOverrides
                    .Where(pair => pair.Key.StartsWith(startStr)))
                    .ToDictionary(kvp => kvp.Key,
                                  kvp => kvp.Value);
            
        }

        public static string ReadEnvVariable(string name)
        {
            var v = ReadOptionalEnvVariable(name, null);
            Utils.Assert(v != null, $"Fatal: {name} environment variable is empty.");
            return v;
        }
        public static string ReadOptionalEnvVariable(string name, string defaultValue = null)
        {
            if (!EnvironmentVariablesOverrides.TryGetValue(name, out string response))
            {
                response = Environment.GetEnvironmentVariable(name) ?? defaultValue;
            }
            return response;
        }

        public static int ReadEnvVariableIntValue(string name)
        {
            Utils.Assert(int.TryParse(Utils.ReadEnvVariable(name), out int answ), $"Fatal: {name} environment variable is not an integer.");
            return answ;
        }

        public static int GetOptionalIntegerEnvVariable(string name, int defaultValue)
        {
            string v = Environment.GetEnvironmentVariable(name);
            if (v == null)
            {
                return defaultValue;
            }
            if (int.TryParse(v, out int r))
            {
                return r;
            }
            else
            {
                throw new Exception($"Fatal: Env Variable [{name}] is supposed to containt Integer value. Value supplied [{v}] cannot be converted to Integer");
            }
        }

        public static string GetLineBegining(string input, int length = 1000)
        {
            return (input.Length > length) ? input.Substring(0, length) + " [string trimmed]" : input;
        }

        public static string jsonValue(JObject json, string name, Boolean required)
        {
            if (json.ContainsKey(name))
            {
                return json[name].ToString();
            }
            else
            {
                if (required) throw new ArgumentException(name + " is missing");
                return "";
            }
        }

        public static void SetValueByPath(JToken token, string path, object value)
        {
            var newToken = value == null ? null : JToken.FromObject(value);
            var targetToken = token.SelectToken(path);
            if (targetToken == null)
            {
                AddTokenByPath(token, path, value);
                return;
            }
            //if (targetToken.Type == JTokenType.Property)
            targetToken.Replace(newToken);
        }
        public static void AddTokenByPath(JToken jToken, string path, object value)
        {
            void SetToken(JToken node, string pathPart, JToken jToken)
            {
                if (node.Type == JTokenType.Object)
                {
                    //get real prop name (convert "['prop']" to "prop")
                    var name = pathPart.Trim('[', ']', '\'');
                    ((JObject)node).Add(name, jToken);
                }
                else if (node.Type == JTokenType.Array)
                {
                    //get real index (convert "[0]" to 0)
                    var index = int.Parse(pathPart.Trim('[', ']'));
                    var jArray = (JArray)node;
                    //if index is bigger than array length, fill the array
                    while (index >= jArray.Count)
                        jArray.Add(null);
                    //set token
                    jArray[index] = jToken;
                }
            }

            // Regex.Split("a.b.d[1]['my1.2.4'][4].af['micor.a.ee.f'].ra[6]", @"(?=\[)|(?=\[\.)|(?<=])(?>\.)")
            // > { "a.b.d", "[1]", "['my1.2.4']", "[4]", "af", "['micor.a.ee.f']", "ra", "[6]" }
            //string[] pathParts = Regex.Split(path, @"(?=\[)|(?=\[\.)|(?<=])(?>\.)");
            string[] pathParts = Regex.Split(path, @"\.");
            JToken node = jToken;
            for (int i = 0; i < pathParts.Length; i++)
            {
                var pathPart = pathParts[i];
                var partNode = node.SelectToken(pathPart);
                //node is null or token with null value
                if (partNode == null || partNode.Type == JTokenType.Null)
                {
                    if (i < pathParts.Length - 1)
                    {
                        //the next level is array or object
                        //accept [0], not ['prop']
                        JToken nextToken;
                        if (Regex.IsMatch(pathParts[i + 1], @"\[\d+\]")) { nextToken = new JArray(); }
                        else { nextToken = new JObject(); }

                        SetToken(node, pathPart, nextToken);
                    }
                    else if (i == pathParts.Length - 1)
                    {
                        //JToken.FromObject(null) will throw a exception
                        var jValue = value == null ?
                            null : JToken.FromObject(value);
                        SetToken(node, pathPart, jValue);
                    }
                    partNode = node.SelectToken(pathPart);
                }
                node = partNode;
            }
        }

        public static void AddRequestParam(NameValueCollection req, string paramName, JObject source, string srcName, bool required, bool addIfEmpty)
        {
            string value = Utils.jsonValue(source, srcName, required);
            if (!"".Equals(value) || addIfEmpty) req.Add(paramName, value);
        }

        internal static SqlInt32 ConvertStringToSqlInt32(string v)
        {
            return new SqlInt32(int.Parse(v));
        }
    }
}

