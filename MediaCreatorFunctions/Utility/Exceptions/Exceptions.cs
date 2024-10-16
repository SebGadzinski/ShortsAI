using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Utility.Exceptions
{
    public class NoScriptDataException : Exception { public NoScriptDataException() : base("Script could not generate") { } }
    public class NoMusicException : Exception { public NoMusicException() : base("No Music Found On Device") { } }
    public class NoMusicTagsException : Exception { public NoMusicTagsException(int id) : base($"No music tags found for script with id {id}") { } }
    public class ObjectDoesNotExistException : Exception { public ObjectDoesNotExistException(string dbObject, object id) : base($"{dbObject}:{id} does not exist") { } }
    public class CantFindTopicException : Exception { public CantFindTopicException(string category, int tries) : base($"Cannot find topic for category '{category}' after {tries}") { } }
    public class WasteMoneyException : Exception { public WasteMoneyException(string wasteReason) : base($"This could cost alot of money, please ensure you want this and uncomment my code block. Waste Reason: {wasteReason}") { } }
    public class UserAlreadyExistsException : Exception { public UserAlreadyExistsException(string message) : base(message) { } }
    public class ConfirmedPasswordNotMatchingException : Exception { public ConfirmedPasswordNotMatchingException(string message) : base(message) { } }
    public class DimensionsNotOkException : Exception { public DimensionsNotOkException() : base("Dimensions must be between between 128 and 1536") { } }
    public class EmailNotConfirmedException : Exception { public EmailNotConfirmedException() : base("Email is not confirmed") { } }
    public class SessionDoesNotExistException : Exception { public SessionDoesNotExistException() : base("Session does not exist") { } }
    public class UserIdDoesNotExistException : Exception { public UserIdDoesNotExistException() : base("User ID does not exist") { } }
    public class PermissionDeniedExeption : Exception { public PermissionDeniedExeption() : base("User does not have permissions to access") { } }
    public class EmailNotOkException : Exception { public EmailNotOkException() : base("Email is not ok") { } }
    public class PaymentFailedException : Exception { public PaymentFailedException(string message) : base($"Payment Failed: {message}") { } }
    public class NotEnoughCreditsException : Exception { public NotEnoughCreditsException() : base($"Not Enough Credits") { } }

    public static class ExceptionCode
    {
        public static int NoScriptDataExceptionCode = 101;
        public static int NoMusicExceptionCode = 102;
        public static int NoMusicTagsExceptionCode = 103;
        public static int ObjectDoesNotExistExceptionCode = 104;
        public static int CantFindTopicExceptionCode = 105;
        public static int WasteMoneyExceptionCode = 106;
        public static int UserAlreadyExistsException = 107;
        public static int ConfirmedPasswordNotMatchingException = 108;
        public static int DimensionsNotOkException = 109;
        public static int EmailNotConfirmedException = 110;
        public static int SessionDoesNotExistException = 111;
        public static int UserIdDoesNotExistExceptionCode = 112;
        public static int PermissionDeniedExeptionCode = 113;
        public static int EmailNotOkExceptionCode = 114;
        public static int PaymentFailedExceptionCode = 115;
        public static int NotEnoughCreditsExceptionCode = 116;

        public static Dictionary<string, int> ExceptionCodes = new Dictionary<string, int>()
            {
                { "NoScriptDataException", NoScriptDataExceptionCode},
                { "NoMusicException", NoMusicExceptionCode},
                { "NoMusicTagsException", NoMusicTagsExceptionCode},
                { "ObjectDoesNotExistException", ObjectDoesNotExistExceptionCode},
                { "CantFindTopicException", CantFindTopicExceptionCode},
                { "WasteMoneyException", WasteMoneyExceptionCode},
                { "UserAlreadyExistsException", UserAlreadyExistsException},
                { "ConfirmedPasswordNotMatchingException", ConfirmedPasswordNotMatchingException},
                { "DimensionsNotOkException", DimensionsNotOkException},
                { "EmailNotConfirmedException", EmailNotConfirmedException},
                { "SessionDoesNotExistException", SessionDoesNotExistException},
                { "UserIdDoesNotExistException", UserIdDoesNotExistExceptionCode},
                { "PermissionDeniedExeption", PermissionDeniedExeptionCode},
                { "EmailNotOkException", EmailNotOkExceptionCode},
                { "PaymentFailedException", PaymentFailedExceptionCode},
                { "NotEnoughCreditsException", NotEnoughCreditsExceptionCode},
            };
    }
}
