using DapperDatabaseUtility.DataAccess;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace MediaCreatorSite.DataAccess
{
    public class Connections : IDapperSqlConnectionFactory
    {
        private readonly Dictionary<string, string> _connectionStrings;

        public Connections(Dictionary<string, string> connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }

        public SqlConnection GetSqlConnection(string dbName)
        {
            return new SqlConnection(_connectionStrings[dbName]);
        }
    }
}
