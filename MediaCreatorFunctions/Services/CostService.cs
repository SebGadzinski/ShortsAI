using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Utility;
using MediaCreatorFunctions.Utility.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Services
{
    public interface ICostService
    {
        Task<StoreReceipt> AddReceipt(int storeId, double cost, string purpose, Guid userId);
    }

    public class CostService : ICostService
    {
        private readonly ILogger<CostService> _logger;
        private readonly IMediaCreatorDatabase _database;

        public CostService(ILogger<CostService> logger, IMediaCreatorDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        public async Task<StoreReceipt> AddReceipt(int storeId, double cost, string purpose, Guid userId)
        {
            try
            {
                var store = _database.GetById<Store, int>(storeId);
                if (store == null) throw new ObjectDoesNotExistException(nameof(Store), storeId);

                return await _database.InsertAsync(new StoreReceipt()
                {
                    user_id = userId,
                    store_id = storeId,
                    cost = cost,
                    purpose = purpose,
                    created_date = DateTime.UtcNow,
                });
            }
            catch(Exception ex)
            {
                _logger.LogError($"ChatGPTService - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }
    }
}
