using Google.Api;
using MediaCreatorSite.Models;
using MediaCreatorSite.Utility.Constants;
using MediaCreatorSite.Utility.Exceptions;
using MediaCreatorSite.Utility.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MediaCreatorSite.Utility.Attributes
{
    public interface IAttributeLogic
    {
        SessionInfo CheckUserHasAtLeastRoleLogic(ref ISession session, string requiredRole);
        SessionInfo EmailVerifiedLogic(ref ISession session);
        SessionInfo IsNotScaryLogic(SessionInfo sessionInfo);
    }

    public class AttributeLogic : IAttributeLogic
    {

        public SessionInfo CheckUserHasAtLeastRoleLogic(ref ISession session, string requiredRole)
        {
            var sessionInfo = session.Get<SessionInfo>(SessionKeys.SESSION_USER_KEY);

            if (sessionInfo?.user?.id == null) throw new UserIdDoesNotExistException();

            // Check if the user has the required role
            if (!sessionInfo.roles.Exists(role => role.name == requiredRole)) throw new PermissionDeniedExeption();

            return sessionInfo;
        }

        public SessionInfo EmailVerifiedLogic(ref ISession session)
        {
            var sessionInfo = session.Get<SessionInfo>(SessionKeys.SESSION_USER_KEY);

            if (sessionInfo == null) throw new SessionDoesNotExistException();

            if (sessionInfo?.user?.id == null) throw new UserIdDoesNotExistException();

            if (!(sessionInfo?.user?.email_confirmed == true)) throw new EmailNotConfirmedException();

            return sessionInfo;
        }

        public SessionInfo IsNotScaryLogic(SessionInfo sessionInfo)
        {
            //Using this session data, check to see if this user has been tryung to hack into our systems or screw with the program

            return sessionInfo;
        }

    }
}
