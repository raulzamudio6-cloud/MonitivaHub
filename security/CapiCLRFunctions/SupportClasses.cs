using System;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiCLRFunctions
{
    public class VarcharConversions
    {
        public static string Bytes2HexStr(byte[] src)
        {
            string res = "";

            foreach (byte b in src)
            {
                res += String.Format("{0:X2}", b);
            }
            return res;
        }

        public static byte[] HexStr2Bytes(string src)
        {
            byte[] res = new byte[src.Length / 2];

            for (int i = 0, b = 0; i < src.Length; i += 2, b++)
            {
                res[b] = Byte.Parse(src.Substring(i, 2), NumberStyles.HexNumber);
            }

            return res;
        }

    }

    public class SettingsDBReader
    {
        public static ProviderSettings Read(SqlConnection conn)
        {
            string name;
            uint type;
            bool exportable;
            uint chipher;
            uint hash;
            uint protect;
            string companyName;
            string keyName;
            byte[] ca = null;

            using (SqlCommand qSettings = new SqlCommand())
            {
                qSettings.Connection = conn;
                qSettings.CommandType = CommandType.Text;
                qSettings.CommandText = "select top 1 * from CAPI.Settings";
                qSettings.Connection.Open();
                using (SqlDataReader reader = qSettings.ExecuteReader())
                {
                    reader.Read();

                    name = reader.GetString(0);         // Name
                    type = (uint)reader.GetInt32(1);    // Type
                    exportable = reader.GetBoolean(2);  // ExportableKey
                    chipher = (uint)reader.GetInt32(3); // ChipherAlg
                    hash = (uint)reader.GetInt32(4);    // HashAlg
                    protect = (uint)reader.GetInt32(5); // Protection
                    companyName = reader.GetString(6);  // CompanyName
                    keyName = reader.GetString(7);      // KeyName
                    // CACertificate
                    if (!reader.IsDBNull(8))
                    {
                        ca = (byte[])reader["CACertificate"];
                    }

                    reader.Close();
                }
                qSettings.Connection.Close();
            }

            return new ProviderSettings(name, type, exportable, chipher, hash, protect, keyName, companyName, ca);
        }
    }
}