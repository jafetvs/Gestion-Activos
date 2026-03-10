using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    internal static class SqlUtils
    {
        public static string GetString(IDataRecord r, string name)
            => r[name] == DBNull.Value ? null : Convert.ToString(r[name]);

        public static int GetInt(IDataRecord r, string name)
            => r[name] == DBNull.Value ? 0 : Convert.ToInt32(r[name]);

        public static int? GetNullableInt(IDataRecord r, string name)
            => r[name] == DBNull.Value ? (int?)null : Convert.ToInt32(r[name]);

        public static bool GetBool(IDataRecord r, string name)
            => r[name] != DBNull.Value && Convert.ToBoolean(r[name]);

        public static DateTime GetDate(IDataRecord r, string name)
            => Convert.ToDateTime(r[name]);

        public static DateTime? GetNullableDate(IDataRecord r, string name)
            => r[name] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[name]);

        public static decimal? GetNullableDecimal(IDataRecord r, string name)
            => r[name] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r[name]);

        public static long? GetNullableLong(IDataRecord r, string name)
    => r[name] == DBNull.Value ? (long?)null : Convert.ToInt64(r[name]);
    }
}
