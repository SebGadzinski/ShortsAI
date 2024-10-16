using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DapperDatabaseUtility.DataAccess
{
    public interface IDapperSqlConnectionFactory
    {
        SqlConnection GetSqlConnection(string dbName);
    }
}
