using SendGrid.Helpers.Mail;
using SendGrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MediaCreatorSite.Utility.Results;
using Stripe;
using static MediaCreatorSite.Services.EmailService;
using MediaCreatorSite.Models;

namespace MediaCreatorSite.Services
{
    public interface IEmailService {
        Task<BaseResult> SendResetPasswordEmail(IEnumerable<EmailAddress> to, string token);
        Task<BaseResult> SendConfirmationEmail(IEnumerable<EmailAddress> to, string token);
        Task<Response?> SendEmail(SendGridMessage email);
        Task<BaseResult> SendReceipt(IEnumerable<EmailAddress> to, CreditPurchaseReceipt receiptInfo);
        Task<BaseResult> SendAlertEmail(IEnumerable<EmailAddress> to, Alert alert);
    }
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly SendGridClient _sendGridClient;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _sendGridClient = new SendGridClient(_configuration["SendGrind:API_KEY"]);
        }

        public async Task<BaseResult> SendConfirmationEmail(IEnumerable<EmailAddress> to, string token)
        {
            var result = new BaseResult();
            try
            {
                var sendGridMessage = new SendGridMessage();
                sendGridMessage.SetFrom(_configuration["SendGrind:ConfirmationEmail:Email"], _configuration["StoreInfo:Name"]);
                sendGridMessage.AddTos(to.ToList());
                sendGridMessage.SetTemplateId(_configuration["SendGrind:ConfirmationEmail:TemplateId"]);
                sendGridMessage.Subject = _configuration["SendGrind:ConfirmationEmail:Subject"];
                sendGridMessage.SetTemplateData(new
                {
                    header_message = _configuration["SendGrind:ConfirmationEmail:header_message"],
                    company_name = _configuration["SendGrind:ConfirmationEmail:company_name"],
                    btn_message = _configuration["SendGrind:ConfirmationEmail:btn_message"],
                    btn_link = _configuration["DomainLink"] + _configuration["SendGrind:ConfirmationEmail:btn_link"] + token + "&email=" + to.First().Email,
                });
                var response = await SendEmail(sendGridMessage);
                if (response != null && !response.IsSuccessStatusCode)
                {
                    var bodyResult = await response.Body.ReadAsStringAsync();
                    _logger.LogWarning($"IEmail Service - Error Sending Email - {bodyResult}");
                    result.exception = new Exception($"Error with sending email. Code: {response.StatusCode}");
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"IEmail Service - SendConfirmationEmail - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result;
        }

        public async Task<BaseResult> SendResetPasswordEmail(IEnumerable<EmailAddress> to, string token)
        {

            var result = new BaseResult();
            try
            {
                var sendGridMessage = new SendGridMessage();
                sendGridMessage.SetFrom(_configuration["SendGrind:ResetPasswordEmail:Email"], _configuration["StoreInfo:Name"]);
                sendGridMessage.AddTos(to.ToList());
                sendGridMessage.SetTemplateId(_configuration["SendGrind:ResetPasswordEmail:TemplateId"]);
                sendGridMessage.SetTemplateData(new
                {
                    header_message = _configuration["SendGrind:ResetPasswordEmail:header_message"],
                    company_name = _configuration["SendGrind:ResetPasswordEmail:company_name"],
                    btn_message = _configuration["SendGrind:ResetPasswordEmail:btn_message"],
                    btn_link = _configuration["DomainLink"] + _configuration["SendGrind:ResetPasswordEmail:btn_link"] + token + "&email=" + to.First().Email,
                });
                var response = await SendEmail(sendGridMessage);

                if (response != null && !response.IsSuccessStatusCode)
                {
                    var bodyResult = await response.Body.ReadAsStringAsync();
                    _logger.LogWarning($"IEmail Service - Error Sending Email - {bodyResult}");
                    result.exception = new Exception($"Error with sending email. Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IEmailService - SendResetPasswordEmail - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result;
        }

        public async Task<BaseResult> SendReceipt(IEnumerable<EmailAddress> to, CreditPurchaseReceipt receiptInfo)
        {
            var result = new BaseResult();
            try
            {
                var sendGridMessage = new SendGridMessage();
                sendGridMessage.SetFrom(_configuration["SendGrind:Receipt:Email"], _configuration["StoreInfo:Name"]);
                sendGridMessage.AddTos(to.ToList());
                sendGridMessage.SetTemplateId(_configuration["SendGrind:Receipt:TemplateId"]);
                sendGridMessage.SetTemplateData(new
                {
                    website_name = _configuration["SendGrind:Receipt:website_name"],
                    company_name = _configuration["SendGrind:Receipt:company_name"],
                    current_year = DateTime.UtcNow.Year,
                    purchase_date = DateTime.UtcNow.ToString(),
                    transaction_id = receiptInfo.creditPurchaseHistoryId,
                    credits_purchased = receiptInfo.creditsPucharchased,
                    charge_amount = receiptInfo.charge,
                    last_4_digits = receiptInfo.last4DigitsOfCard,
                    support_email = _configuration["ContactInfo:Email:General"],
                    terms_and_conditions_link = _configuration["DomainLink"] + "/legal/terms-of-service"
                });

                var response = await SendEmail(sendGridMessage);
                if (response != null && !response.IsSuccessStatusCode)
                {
                    var bodyResult = await response.Body.ReadAsStringAsync();
                    _logger.LogWarning($"IEmail Service - Error Sending Email - {bodyResult}");
                    result.exception = new Exception($"Error with sending email. Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IEmailService - SendResetPasswordEmail - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result;
        }

        public async Task<BaseResult> SendAlertEmail(IEnumerable<EmailAddress> to, Alert alert)
        {
            var result = new BaseResult();
            try
            {
                var sendGridMessage = new SendGridMessage();
                sendGridMessage.SetFrom(_configuration["SendGrid:Alert:Email"]);
                sendGridMessage.AddTos(to.ToList());
                sendGridMessage.SetTemplateId(_configuration["SendGrind:Alert:TemplateId"]);
                sendGridMessage.SetTemplateData(new
                {
                    title= alert.title,
                    body= alert.body
                });
                var response = await SendEmail(sendGridMessage);
                if (_configuration["SendGrind:Active"].Equals("true"))
                {
                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        var bodyResult = await response.Body.ReadAsStringAsync();
                        _logger.LogWarning($"IEmail Service - Error Sending Email - {bodyResult}");
                        result.exception = new Exception($"Error with sending email. Code: {response.StatusCode}");
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"IEmailService - SendResetPasswordEmail - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result;
        }

        public async Task<Response?> SendEmail(SendGridMessage email)
        {
            if (_configuration["SendGrind:Active"].Equals("true"))
                return await _sendGridClient.SendEmailAsync(email);
            else
            {
                _logger.LogInformation($"EmailService: Emailing Inactive. Email suppose to send:\n {JsonConvert.SerializeObject(email)}");
                return null;
            }
        }
    }
}
