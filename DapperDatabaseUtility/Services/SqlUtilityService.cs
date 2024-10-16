using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using DapperDatabaseUtility.Models;

namespace DapperDatabaseUtility.Services
{
    public interface ISqlUtilityService
    {
        DataTable ConvertToDataTable<T>(IEnumerable<T> data, HashSet<string>? propertiesToUse = null);
        string SQLValue(object value);
        string GetAllSelectValues<T>(string prefix = "");
        string GetTableName<T>();
        public List<string> GetAllProperties<T>();
        string GetSQLType(Type type);
        string CreateTempTableSQLExecutable<T>(string tableName = "", HashSet<string>? propertiesToUse = null);
        string UpdateTempTableSQLExecutable<T>(string tableName = "", HashSet<string>? propertiesToUpdate = null);
        string DeleteTempTableSQLExecutable<T>(string tableName = "");
        string ConvertSqlTableInfoToString(BaseSqlTableQuery baseTableInfo, List<SecondarySqlTableQuery> secondaryTablesInfo);
    }
    public class SqlUtilityService : ISqlUtilityService
    {
        public DataTable ConvertToDataTable<T>(IEnumerable<T> data, HashSet<string>? propertiesToUse = null)
        {
            var propertiesToUpdateCheckList = propertiesToUse != null ? propertiesToUse.ToHashSet() : null;
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var dataTable = new DataTable();
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor prop = properties[i];
                if (i == 0 || propertiesToUse == null)
                {
                    dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
                else if (i > 0 && propertiesToUpdateCheckList != null)
                {
                    if (propertiesToUpdateCheckList.Contains(prop.Name))
                    {
                        propertiesToUpdateCheckList.Remove(properties[i].Name);
                        dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }
                    else continue;
                }
            }

            if (propertiesToUpdateCheckList != null && propertiesToUpdateCheckList.Count > 0) throw new Exception($"Properties not found: {JsonConvert.SerializeObject(propertiesToUpdateCheckList)}");

            foreach (var item in data)
            {
                var row = dataTable.NewRow();
                for (int i = 0; i < properties.Count; i++)
                {
                    PropertyDescriptor prop = properties[i];
                    if (i == 0 || propertiesToUse == null || (propertiesToUse != null && propertiesToUse.Contains(prop.Name)))
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
        public string GetAllSelectValues<T>(string prefix = "")
        {
            prefix += !prefix.Equals("") ? "." : "";
            var selectString = "";
            var properties = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in properties)
                selectString += $"{prefix}[{prop.Name}],";
            return selectString.TrimEnd(',') + " ";
        }

        public List<string> GetAllProperties<T>()
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var propertiesList = new List<string>();
            foreach (PropertyDescriptor prop in properties)
                propertiesList.Add(prop.Name);
            return propertiesList;
        }

        public string GetTableName<T>()
        {
            return typeof(T).Name;
        }

        public string SQLValue(object value)
        {
            if (value == DBNull.Value) return "NULL";
            else if (value.GetType() == typeof(Guid)) return $"'{(Guid)value}'";
            else if (value.GetType() == typeof(string)) return $"'{((string)value).Replace("'", "''")}'";
            else if (value.GetType() == typeof(DateTime)) return $"'{value}'";
            else if (value.GetType() == typeof(bool)) return (bool)value ? "1" : "0";
            else return $"{value}";
        }
        public string GetSQLType(Type type)
        {
            if (type == null) throw new Exception("SQL type cannot be null");
            else if (type == typeof(string)) return "varchar(MAX)";
            else if (type == typeof(Guid)) return "UNIQUEIDENTIFIER";
            else if (type == typeof(DateTime)) return "datetime";
            else if (type == typeof(int)) return "int";
            else if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return "float";
            else if (type == typeof(bool)) return "bit";
            else throw new Exception("SQL type cannot be found for: " + JsonConvert.SerializeObject(type));
        }
        public string CreateTempTableSQLExecutable<T>(string tableName = "", HashSet<string>? propertiesToUse = null)
        {
            var propertiesToUpdateCheckList = propertiesToUse != null ? propertiesToUse.ToHashSet() : null;
            tableName = tableName.Equals("") ? typeof(T).Name : tableName;
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var sqlExecutableString = $"CREATE TABLE #Temp{tableName}(";
            for (int i = 0; i < properties.Count; i++)
            {
                if (i > 0 && propertiesToUpdateCheckList != null)
                {
                    if (propertiesToUpdateCheckList.Contains(properties[i].Name)) propertiesToUpdateCheckList.Remove(properties[i].Name);
                    else continue;
                }

                sqlExecutableString += $"{properties[i].Name} {GetSQLType(Nullable.GetUnderlyingType(properties[i].PropertyType) ?? properties[i].PropertyType)},";
            }
            if (propertiesToUpdateCheckList != null && propertiesToUpdateCheckList.Count > 0) throw new Exception($"Properties not found: {JsonConvert.SerializeObject(propertiesToUpdateCheckList)}");

            return sqlExecutableString.TrimEnd(',') + ")";
        }
        public string UpdateTempTableSQLExecutable<T>(string tableName = "", HashSet<string>? propertiesToUpdate = null)
        {
            var propertiesToUpdateCheckList = propertiesToUpdate != null ? propertiesToUpdate.ToHashSet() : null;
            tableName = tableName.Equals("") ? typeof(T).Name : tableName;
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var sqlExecutableString = $"UPDATE T SET ";
            for (int i = 1; i < properties.Count; i++)
            {
                if (propertiesToUpdateCheckList != null)
                {
                    if (propertiesToUpdateCheckList.Contains(properties[i].Name)) propertiesToUpdateCheckList.Remove(properties[i].Name);
                    else continue;
                }

                sqlExecutableString += $"{properties[i].Name} = temp.{properties[i].Name},";
            }
            //If there was a unique set of properties to update, check if there are any that did not get updated, and if so
            if (propertiesToUpdateCheckList != null && propertiesToUpdateCheckList.Count > 0) throw new Exception($"Properties not found: {JsonConvert.SerializeObject(propertiesToUpdateCheckList)}");
            sqlExecutableString = sqlExecutableString.TrimEnd(',');
            return sqlExecutableString += $" FROM dbo.[{tableName}] T INNER JOIN #Temp{tableName} temp ON T.{properties[0].Name} = temp.{properties[0].Name}; DROP TABLE #Temp{tableName}";
        }
        public string DeleteTempTableSQLExecutable<T>(string tableName = "")
        {
            tableName = tableName.Equals("") ? typeof(T).Name : tableName;
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var sqlExecutableString = $"DELETE [{tableName}] FROM {tableName} T Inner Join (SELECT {GetAllSelectValues<T>()} FROM #Temp{tableName}) temp on T.{properties[0].Name} = temp.{properties[0].Name}";
            return sqlExecutableString;
        }
        public string ConvertSqlTableInfoToString(BaseSqlTableQuery baseTableInfo, List<SecondarySqlTableQuery> secondaryTablesInfo)
        {
            var selectStatement = $"Select ";
            var tableJoiningStatement = $"From [{baseTableInfo.TableName}]";
            var namesList = new HashSet<string>();
            namesList.Add(baseTableInfo.PropertyPrefix);

            foreach (var table in secondaryTablesInfo)
            {
                if (!namesList.Add(table.PropertyPrefix)) throw new Exception($"Make sure that all prefixes coming from the SqlTablesQuery's are unique. Error with {table.PropertyPrefix}");
                foreach (var property in table.Properties)
                {
                    selectStatement += table.PropertyPrefix + "." + property + ",";
                    tableJoiningStatement += table.JoinOnStatement + " ";
                }
            }
            return selectStatement.TrimEnd(',') + tableJoiningStatement;
        }
    }
}
