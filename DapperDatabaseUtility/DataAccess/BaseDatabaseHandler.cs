using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DapperDatabaseUtility.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperDatabaseUtility.Extensions;

namespace DapperDatabaseUtility.DataAccess
{
    public delegate void VoidFunc<T1>(T1 obj1);
    public delegate void VoidFunc();
    public interface IBaseDatabaseHandler
    {
        /* Connection */

        public SqlConnection Connection();

        /* Code Blocks */

        public Task<T> BlockAsync<T>(string functionName, Func<Task<T>> function);
        public T Block<T>(string functionName, Func<T> function);
        public Task BlockAsync(string functionName, Func<Task> function);
        public void Block(string functionName, VoidFunc function);
        //SQL CONNECTION
        public Task<T> BlockAsync<T>(string functionName, Func<SqlConnection, Task<T>> function);
        public T Block<T>(string functionName, Func<SqlConnection, T> function);
        public Task BlockAsync(string functionName, Func<SqlConnection, Task> function);
        public void Block(string functionName, VoidFunc<SqlConnection> function);

        /* Query */

        public Task<IEnumerable<T>> GetAllItemsInTableAsync<T>(string tableName = "", bool onlyActiveRows = true);
        public IEnumerable<T> GetAllItemsInTable<T>(string tableName = "", bool onlyActiveRows = true);
        public Task<T> GetByIdAsync<T, G>(G id, string tableName = "", bool onlyActiveRows = true);
        public T GetById<T, G>(G id, string tableName = "", bool onlyActiveRows = true);
        public Task<IEnumerable<T>> WhereAsync<T>(string whereStatement, object values, string tableName = "", bool onlyActiveRows = true);
        public Task<IEnumerable<T>> WhereAsync<T>(string whereStatement, string tableName = "", bool onlyActiveRows = true);
        public IEnumerable<T> Where<T>(string whereStatement, string tableName = "", bool onlyActiveRows = true);
        public IEnumerable<T> Where<T>(string whereStatement, object values, string tableName = "", bool onlyActiveRows = true);
        public Task<T> FirstOrDefaultAsync<T>(string whereStatement, string tableName = "", bool onlyActiveRows = true);
        public Task<T> FirstOrDefaultAsync<T>(string whereStatement, object values, string tableName = "", bool onlyActiveRows = true);
        public T FirstOrDefault<T>(string whereStatement, string tableName = "", bool onlyActiveRows = true);
        public T FirstOrDefault<T>(string whereStatement, object values, string tableName = "", bool onlyActiveRows = true);

        /* Command */

        public T Insert<T>(T item, bool insertWithPrimary = false, string tableName = "");
        public Task<T> InsertAsync<T>(T item, bool insertWithPrimary = false, string tableName = "");
        public void Update<T>(T item, bool updateWithPrimary = false, string tableName = "");
        public Task UpdateAsync<T>(T item, bool updateWithPrimary = false, string tableName = "");
        public void UpdateByProperty(string whereStatement, Dictionary<string, object?> propertiesToUpdate, string tableName);
        public void UpdateByProperty(string whereStatement, Dictionary<string, object?> propertiesToUpdate, object values, string tableName);
        public Task UpdateByPropertyAsync(string whereStatement, Dictionary<string, object?> propertiesToUpdate, string tableName);
        public Task UpdateByPropertyAsync(string whereStatement, Dictionary<string, object?> propertiesToUpdate, object values, string tableName);
        public void Delete<T>(T item, string tableName = "");
        public Task DeleteAsync<T>(T item, string tableName = "");
        public void DeleteWhere(string whereStatement, string tableName);
        public void DeleteWhere(string whereStatement, object values, string tableName);
        Task DeleteWhereAsync(string whereStatement, string tableName);
        Task DeleteWhereAsync(string whereStatement, object values, string tableName);

        /* Bulks */
        public void BulkInsert<T>(IEnumerable<T> list, string tableName = "");
        public Task BulkInsertAsync<T>(IEnumerable<T> list, string tableName = "");
        public void BulkUpdate<T>(IEnumerable<T> list, HashSet<string>? propertiesToUpdate = null, string tableName = "");
        public Task BulkUpdateAsync<T>(IEnumerable<T> list, HashSet<string>? propertiesToUpdate = null, string tableName = "");
        public void BulkDelete<T>(IEnumerable<T> list, string tableName = "");
        public Task BulkDeleteAsync<T>(IEnumerable<T> list, string tableName = "");

