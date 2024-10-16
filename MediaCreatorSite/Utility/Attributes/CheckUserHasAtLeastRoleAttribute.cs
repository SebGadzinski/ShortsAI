using MediaCreatorSite.Models;
using MediaCreatorSite.Utility.Constants;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using MediaCreatorSite.Utility.Extensions;
using MediaCreatorSite.Utility.Results;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;

namespace MediaCreatorSite.Utility.Attributes
{
    public class CheckUserRoleAttribute : ActionFilterAttribute
    {
        private readonly string _requiredRole;
        private readonly IAttributeLogic _attributeLogic;

        public CheckUserRoleAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
            _attributeLogic = new AttributeLogic();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var result = new BaseResult();
            try
            {
                var session = context.HttpContext.Session;

                context.HttpContext.Items["SessionInfo"] = _attributeLogic.CheckUserHasAtLeastRoleLogic(ref session, _requiredRole);


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
