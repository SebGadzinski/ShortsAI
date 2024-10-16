using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDatabaseUtility.Models
{
    public class BaseSqlTableQuery
    {
        public string TableName { get; set; } = string.Empty;
        public List<string> Properties { get; set; } = new List<string>();
        public string PropertyPrefix { get; set; } = string.Empty;

        public BaseSqlTableQuery(string tableName, List<string> properties, string propertyPrefix)
        {
            TableName = tableName;
            Properties = properties;
            PropertyPrefix = propertyPrefix;
        }
    }
    public class SecondarySqlTableQuery : BaseSqlTableQuery
    {
        public string JoinOnStatement { get; set; } = string.Empty;
        public SecondarySqlTableQuery(string tableName, List<string> properties, string propertyPrefix, string joinOnStatement) : base(tableName, properties, propertyPrefix)
        {
            JoinOnStatement = joinOnStatement;
        }
    }
}
