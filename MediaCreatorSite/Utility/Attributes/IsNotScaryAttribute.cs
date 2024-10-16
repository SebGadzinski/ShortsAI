using MediaCreatorSite.Models;
using MediaCreatorSite.Utility.Constants;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using MediaCreatorSite.Utility.Extensions;
using MediaCreatorSite.Utility.Results;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;
using MediaCreatorSite.DataAccess.Constants;
using MediaCreatorSite.Utility.Exceptions;

namespace MediaCreatorSite.Utility.Attributes
{
    public class IsNotScaryAttribute : ActionFilterAttribute
    {
        private readonly IAttributeLogic _attributeLogic = new AttributeLogic();

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var result = new BaseResult();
            try
            {
                var session = context.HttpContext.Session;

                var sessionInfo = _attributeLogic.EmailVerifiedLogic(ref session);
                context.HttpContext.Items["SessionInfo"] = sessionInfo;
                _attributeLogic.IsNotScaryLogic(sessionInfo);

                base.OnActionExecuting(context);
            }
            catch (Exception ex)
            {
                result.exception = ex;
                result.CloseResult();
                context.Result = new ObjectResult(result);
            }
        }
    }
}
