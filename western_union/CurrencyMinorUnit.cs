using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;

namespace eu.advapay.core.hub.western_union
{
    internal class CurrencyMinorUnit
    {
        private readonly Dictionary<string, int> _paramsByName;
        private readonly string _filePath;
        private static CurrencyMinorUnit instance;

        public static void Init(string filePath)
        {
            instance = new CurrencyMinorUnit(filePath);
        }

        public static CurrencyMinorUnit getInstance()
        {
            return instance;
        }

        private CurrencyMinorUnit(string filePath)
        {
            _filePath = filePath;
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"No file with currency data can be found: {_filePath}");

            _paramsByName = GetParamsDictionary();
        }

        private Dictionary<string, int> GetParamsDictionary()
        {
            var paramsByName = new Dictionary<string, int>();
            using (TextFieldParser parser = new TextFieldParser(_filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters( new string[] { ",", ";"} );
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    string name = fields[0].Trim().ToUpperInvariant();

                    if (string.IsNullOrEmpty(name) ||
                        (name.Length != 3) ||
                        paramsByName.ContainsKey(name))
                        continue;

                    if (!int.TryParse(fields[1].Trim(), out int value))
                    {
                        const int defaultValue = 0;
                        value = defaultValue;
                    }

                    paramsByName.Add(name, value);
                }
            }
            return paramsByName;
        }

        public int GetMinorUnit(string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode))
                throw new ArgumentNullException(nameof(currencyCode));

            if (!_paramsByName.TryGetValue(currencyCode.ToUpperInvariant(), out int value))
                throw new KeyNotFoundException($"Currency by code {currencyCode} is not found");

            return value;
        }

    }
}
