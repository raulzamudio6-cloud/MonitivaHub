using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace eu.advapay.core.hub.western_union
{
    internal class ACHCountryCurrency
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, string> _paramsByName;
        private readonly string _filePath;
        private static ACHCountryCurrency instance;

        public static void Init(string filePath)
        {
            instance = new ACHCountryCurrency(filePath);
        }

        public static ACHCountryCurrency getInstance()
        {
            return instance;
        }

        private ACHCountryCurrency(string filePath)
        {
            _filePath = filePath;
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"No file with country-currency data can be found: {_filePath}");

            _paramsByName = GetParamsDictionary();
        }

        private Dictionary<string, string> GetParamsDictionary()
        {
            var paramsByName = new Dictionary<string, string>();
            using (TextFieldParser parser = new TextFieldParser(_filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters( new string[] { ",", ";"} );
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    //log.Debug($"isACH parser fields='" + fields.ToString() + "'");
                    string name = fields[0].Trim().ToUpperInvariant();

                    //log.Debug($"isACH parser name='" + name + "'");
                    if (string.IsNullOrEmpty(name) ||
                        (name.Length != 2) ||
                        paramsByName.ContainsKey(name))
                        continue;

                    string cur = fields[1].Trim().ToUpperInvariant();
                    if (string.IsNullOrEmpty(cur) ||
                        (cur.Length != 3) )
                        continue;
                    /*                    if (!int.TryParse(fields[1].Trim(), out int value))
                                        {
                                            const int defaultValue = 0;
                                            value = defaultValue;
                                        }*/

                    paramsByName.Add(name, cur);
                }
            }
            return paramsByName;
        }

        public Boolean isACH(string country, string currency)
        {
            Boolean value = false;
            //log.Debug($"isACH country='" + country + ", _paramsByName=" + _paramsByName.Count.ToString() + "'");

            if (string.IsNullOrEmpty(country))
                throw new ArgumentNullException(nameof(country));

            //if (_paramsByName.ContainsKey(country.ToUpperInvariant()))
            //    log.Debug($"isACH ContainsKey country='" + country.ToUpperInvariant() + "'");

            if (_paramsByName.TryGetValue(country.ToUpperInvariant(), out string cur))
            {
                //log.Debug($"isACH country='" + country + ", cur=" + cur + ", currency=" + currency + "'");
                if (currency.Equals(cur))
                    value = true;
            }
            return value;
        }

    }
}
