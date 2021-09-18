using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OF.Auth.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace OF.Infrastructure.Auth
{
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        private readonly string[] requiredRoles;
        /// <summary>
        /// 
        /// </summary>
        public RequireLoginAttribute()
        {
            requiredRoles = new string[] { };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roles">A comma delimited list of RoleCodes allowed on this endpoint</param>
        public RequireLoginAttribute(string roles)
        {
            requiredRoles = roles.ToUpper().Split(',');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Items.ContainsKey("User"))
            {
                filterContext.Result = new ContentResult() { Content = "Request is not authorized. Login required", StatusCode = (int)HttpStatusCode.Unauthorized };
                return;
            }
            var usr = (IUser)filterContext.HttpContext.Items["User"];

            bool hsrole = (usr.UserRoles.Intersect(requiredRoles.Distinct()).ToList().Distinct().Count() == requiredRoles.Length);

            if (!hsrole) { filterContext.Result = new ContentResult() { Content = "Insufficent Permissions", StatusCode = (int)HttpStatusCode.Unauthorized }; };
        }
    }

}
