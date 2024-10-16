using Grpc.Core;
using MediaCreatorSite.DataAccess;
using MediaCreatorSite.DataAccess.Constants;
using MediaCreatorSite.DataAccess.DTO;
using MediaCreatorSite.DataAccess.QueryModels;
using MediaCreatorSite.Models;
using MediaCreatorSite.Services;
using MediaCreatorSite.Utility.Attributes;
using MediaCreatorSite.Utility.Exceptions;
using MediaCreatorSite.Utility.Extensions;
using MediaCreatorSite.Utility.Results;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using Stripe;

namespace MediaCreatorSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly IStripeService _stripeService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IMediaCreatorDatabase database, IStripeService stripeService, IEmailService emailService, ILogger<ProfileController> logger)
        {
            _database = database;
            _stripeService = stripeService;
            _emailService = emailService;
            _logger = logger;
        }

        public class ProfileIndexData
        {
            public double credits { get; set; }
        }

        [EmailVerified]
        [HttpGet]
        public async Task<string> Get()
        {
            var result = new DataResult<ProfileIndexData>() { data = new ProfileIndexData() };
            try
            {
                var sessionInfo = HttpContext.Items["SessionInfo"] as SessionInfo;
                var credit = await _database.FirstOrDefaultAsync<Credit>("user_id = @userId", new { @userId = sessionInfo.user.id });
                if (credit == null) throw new ObjectDoesNotExistException("Credit - user_id", sessionInfo.user.id);
                result.data.credits = credit.amount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Video Controller - PurchaseCredits - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }

            return result.CloseResult();
        }

        public class PurchaseCreditsModel
        {
            public double credits { get; set; }
            public string paymentMethodId { get; set; }
            public string last4Digits { get; set; }
        }

        [IsNotScary]
        [HttpPost]
        [Route("PurchaseCredits")]
        public async Task<string> PurchaseCredits([FromBody] PurchaseCreditsModel model)
        {
            var result = new BaseResult();
            try
            {
                var sessionInfo = HttpContext.Items["SessionInfo"] as SessionInfo;

                // Convert the credits to an amount in cents (or the smallest currency unit)
                var amount = Convert.ToInt64((model.credits - 0.01) * 100);

                // Create a PaymentIntent with the order amount and currency
                var paymentIntent = await _stripeService.CreatePaymentIntent(amount, "cad");

                // Confirm the payment using the payment method obtained from the frontend
                var confirmOptions = new PaymentIntentConfirmOptions { PaymentMethod = model.paymentMethodId };
                var confirmedPaymentIntent = await _stripeService.ConfirmPaymentIntent(paymentIntent.Id, confirmOptions);

                var responseJSon = JsonConvert.SerializeObject(confirmedPaymentIntent);

                if (!confirmedPaymentIntent.Status.Equals("succeeded")) throw new PaymentFailedException(confirmedPaymentIntent.StripeResponse.Content);

                var credit = await _database.FirstOrDefaultAsync<Credit>("user_id = @userId", new { @userId = sessionInfo.user.id });

                credit.amount += Convert.ToDouble(model.credits);
                await _database.UpdateAsync(credit);

                //Add to history
                var creditPurchaseHistory = await _database.InsertAsync(new CreditPurchaseHistory()
                {
                    credit_id = credit.id,
                    user_id = sessionInfo.user.id,
                    amount = Convert.ToDouble(model.credits),
                    created_date = DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                    modified_by = sessionInfo.user.email
                });

                //Send receipt email
                var emailList = new List<EmailAddress>() { new EmailAddress() { Email = sessionInfo.user.email } };
                var emailResult = await _emailService.SendReceipt(emailList, new CreditPurchaseReceipt()
                {
                    creditPurchaseHistoryId = creditPurchaseHistory.id,
                    charge = $"${model.credits-1}.99",
                    creditsPucharchased = model.credits,
                    last4DigitsOfCard = model.last4Digits
                });
                result.exception = emailResult.exception;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - PurchaseCredits - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }

            return result.CloseResult();
        }

        [EmailVerified]
        [HttpGet]
        [Route("GetCredits")]
        public async Task<string> GetCredits()
        {
            var result = new DataResult<double>();
            try
            {
                var sessionInfo = HttpContext.Items["SessionInfo"] as SessionInfo;
                var credit = await _database.FirstOrDefaultAsync<Credit>("user_id = @userId", new { @userId = sessionInfo.user.id });
                if(credit == null)
                {
                    credit = await _database.InsertAsync(new Credit() { user_id = sessionInfo.user.id, amount = 0.0, created_date = DateTime.UtcNow, modified_date = DateTime.UtcNow, modified_by = sessionInfo.user.email });
                }
                result.data = credit.amount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - PurchaseCredits - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }

            return result.CloseResult();
        }

    }
}
