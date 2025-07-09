using AutoCode.Presentation.Enum;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCode.Presentation
{
    public static class SettingHelper
    {
        public static SqlConnectionStringBuilder SqlConnectionStringBuilder = new SqlConnectionStringBuilder();

        public static NpgsqlConnectionStringBuilder PostgreSqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder();

        public static ConnectionType ConnectionType = new ConnectionType();

        public static Dictionary<string, Tuple<string, bool, bool>> TableColumnList = new Dictionary<string, Tuple<string, bool, bool>>();
        public static string primaryKeyOfTable = string.Empty;
        public static string tableName = string.Empty;


        public static Dictionary<string, Tuple<string, bool, bool>> Temp_TableColumnList = new Dictionary<string, Tuple<string, bool, bool>>();
        public static string Temp_primaryKeyOfTable = string.Empty;
        public static string Temp_tableName = string.Empty;

    }
}
