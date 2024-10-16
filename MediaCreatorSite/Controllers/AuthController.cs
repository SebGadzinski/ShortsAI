using MediaCreatorSite.DataAccess;
using MediaCreatorSite.DataAccess.Constants;
using MediaCreatorSite.DataAccess.Dto;
using MediaCreatorSite.DataAccess.DTO;
using MediaCreatorSite.DataAccess.QueryModels;
using MediaCreatorSite.Models;
using MediaCreatorSite.Services;
using MediaCreatorSite.Utility.Constants;
using MediaCreatorSite.Utility.Exceptions;
using MediaCreatorSite.Utility.Extensions;
using MediaCreatorSite.Utility.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;

namespace MediaCreatorSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IMediaCreatorDatabase _database;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IEmailService emailService, IMediaCreatorDatabase database, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger<AuthController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _emailService = emailService;
            _database = database;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("GetSession")]
        public string GetSession()
        {
            var result = new DataResult<SessionInfo> { };
            try
            {
                var sessionInfo = this.GetSessionInfo();
                if (sessionInfo == null)
                {
                    sessionInfo = new SessionInfo();
                    this.SetSessionInfo(sessionInfo);
                }
                result.successResult = "session grabbed";
                result.data = sessionInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - GetSession - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            var finalResult = result.CloseResult();
            return finalResult;
        }

        [HttpGet]
        [Route("CheckEmailVerified")]
        public async Task<string> CheckEmailVerified()
        {
            var result = new DataResult<bool> { data = false};
            try
            {
                var sessionInfo = this.GetSessionInfo();
                if (sessionInfo != null && sessionInfo.user != null)
                {
                    result.data = await _userManager.IsEmailConfirmedAsync(sessionInfo.user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - CheckEmailVerified - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            var finalResult = result.CloseResult();
            return finalResult;
        }

        public class LoginModel
        {
            public string email { get; set; } = "";
            public string password { get; set; } = "";
            public string deviceName { get; set; } = "";
        }
        //[AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<string> Login([FromBody] LoginModel model)
        {
            var result = new DataResult<SessionInfo> { };
            try
            {
                var user = await _database.FirstOrDefaultAsync<AppUser>($"email = '{model.email.ToUpper()}'");
                //Check if the user exists
                if (user != null)
                {
                    //Check if the password is correct
                    var passwordCheckResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.password, model.password).GetHashCode();
                    if(passwordCheckResult == 2)
                    {
                        _logger.LogError($"Auth Controller - Login Password Outdated - {model.email}");
                    }
                    if (passwordCheckResult == 1)
                    {
                        var sessionInfo = new SessionInfo();
                        var currentSession = this.GetSessionInfo();
                        var newSession = new AppUserSession() { user_id = user.id, device_name = model.deviceName, created_date = DateTime.UtcNow, modified_date = DateTime.UtcNow, modified_by = user.email };
                        newSession = await _database.InsertAsync(newSession);

                        //Remove the security proprties
                        user.password = "";
                        sessionInfo.user = user;
                        sessionInfo.roles = (await _database.GetUserRolesAsync(user.id)).ToList();
                        sessionInfo.claims = (await _database.GetUserClaimsAsync(user.id)).ToList();
                        sessionInfo.sessionId = newSession.id;
                        sessionInfo.lastLoginDate = DateTime.UtcNow;

                        if(currentSession != null)
                        {
                            sessionInfo.language = currentSession.language;
                            sessionInfo.currency = currentSession.currency;
                        }

                        result.data = sessionInfo;
                        this.SetSessionInfo(sessionInfo);

                        result.successResult = "Login Successful";
                    }
                    else
                    {
                        result.errorResult = "Password is not correct";
                    }
                }
                else
                {
                    result.errorResult = "User with that email does not exist";
                }
            }
            catch (Exception ex)
            {
                
                result.exception = ex;
            }
            var finalResult = result.CloseResult();
            return finalResult;
        }

        public class SignUpModel
        {
            public string email { get; set; } = "";
            public string password { get; set; } = "";
            public string confirmedPassword { get; set; } = "";
            public string phoneNumber { get; set; } = "";
            public string deviceName { get; set; } = "";
            public List<SingleClaim> claims {get; set;} = new List<SingleClaim>();
        }

        [HttpPost]
        //[Authorize(Roles = Roles.AuthorizeShopper)]
        [Route("SignUp")]
        public async Task<string> SignUp([FromBody] SignUpModel model)
        {
            var result = new DataResult<SessionInfo> { };
            try
            {
                model.email = model.email.ToUpper();
                //CHECK FOR PROBLEMS
                if (!model.password.Equals(model.confirmedPassword)) throw new ConfirmedPasswordNotMatchingException($"The password given does not match the confirmed password");
                if (!IsValidEmail(model.email)) throw new EmailNotOkException();

                var existingUser = _database.FirstOrDefault<AppUser>($"email = '{model.email.ToUpper()}'");
                if (existingUser != null)
                {
                    result.errorResult = "User Already Exists";
                    throw new UserAlreadyExistsException($"The email {model.email.ToUpper()}");
                }

                //ALL GOOD CREATE USER
                var newUser = new AppUser()
                {
                    email = model.email.ToUpper(),
                    password = model.password,
                    email_confirmed = false,
                    user_name = model.email.ToUpper(),
                    phone_number = model.phoneNumber,
                    phone_number_confirmed = false,
                    two_factor_enabled= false,
                    created_date = DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                    modified_by = model.email
                };
                newUser.password = _userManager.PasswordHasher.HashPassword(newUser, newUser.password);
                //Create new user
                newUser = await _database.InsertAsync(newUser);

                var getShopperRole = await _database.FirstOrDefaultAsync<AppRole>("name = 'shopper'");
                await _userManager.AddToRoleAsync(newUser, getShopperRole.name);

                var allClaims = (await _database.GetAllItemsInTableAsync<AppClaim>()).ToDictionary(x => x.name);
                var userClaims = new List<AppUserClaim>();
                model.claims.ForEach(x =>
                {
                    if (allClaims.ContainsKey(x.name))
                    {
                        userClaims.Add(new AppUserClaim()
                        {
                            id = Guid.NewGuid(),
                            claim_id = allClaims[x.name].id,
                            user_id = newUser.id,
                            value = x.value,
                            created_date = DateTime.UtcNow,
                            modified_date = DateTime.UtcNow,
                            modified_by = "sign-up"
                        });
                    }
                });
                await _database.BulkInsertAsync(userClaims);

                //Do not give them the correct guid
                var sessionInfo = new SessionInfo();
                var newSession = new AppUserSession() { user_id = newUser.id, device_name = model.deviceName, created_date = DateTime.UtcNow, modified_date = DateTime.UtcNow, modified_by = newUser.email };
                newSession = await _database.InsertAsync(newSession);
                newUser.password = "";
                sessionInfo.user = newUser;
                sessionInfo.roles = new List<SingleRole>() { new SingleRole() { name = getShopperRole.name } };
                sessionInfo.claims = (await _database.GetUserClaimsAsync(newUser.id)).ToList();
                sessionInfo.sessionId = newSession.id;
                sessionInfo.lastLoginDate = DateTime.UtcNow;

                var currentSession = this.GetSessionInfo();
                if (currentSession != null)
                {
                    sessionInfo.language = currentSession.language;
                    sessionInfo.currency = currentSession.currency;
                }

                this.SetSessionInfo(sessionInfo);
                result.data = sessionInfo;
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                var emailList = new List<EmailAddress>() { new EmailAddress() { Email = model.email, Name = "New Shopper" } };
                var emailResult = await _emailService.SendConfirmationEmail(emailList, token);

                //Add Credits
                await _database.InsertAsync(new Credit()
                {
                    user_id = newUser.id,
                    amount = 0,
                    created_date = DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                    modified_by = "Sign Up"
                });

                result.exception = emailResult.exception;
                result.successResult = "Your confirmation email was sent";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - SignUp - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result.CloseResult();
        }

        public class SendConfirmationModel
        {
            public string userId { get; set; } = "";
        }
        [HttpPost]
        [Route("SendConfirmationEmail")]
        public async Task<string> SendConfirmationEmail([FromBody] SendConfirmationModel model)
        {
            var result = new BaseResult { };
            try
            {
                //Check if this user exists
                if (this.CheckForRole(Roles.AuthorizeShopperRoles, result))
                {
                    var dbUser = await _database.FirstOrDefaultAsync<AppUser>("id = @userId", new { model.userId });
                    if(dbUser != null)
                    {
                        //Check if this user is confirmed
                        if (!dbUser.email_confirmed)
                        {
                            var token = await _userManager.GenerateEmailConfirmationTokenAsync(dbUser);
                            var emailList = new List<EmailAddress>() { new EmailAddress() { Email = dbUser.email } };
                            var emailResult = await _emailService.SendConfirmationEmail(emailList, token);
                            result.exception = emailResult.exception;
                            result.successResult = "Your email was sent";
                        }
                        else
                        {
                            result.successResult = "User is already confirmed";
                        }
                    }
                    else
                    {
                        result.successResult = "User does not exist";
                    }
                }
                else
                {
                    result.errorResult = "User with that email does not exist";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - SendConfirmationEmail - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result.CloseResult();
        }

        public class ConfirmEmailModel
        {
            public string token { get; set; } = "";
        }
        /// <summary>
        /// This is going to be called in a script on the confirming email page
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ConfirmEmail")]
        public async Task<string> ConfirmEmail([FromBody] ConfirmEmailModel model)
        {
            var result = new BaseResult { };
            try
            {
                //Check if this user exists
                var sessionInfo = this.GetSessionInfo();
                if (sessionInfo != null)
                {
                    //Check if this user is confirmed
                    if (!sessionInfo.user.email_confirmed)
                    {
                        var user = await _database.FirstOrDefaultAsync<AppUser>($"id = '{sessionInfo.user.id}'");
                        var confirmationResult = await _userManager.ConfirmEmailAsync(user, model.token);
                        result.exception = confirmationResult.Succeeded ? null : new Exception("Confirmation failed: " + JsonConvert.SerializeObject(confirmationResult.Errors));
                        if (confirmationResult.Succeeded)
                        {

                            user.email_confirmed = true;
                            sessionInfo.user = user;
                            this.SetSessionInfo(sessionInfo);
                            result.successResult = "Email confirmed";
                        }
                        else
                        {
                            result.errorResult = "Confirmation Failed";
                        }
                    }
                    else
                    {
                        result.successResult = "User is already confirmed";
                    }
                }
                else
                {
                    result.errorResult = "User with that email does not exist";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - ConfirmEmail - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            var test = result.CloseResult();
            return result.CloseResult();
        }

        public class SendResetPasswordEmailModel
        {
            public string email { get; set; } = "";
        }
        //[AllowAnonymous]
        [HttpPost]
        [Route("SendResetPasswordEmail")]
        public async Task<string> SendResetPasswordEmail([FromBody] SendResetPasswordEmailModel model)
        {
            var result = new BaseResult { };
            try
            {
                var user = await _database.FirstOrDefaultAsync<AppUser>($"email = '{model.email.ToUpper()}'");
                if(user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var emailList = new List<EmailAddress>() { new EmailAddress() { Email = model.email } };
                    var emailResult = await _emailService.SendResetPasswordEmail(emailList, token);
                    if (emailResult != null)
                    {
                        if (emailResult.exception == null)
                        {
                            result.successResult = "Email has been sent";
                            result.exception = emailResult.exception;
                        }
                        else
                        {
                            result.errorResult = "Email could not be sent: " + emailResult.exception;
                        }
                    }
                    else
                    {
                        result.errorResult = "Email could not be sent";
                    }
                }
                else
                {
                    result.errorResult = "User with that email does not exist";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - SendResetPasswordEmail - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result.CloseResult();
        }

        public class ResetPasswordModel
        {
            public string email { get; set; } = "";
            public string password { get; set; } = "";
            public string token { get; set; } = "";
        }
        /// <summary>
        /// This is going to be called in a script on the resting page email page
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        //[Authorize(Policy = "shopper, admin")]
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<string> ResetPassword([FromBody]ResetPasswordModel model)
        {
            var result = new BaseResult { };
            try
            {
                //Check if this user exists
                var user = await _database.FirstOrDefaultAsync<AppUser>($"email = '{model.email.ToUpper()}'");
                if (user != null)
                {
                    var confirmationResult = await _userManager.ResetPasswordAsync(user, model.token, model.password);
                    result.exception = confirmationResult.Succeeded ? null : new Exception("Reset Password Failed: " + JsonConvert.SerializeObject(confirmationResult.Errors));
                    if (confirmationResult.Succeeded)
                    {
                        result.successResult = "Password reset confirmed";
                    }
                    else
                    {
                        result.errorResult = "Password reset failed";
                    }
                }
                else
                {
                    result.errorResult = "User with that email does not exist";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - SendResetPasswordEmail - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result.CloseResult();
        }

        //[Authorize(Policy = "shopper, admin")]
        [HttpPost]
        [Route("Logout")]
        public async Task<string> Logout()
        {
            var result = new DataResult<SessionInfo> { };
            try
            {
                //Check if this user exists
                var sessionInfo = this.GetSessionInfo();
                if(sessionInfo != null)
                {
                    sessionInfo.claims = new List<SingleClaim>();
                    sessionInfo.roles = new List<SingleRole>();
                    sessionInfo.user = null;
                    this.SetSessionInfo(sessionInfo);
                    result.data = sessionInfo;
                    result.successResult = "Logging Out...";
                }
                else
                {
                    result.errorResult = "Session never existed";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auth Controller - Logout - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result.CloseResult();
        }

        public static bool IsValidEmail(string email)
        {
            // Email validation pattern
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Check if the email matches the pattern
            bool isValid = Regex.IsMatch(email, pattern);

            return isValid;
        }

    }
}
