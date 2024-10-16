using MediaCreatorSite.DataAccess.Constants;
using MediaCreatorSite.Models;
using MediaCreatorSite.Utility.Constants;
using MediaCreatorSite.Utility.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MediaCreatorSite.Utility.Extensions
{
    public static class ControllerExtensions
    {
        public static bool CheckForRole(this ControllerBase controller, HashSet<string> acceptableRoles, BaseResult result)
        {
            var sessionInfo = controller.HttpContext.Session.Get<SessionInfo>(SessionKeys.SESSION_USER_KEY);
            if (sessionInfo == null)
            {
                result.errorResult = "Session does not exist";
                return false;
            }
            if (!sessionInfo.roles.Any(x => acceptableRoles.Contains(x.name)))
            {
                result.errorResult = "User does not have access to this functionality";
                result.status = 3;
                return false;
            }
            return true;
        }
        public static bool CheckForRole(this ControllerBase controller, HashSet<string> acceptableRoles)
        {
            var sessionInfo = controller.HttpContext.Session.Get<SessionInfo>(SessionKeys.SESSION_USER_KEY);
            if (sessionInfo == null)
            {
                return false;
            }
            if (!sessionInfo.roles.Any(x => acceptableRoles.Contains(x.name)))
            {
                return false;
            }
            return true;
        }
        public static SessionInfo? GetSessionInfo(this ControllerBase controller)
        {
            return controller.HttpContext.Session.Get<SessionInfo>(SessionKeys.SESSION_USER_KEY);
        }
        public static void SetSessionInfo(this ControllerBase controller, SessionInfo sessionInfo)
        {
            controller.HttpContext.Session.Set(SessionKeys.SESSION_USER_KEY, sessionInfo);
        }
        public static string GetUserEmail(this ControllerBase controller)
        {
            try
            {
                var sessionInfo = controller.HttpContext.Session.Get<SessionInfo>(SessionKeys.SESSION_USER_KEY);
                return sessionInfo.user != null ? sessionInfo.user.email : "Unknown";
            }
            catch(Exception ex)
            {
                return "Unkown";
            }
        }
        public static T EmailVerification<T>(this ControllerBase controller, T result) where T : BaseResult
        {
            var sessionInfo = controller.HttpContext.Session.Get<SessionInfo>(SessionKeys.SESSION_USER_KEY);
            if (sessionInfo == null)
            {
                result.errorResult = "Session does not exist";
            }
            if (!sessionInfo.roles.Any(x => Roles.AuthorizeShopperRoles.Contains(x.name)))
            {
                result.errorResult = "User does not have access to this functionality";
            }
            if(sessionInfo.user == null)
            {
                result.errorResult = "Session does not have user";
            }
            if (!sessionInfo.user.email_confirmed)
            {
                result.errorResult = "Please Confirm Your Email!";
                result.status = 3;
            }

            return result;
        }
        public static Guid? GetUserId(this ControllerBase controller)
        {
            try
            {
                var sessionInfo = controller.HttpContext.Session.Get<SessionInfo>(SessionKeys.SESSION_USER_KEY);
                return sessionInfo.user != null ? sessionInfo.user.id : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