        /* Temp Table */
        public void WriteToTempTable<T>(SqlConnection command, IEnumerable<T> list, HashSet<string>? tempTableProperties = null, string tableName = "");
        public Task WriteToTempTableAsync<T>(SqlConnection connection, IEnumerable<T> list, HashSet<string>? tempTableProperties = null, string tableName = "");

        //Strings needed for identity:
        public string InsertString<T>(ref string queryString, T item, bool insertWithPrimary = false, string tableName = "");
        public string GetByIdString<T, G>(ref string queryString, G id, string tableName, bool onlyActiveRows = true);
        public string UpdateString<T>(ref string queryString, T item, bool updateWithPrimary = false, string tableName = "");
    }

    public class BaseDatabaseHandler : IBaseDatabaseHandler
    {
        protected readonly IDapperSqlConnectionFactory _connectionFactory;
        protected static readonly ISqlUtilityService _sqlUtilityService = new SqlUtilityService();
        protected readonly ILogger<BaseDatabaseHandler> _logger;
        protected readonly string CONNECTION_STRING_CONTEXT;

        public BaseDatabaseHandler(IDapperSqlConnectionFactory connectionFactory, string connectionStringContext, ILogger<BaseDatabaseHandler> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            this.CONNECTION_STRING_CONTEXT = connectionStringContext;
        }

        /* Connection */

        public SqlConnection Connection()
        {
            return _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT);
        }

        /* Code Blocks */

