using MediaCreatorSite.DataAccess;
using MediaCreatorSite.DataAccess.DTO;
using MediaCreatorSite.Utility;
using MediaCreatorSite.Utility.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using Stripe.FinancialConnections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.Services
{
    public interface IStripeService
    {
        Task<PaymentIntent> CreatePaymentIntent(long amount, string currency);
        Task<PaymentIntent> ConfirmPaymentIntent(string paymentIntentId, PaymentIntentConfirmOptions options);
    }

    public class StripeService : IStripeService
    {
        private readonly ILogger<StripeService> _logger;
        private readonly IMediaCreatorDatabase _database;
        private readonly IConfiguration _configuration;

        public StripeService(ILogger<StripeService> logger, IMediaCreatorDatabase database, IConfiguration configuration)
        {
            _logger = logger;
            _database = database;
            _configuration = configuration;

            StripeConfiguration.ApiKey = _configuration["Stripe:SECRET_KEY"];
        }

        public async Task<PaymentIntent> CreatePaymentIntent(long amount, string currency)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = currency,
                    // Add any other options you need
                };

                var service = new PaymentIntentService();
                return await service.CreateAsync(options);
            }
            catch(Exception ex)
            {
                _logger.LogError($"IStripeService - CreatePaymentIntent - Error: {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public async Task<PaymentIntent> ConfirmPaymentIntent(string paymentIntentId, PaymentIntentConfirmOptions options)
        {
            try
            {
                var service = new PaymentIntentService();
                return await service.ConfirmAsync(paymentIntentId, options);
            }
            catch (Exception ex)
            {
                _logger.LogError($"IStripeService - ConfirmPaymentIntent - Error: {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

    }
}