        #region Code Blocks
        /// <summary>
        /// With a function name, write code in a safe try catch logged environment and returns the object wanted (T)
        /// </summary>
        /// <typeparam name="T">Returning Object</typeparam>
        /// <param name="functionName">Name of the function</param>
        /// <param name="function"> async () => { async code; return T } </param>
        /// <returns></returns>
        public async Task<T> BlockAsync<T>(string functionName, Func<Task<T>> function)
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting {nameof(BlockAsync)}: {functionName}");
                return await function();
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} - {1} - Error: {2}", CONNECTION_STRING_CONTEXT.ToUpperInvariant(), functionName, JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        /// <summary>
        /// With a function name, write code in a safe try catch logged environment and returns the object wanted (T)
        /// </summary>
        /// <typeparam name="T">Returning Object</typeparam>
        /// <param name="functionName">Name of the function</param>
        /// <param name="function"> () => { code; return T } </param>
        /// <returns></returns>
        public T Block<T>(string functionName, Func<T> function)
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting {nameof(Block)}: {functionName}");
                return function();
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} - {1} - Error: {2}", CONNECTION_STRING_CONTEXT, functionName, JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        /// <summary>
        /// With a function name, write code in a safe try catch logged environment
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="function"> async () => { async code; return T } </param>
        /// <returns></returns>
        public async Task BlockAsync(string functionName, Func<Task> function)
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting {nameof(BlockAsync)}: {functionName}");
                await function();
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} - {1} - Error: {2}", CONNECTION_STRING_CONTEXT.ToUpperInvariant(), functionName, JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        /// <summary>
        /// With a function name, write code in a safe try catch logged environment
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="function"> () => { code; return T } </param>
        /// <returns></returns>
        public void Block(string functionName, VoidFunc function)
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting {nameof(Block)}: {functionName}");
                function();
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} - {1} - Error: {2}", CONNECTION_STRING_CONTEXT, functionName, JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        /// <summary>
        /// With a function name, give a block of code that contains a connection to run within a safe try catch environment and returns the object wanted (T)
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="function"> async (connection) => { async code; return T } </param>
        public async Task<T> BlockAsync<T>(string functionName, System.Func<SqlConnection, Task<T>> function)
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting {nameof(BlockAsync)}: {functionName}");
                return await function(Connection());
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} - {1} - Error: {2}", CONNECTION_STRING_CONTEXT, functionName, JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        /// <summary>
        /// With a function name, give a block of code that contains a connection to run within a safe try catch environment and returns the object wanted (T)
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="function"> (connection) => { code; return T } </param>
        public T Block<T>(string functionName, System.Func<SqlConnection, T> function)
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting {nameof(Block)}: {functionName}");
                return function(Connection());
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} - {1} - Error: {2}", CONNECTION_STRING_CONTEXT, functionName, JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        /// <summary>
        /// With a function name, give a block of code that contains a connection to run within a safe try catch environment
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="function"> async (connection) => { async code } </param>
        public async Task BlockAsync(string functionName, Func<SqlConnection, Task> function)
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting {nameof(BlockAsync)}: {functionName}");
                await function(Connection());
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} - {1} - Error: {2}", CONNECTION_STRING_CONTEXT, functionName, JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        /// <summary>
        /// With a function name, give a block of code that contains a connection to run within a safe try catch environment
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="function"> (connection) => { code } </param>
        public void Block(string functionName, VoidFunc<SqlConnection> function)
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting {nameof(Block)}: {functionName}");
                function(Connection());
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} - {1} - Error: {2}", CONNECTION_STRING_CONTEXT, functionName, JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        #endregion

        /* Query */

        #region Get_By_Id
        /// <summary>
        /// Gets item in table by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>item in table with id</returns>
        public async Task<T> GetByIdAsync<T, G>(G id, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(GetByIdAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return await connection.QueryFirstOrDefaultAsync<T>(GetByIdString<T, G>(ref queryString, id, tableName, onlyActiveRows: onlyActiveRows));
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.GetByIdAsync - Error: {0} \n Built Query String: {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Gets item in table by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>item in table with id</returns>
        public T GetById<T, G>(G id, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(GetById)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return connection.QueryFirstOrDefault<T>(GetByIdString<T, G>(ref queryString, id, tableName, onlyActiveRows));
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.GetById - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        public string GetByIdString<T, G>(ref string queryString, G id, string tableName, bool onlyActiveRows = true)
        {
            try
            {
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                var properties = TypeDescriptor.GetProperties(typeof(T));
                queryString = $"Select Top(1) {_sqlUtilityService.GetAllSelectValues<T>()} From [{tableName}] Where {properties[0].Name} = {_sqlUtilityService.SQLValue(id)}";
                return queryString.ApplyIsActive<T>(onlyActiveRows);
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.GetByIdString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        #endregion

        #region Get_All_Items_In_Table
        /// <summary>
        /// Grab all items in table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <param name="onlyActiveRows">Only allow active rows</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAllItemsInTableAsync<T>(string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(GetAllItemsInTableAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return await connection.QueryAsync<T>(GetAllItemsInTableString<T>(ref queryString, tableName, onlyActiveRows));
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.GetAllItemsInTableAsync - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Grab all items in table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <param name="onlyActiveRows">Only allow active rows</param>
        /// <returns></returns>
        public IEnumerable<T> GetAllItemsInTable<T>(string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(GetAllItemsInTable)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return connection.Query<T>(GetAllItemsInTableString<T>(ref queryString, tableName, onlyActiveRows));

            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.GetAllFromTable - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        private string GetAllItemsInTableString<T>(ref string queryString, string tableName, bool onlyActiveRows = true)
        {
            try
            {
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                queryString = $"Select {_sqlUtilityService.GetAllSelectValues<T>()} From [{tableName}] ";
                return queryString.ApplyIsActive<T>(onlyActiveRows);
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.GetAllItemsInTableString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        #endregion

        #region Where
        /// <summary>
        /// Search through table via where statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereStatement"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>IEnumerable of items</returns>
        public async Task<IEnumerable<T>> WhereAsync<T>(string whereStatement, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(WhereAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return await connection.QueryAsync<T>(WhereString<T>(ref queryString, whereStatement, tableName, onlyActiveRows));
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.WhereAsync - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Search through table via where statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereStatement"></param>
        /// <param name="values">Object containing @value parameters used</param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>IEnumerable of items</returns>
        public async Task<IEnumerable<T>> WhereAsync<T>(string whereStatement, object values, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(WhereAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return await connection.QueryAsync<T>(WhereString<T>(ref queryString, whereStatement, tableName, onlyActiveRows), values);
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.WhereAsync - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Search through table via where statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereStatement"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>IEnumerable of items</returns>
        public IEnumerable<T> Where<T>(string whereStatement, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(Where)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return connection.Query<T>(WhereString<T>(ref queryString, whereStatement, tableName, onlyActiveRows));

            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.Where - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Search through table via where statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereStatement"></param>
        /// <param name="values">Object containing @value parameters used</param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>IEnumerable of items</returns>
        public IEnumerable<T> Where<T>(string whereStatement, object values, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(Where)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return connection.Query<T>(WhereString<T>(ref queryString, whereStatement, tableName, onlyActiveRows), values);
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.Where - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        public string WhereString<T>(ref string queryString, string whereStatement, string tableName = "", bool onlyActiveRows = true)
        {
            try
            {
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                queryString = @$"Select {_sqlUtilityService.GetAllSelectValues<T>()} from [{tableName}] {(!whereStatement.Trim().Equals("") ? $"Where {whereStatement}" : "")}";
                return queryString.ApplyIsActive<T>(onlyActiveRows);
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.WhereString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        #endregion

        #region First_Or_Default
        /// <summary>
        /// Grab the first or default item based on the where statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereStatement"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>Single item from table</returns>
        public async Task<T> FirstOrDefaultAsync<T>(string whereStatement, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(FirstOrDefaultAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return await connection.QueryFirstOrDefaultAsync<T>(FirstOrDefaultString<T>(ref queryString, whereStatement, tableName, onlyActiveRows));
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.FirstOrDefaultAsync - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Search through table via where statement and only get one row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereStatement"></param>
        /// <param name="values">Object containing @value parameters used</param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>IEnumerable of items</returns>
        public async Task<T> FirstOrDefaultAsync<T>(string whereStatement, object values, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(FirstOrDefaultAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return await connection.QueryFirstOrDefaultAsync<T>(FirstOrDefaultString<T>(ref queryString, whereStatement, tableName, onlyActiveRows), values);
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.FirstOrDefaultAsync - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Grab the first or default item based on the where statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereStatement"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>Single item from table</returns>
        public T FirstOrDefault<T>(string whereStatement, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(FirstOrDefault)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return connection.QueryFirstOrDefault<T>(FirstOrDefaultString<T>(ref queryString, whereStatement, tableName, onlyActiveRows));
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.FirstOrDefault - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Search through table via where statement and only get one row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereStatement"></param>
        /// <param name="values">Object containing @value parameters used</param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns>IEnumerable of items</returns>
        public T FirstOrDefault<T>(string whereStatement, object values, string tableName = "", bool onlyActiveRows = true)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(FirstOrDefault)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                    return connection.QueryFirstOrDefault<T>(FirstOrDefaultString<T>(ref queryString, whereStatement, tableName, onlyActiveRows), values);
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.FirstOrDefault - Error: {0} \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        private string FirstOrDefaultString<T>(ref string queryString, string whereStatement, string tableName, bool onlyActiveRows = true)
        {
            try
            {
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                queryString = $"Select Top(1) {_sqlUtilityService.GetAllSelectValues<T>()} from [{tableName}] {(!whereStatement.Trim().Equals("") ? $" WHERE {whereStatement}" : "")}";
                return queryString.ApplyIsActive<T>(onlyActiveRows);
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseQueryHandler.FirstOrDefaultString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        #endregion

        /* Command */

        #region Insert
        /// <summary>
        /// Insert into a sql table
        /// </summary>
        /// <typeparam name="T">Order of properties in class should match sql table order</typeparam>
        /// <param name="item"></param>
        /// <param name="insertWithPrimary">Set this to true if the insertion should contain the primary key</param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns></returns>
        public T Insert<T>(T item, bool insertWithPrimary = false, string tableName = "")
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(Insert)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    return connection.QueryFirstOrDefault<T>(InsertString(ref queryString, item, insertWithPrimary, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.Insert - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        /// <summary>
        /// Insert into a sql table
        /// </summary>
        /// <typeparam name="T">Order of properties in class should match sql table order</typeparam>
        /// <param name="item"></param>
        /// <param name="insertWithPrimary">Set this to true if the insertion should contain the primary key</param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        /// <returns></returns>
        public async Task<T> InsertAsync<T>(T item, bool insertWithPrimary = false, string tableName = "")
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(InsertAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    return await connection.QueryFirstOrDefaultAsync<T>(InsertString(ref queryString, item, insertWithPrimary, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.InsertAsync - Error: {0}", JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        public string InsertString<T>(ref string queryString, T item, bool insertWithPrimary = false, string tableName = "")
        {
            try
            {
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                queryString = $"INSERT INTO dbo.[{tableName}] (";
                var valuesString = " OUTPUT INSERTED.* VALUES ( ";
                var properties = TypeDescriptor.GetProperties(typeof(T));
                for (int i = (insertWithPrimary ? 0 : 1); i < properties.Count; i++)
                {
                    var property = properties[i];
                    queryString += $"{property.Name},";
                    valuesString += $"{_sqlUtilityService.SQLValue(property.GetValue(item) ?? DBNull.Value)},";
                }
                return queryString.TrimEnd(',') + ") " + valuesString.TrimEnd(',') + ")";
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.InsertString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Update object in sql table
        /// </summary>
        /// <typeparam name="T">Order of properties in class should match sql table order</typeparam>
        /// <param name="item"></param>
        /// <param name="updateWithPrimary">Set this to true if the update should contain the primary key</param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public void Update<T>(T item, bool updateWithPrimary = false, string tableName = "")
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(Update)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    connection.Execute(UpdateString(ref queryString, item, updateWithPrimary, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.Update - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        /// <summary>
        /// Update object in sql table
        /// </summary>
        /// <typeparam name="T">Order of properties in class should match sql table order</typeparam>
        /// <param name="item"></param>
        /// <param name="updateWithPrimary">Set this to true if the update should contain the primary key</param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public async Task UpdateAsync<T>(T item, bool updateWithPrimary = false, string tableName = "")
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(UpdateAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    await connection.ExecuteAsync(UpdateString(ref queryString, item, updateWithPrimary, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.UpdateAsync - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        public string UpdateString<T>(ref string queryString, T item, bool updateWithPrimary = false, string tableName = "")
        {
            try
            {
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    queryString = $"UPDATE dbo.[{tableName}] SET ";
                    var properties = TypeDescriptor.GetProperties(typeof(T));
                    var primaryKeyValue = _sqlUtilityService.SQLValue(properties[0].GetValue(item) ?? DBNull.Value);
                    for (int i = 0; i < properties.Count; i++)
                    {
                        if (i > 0 || updateWithPrimary) queryString += $"{properties[i].Name} = {_sqlUtilityService.SQLValue(properties[i].GetValue(item) ?? DBNull.Value)},";
                    }
                    queryString = queryString.TrimEnd(',') + $" WHERE {(properties[0]).Name} = {primaryKeyValue}";
                    if (primaryKeyValue != "NULL" && primaryKeyValue != "") return queryString;
                    else throw new Exception($"Invalid Primary Key : {primaryKeyValue}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.UpdateString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        #endregion

        #region Update_By_Property
        /// <summary>
        /// Update certain properties of a subset of items from a sql table based on the where statement
        /// </summary>
        /// <param name="whereStatement">Do not include where in here</param>
        /// <param name="propertiesToUpdate">Properties to update</param>
        /// <param name="tableName">Sql table name</param>
        public void UpdateByProperty(string whereStatement, Dictionary<string, object?> propertiesToUpdate, string tableName)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(UpdateByProperty)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    connection.Execute(UpdatePropertyString(ref queryString, whereStatement, propertiesToUpdate, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.UpdateByProperty - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Update certain properties of a subset of items from a sql table based on the where statement
        /// </summary>
        /// <param name="whereStatement">Do not include where in here</param>
        /// <param name="propertiesToUpdate">Properties to update</param>
        /// <param name="values">Object containing @value parameters used</param>
        /// <param name="tableName">Sql table name</param>
        public void UpdateByProperty(string whereStatement, Dictionary<string, object?> propertiesToUpdate, object values, string tableName)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(UpdateByProperty)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    connection.Execute(UpdatePropertyString(ref queryString, whereStatement, propertiesToUpdate, tableName), values);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.UpdateByProperty - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        /// <summary>
        /// Update certain properties of a subset of items from a sql table based on the where statement
        /// </summary>
        /// <param name="whereStatement">Do not include where in here</param>
        /// <param name="propertiesToUpdate">Properties to update</param>
        /// <param name="tableName">Sql table name</param>
        public async Task UpdateByPropertyAsync(string whereStatement, Dictionary<string, object?> propertiesToUpdate, string tableName)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(UpdateByPropertyAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    await connection.ExecuteAsync(UpdatePropertyString(ref queryString, whereStatement, propertiesToUpdate, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.UpdateByPropertyAsync - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Update certain properties of a subset of items from a sql table based on the where statement
        /// </summary>
        /// <param name="whereStatement">Do not include where in here</param>
        /// <param name="propertiesToUpdate">Properties to update</param>
        /// <param name="values">Object containing @value parameters used</param>
        /// <param name="tableName">Sql table name</param>
        public async Task UpdateByPropertyAsync(string whereStatement, Dictionary<string, object?> propertiesToUpdate, object values, string tableName)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(UpdateByPropertyAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    await connection.ExecuteAsync(UpdatePropertyString(ref queryString, whereStatement, propertiesToUpdate, tableName), values);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.UpdateByPropertyAsync - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        private string UpdatePropertyString(ref string queryString, string whereStatement, Dictionary<string, object?> propertiesToUpdate, string tableName)
        {
            try
            {
                queryString = $"UPDATE dbo.[{tableName}] SET ";
                foreach (var property in propertiesToUpdate)
                {
                    queryString += $"{property.Key} = {_sqlUtilityService.SQLValue(property.Value ?? DBNull.Value)},";
                }
                return queryString.TrimEnd(',') + (!whereStatement.Trim().Equals("") ? $" WHERE {whereStatement}" : "");
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.UpdatePropertyString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete object in sql table 
        /// </summary>
        /// <typeparam name="T">Order of properties in class should match sql table order</typeparam>
        /// <param name="item"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public void Delete<T>(T item, string tableName = "")
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(Delete)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    connection.Execute(DeleteString(ref queryString, item, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.Delete - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Delete object in sql table 
        /// </summary>
        /// <typeparam name="T">Order of properties in class should match sql table order</typeparam>
        /// <param name="item"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public async Task DeleteAsync<T>(T item, string tableName = "")
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(DeleteAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    await connection.ExecuteAsync(DeleteString(ref queryString, item, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.DeleteAsync - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        private string DeleteString<T>(ref string queryString, T item, string tableName)
        {
            try
            {
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                var properties = TypeDescriptor.GetProperties(typeof(T));
                return $"DELETE dbo.[{tableName}] WHERE {(properties[0]).Name} = {properties[0].GetValue(item) ?? DBNull.Value}";
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.DeleteString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        #endregion

        #region Delete_Where
        /// <summary>
        /// Delete objects in a sql table based on where statement
        /// </summary>
        /// <param name="whereStatement"></param>
        /// <param name="tableName">Sql table name</param>
        public void DeleteWhere(string whereStatement, string tableName)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(DeleteWhere)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    connection.Execute(DeleteWhereString(ref queryString, whereStatement, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.DeleteWhere - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Delete objects in a sql table based on where statement
        /// </summary>
        /// <param name="whereStatement"></param>
        /// <param name="values">Object containing @value parameters used</param>
        /// <param name="tableName">Sql table name</param>
        public void DeleteWhere(string whereStatement, object values, string tableName)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(DeleteWhere)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    connection.Execute(DeleteWhereString(ref queryString, whereStatement, tableName), values);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.DeleteWhere - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Delete objects in a sql table based on where statement
        /// </summary>
        /// <param name="whereStatement"></param>
        /// <param name="tableName">Sql table name</param>
        /// 
        public async Task DeleteWhereAsync(string whereStatement, string tableName)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(DeleteWhereAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    await connection.ExecuteAsync(DeleteWhereString(ref queryString, whereStatement, tableName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.DeleteWhereAsync - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        /// <summary>
        /// Delete objects in a sql table based on where statement
        /// </summary>
        /// <param name="whereStatement"></param>
        /// <param name="values">Object containing @value parameters used</param>
        /// <param name="tableName">Sql table name</param>
        public async Task DeleteWhereAsync(string whereStatement, object values, string tableName)
        {
            var queryString = "";
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(DeleteWhereAsync)}");
                using (var connection = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    await connection.ExecuteAsync(DeleteWhereString(ref queryString, whereStatement, tableName), values);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.DeleteWhereAsync - Error: {0}, \n Query: \n {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }

        private string DeleteWhereString(ref string queryString, string whereStatement, string tableName)
        {
            try
            {
                return $"DELETE dbo.[{tableName}] {(!whereStatement.Trim().Equals("") ? $" WHERE {whereStatement}" : "")}";
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.DeleteWhereString - Creation of query error: {0} \n Query String [Incomplete] : {1}", JsonConvert.SerializeObject(ex), queryString);
                throw;
            }
        }
        #endregion

        /* Bulks */

        #region Bulk_Insert
        /// <summary>
        /// Bulk insert into a sql table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public void BulkInsert<T>(IEnumerable<T> list, string tableName = "")
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(BulkInsert)}");
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                var dt = _sqlUtilityService.ConvertToDataTable(list);
                using var conn = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT);
                try
                {
                    conn.Open();
                    using var sqlBulk = new SqlBulkCopy(conn) { DestinationTableName = "[" + tableName + "]" };
                    sqlBulk.BulkCopyTimeout = 300;
                    sqlBulk.WriteToServer(dt);
                }
                catch (Exception ex)
                {
                    _logger.LogError("BaseDatabaseCommanderHandler.BulkUpdateAsync - Error: {0}", JsonConvert.SerializeObject(ex));
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.BulkUpdateAsync - Error: {0}", JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        /// <summary>
        /// Bulk insert into a sql table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public async Task BulkInsertAsync<T>(IEnumerable<T> list, string tableName = "")
        {
            try
            {
                _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(BulkInsertAsync)}");
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                var dt = _sqlUtilityService.ConvertToDataTable(list);
                using (var conn = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
                {
                    try
                    {
                        conn.Open();
                        using (var sqlBulk = new SqlBulkCopy(conn) { DestinationTableName = "[" + tableName + "]" })
                        {
                            await sqlBulk.WriteToServerAsync(dt);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("BaseDatabaseCommanderHandler.BulkUpdateAsync - Error: {0}", JsonConvert.SerializeObject(ex));
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("BaseDatabaseCommanderHandler.BulkInsertAsync - Error: {0}", JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        #endregion

        #region Bulk_Update
        /// <summary>
        /// Bulk update into a sql table
        /// </summary>
        /// <typeparam name="T">Primary key must be first property of class</typeparam>
        /// <param name="list"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public void BulkUpdate<T>(IEnumerable<T> list, HashSet<string>? propertiesToUpdate = null, string tableName = "")
        {
            _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(BulkUpdate)}");
            var dt = new DataTable("bulkUpdateTable");
            dt = _sqlUtilityService.ConvertToDataTable(list, propertiesToUpdate);

            using (var conn = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
            {
                using (var command = new SqlCommand("", conn))
                {
                    try
                    {
                        tableName = (tableName.Equals("") ? typeof(T).Name : tableName);
                        conn.Open();

                        //Creating temp table on database
                        command.CommandText = _sqlUtilityService.CreateTempTableSQLExecutable<T>(tableName, propertiesToUpdate);
                        command.ExecuteNonQuery();

                        //Bulk insert into temp table
                        using (SqlBulkCopy bulkcopy = new SqlBulkCopy(conn))
                        {
                            bulkcopy.BatchSize = 10000;
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = $"#Temp{tableName}";
                            bulkcopy.WriteToServer(dt);
                            bulkcopy.Close();
                        }

                        // Updating destination table, and dropping temp table
                        command.CommandTimeout = 300;
                        command.CommandText = _sqlUtilityService.UpdateTempTableSQLExecutable<T>(tableName, propertiesToUpdate);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("BaseDatabaseCommanderHandler.BulkUpdate - Error: {0}", JsonConvert.SerializeObject(ex));
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Bulk update into a sql table
        /// </summary>
        /// <typeparam name="T">Primary key must be first property of class</typeparam>
        /// <param name="list"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public async Task BulkUpdateAsync<T>(IEnumerable<T> list, HashSet<string>? propertiesToUpdate = null, string tableName = "")
        {
            _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(BulkUpdateAsync)}");
            var dt = new DataTable("bulkUpdateTable");
            dt = _sqlUtilityService.ConvertToDataTable(list, propertiesToUpdate);

            using (var conn = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
            {
                using (var command = new SqlCommand("", conn))
                {
                    try
                    {
                        tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                        conn.Open();

                        //Creating temp table on database
                        command.CommandText = _sqlUtilityService.CreateTempTableSQLExecutable<T>(tableName, propertiesToUpdate);
                        await command.ExecuteNonQueryAsync();

                        //Bulk insert into temp table
                        using (SqlBulkCopy bulkcopy = new SqlBulkCopy(conn))
                        {
                            bulkcopy.BatchSize = 10000;
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = $"#Temp{tableName}";
                            await bulkcopy.WriteToServerAsync(dt);
                            bulkcopy.Close();
                        }

                        // Updating destination table, and dropping temp table
                        command.CommandTimeout = 300;
                        command.CommandText = _sqlUtilityService.UpdateTempTableSQLExecutable<T>(tableName, propertiesToUpdate);
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("BaseDatabaseCommanderHandler.BulkUpdateAsync - Error: {0}", JsonConvert.SerializeObject(ex));
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion

        #region Bulk_Delete
        /// <summary>
        /// Bulk delete into a sql table
        /// </summary>
        /// <typeparam name="T">Primary key must be first property of class</typeparam>
        /// <param name="list"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public void BulkDelete<T>(IEnumerable<T> list, string tableName = "")
        {
            _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(BulkDelete)}");
            var dt = new DataTable("bulkUpdateTable");
            dt = _sqlUtilityService.ConvertToDataTable(list);

            using (var conn = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
            {
                using (var command = new SqlCommand("", conn))
                {
                    try
                    {
                        tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                        conn.Open();

                        //Creating temp table on database
                        command.CommandText = _sqlUtilityService.CreateTempTableSQLExecutable<T>(tableName);
                        command.ExecuteNonQuery();

                        //Bulk insert into temp table
                        using (SqlBulkCopy bulkcopy = new SqlBulkCopy(conn))
                        {
                            bulkcopy.BatchSize = 10000;
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = $"#Temp{tableName}";
                            bulkcopy.WriteToServer(dt);
                            bulkcopy.Close();
                        }

                        // Updating destination table, and dropping temp table
                        command.CommandTimeout = 300;
                        command.CommandText = _sqlUtilityService.DeleteTempTableSQLExecutable<T>(tableName);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("BaseDatabaseCommanderHandler.BulkDelete - Error: {0}", JsonConvert.SerializeObject(ex));
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Bulk delete into a sql table
        /// </summary>
        /// <typeparam name="T">Primary key must be first property of class</typeparam>
        /// <param name="list"></param>
        /// <param name="tableName">Class name used table name if no table name given</param>
        public async Task BulkDeleteAsync<T>(IEnumerable<T> list, string tableName = "")
        {
            _logger.LogDebug($"{CONNECTION_STRING_CONTEXT.ToUpper()} - Starting : {nameof(BulkDeleteAsync)}");
            var dt = new DataTable("bulkUpdateTable");
            dt = _sqlUtilityService.ConvertToDataTable(list);

            using (var conn = _connectionFactory.GetSqlConnection(CONNECTION_STRING_CONTEXT))
            {
                using (var command = new SqlCommand("", conn))
                {
                    try
                    {
                        tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                        conn.Open();

                        //Creating temp table on database
                        command.CommandText = _sqlUtilityService.CreateTempTableSQLExecutable<T>(tableName);
                        await command.ExecuteNonQueryAsync();

                        //Bulk insert into temp table
                        using (SqlBulkCopy bulkcopy = new SqlBulkCopy(conn))
                        {
                            bulkcopy.BatchSize = 10000;
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = $"#Temp{tableName}";
                            await bulkcopy.WriteToServerAsync(dt);
                            bulkcopy.Close();
                        }

                        // Updating destination table, and dropping temp table
                        command.CommandTimeout = 300;
                        command.CommandText = _sqlUtilityService.DeleteTempTableSQLExecutable<T>(tableName);
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("BaseDatabaseCommanderHandler.BulkDeleteAsync - Error: {0}", JsonConvert.SerializeObject(ex));
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion


        public void WriteToTempTable<T>(SqlConnection connection, IEnumerable<T> list, HashSet<string>? tempTableProperties = null, string tableName = "")
        {
            if (!list.Any()) return;
            using (var command = new SqlCommand("", connection))
            {
                var dt = new DataTable($"{tableName}Data");
                dt = _sqlUtilityService.ConvertToDataTable(list, tempTableProperties);
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                var tempTableExec = _sqlUtilityService.CreateTempTableSQLExecutable<T>(tableName, tempTableProperties);
                command.CommandText = tempTableExec;
                command.ExecuteNonQuery();

                //Bulk insert into temp table
                using (SqlBulkCopy bulkcopy = new SqlBulkCopy(command.Connection))
                {
                    bulkcopy.BatchSize = 10000;
                    bulkcopy.BulkCopyTimeout = 660;
                    bulkcopy.DestinationTableName = $"#Temp{tableName}";
                    bulkcopy.WriteToServer(dt);
                    bulkcopy.Close();
                }
            }
        }

        public async Task WriteToTempTableAsync<T>(SqlConnection connection, IEnumerable<T> list, HashSet<string>? tempTableProperties = null, string tableName = "")
        {
            if (!list.Any()) return;
            using (var command = new SqlCommand("", connection))
            {
                var dt = new DataTable($"{tableName}Data");
                dt = _sqlUtilityService.ConvertToDataTable(list, tempTableProperties);
                tableName = tableName.Equals("") ? typeof(T).Name : tableName;
                var tempTableExec = _sqlUtilityService.CreateTempTableSQLExecutable<T>(tableName, tempTableProperties);
                command.CommandText = tempTableExec;
                await command.ExecuteNonQueryAsync();

                //Bulk insert into temp table
                using (SqlBulkCopy bulkcopy = new SqlBulkCopy(command.Connection))
                {
                    bulkcopy.BatchSize = 10000;
                    bulkcopy.BulkCopyTimeout = 660;
                    bulkcopy.DestinationTableName = $"#Temp{tableName}";
                    await bulkcopy.WriteToServerAsync(dt);
                    bulkcopy.Close();
                }
            }
        }

    }
}

