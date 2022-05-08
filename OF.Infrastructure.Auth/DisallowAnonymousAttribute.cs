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
    public class DisallowAnonymousAttribute : ActionFilterAttribute
    {
        private readonly string[] requiredRoles;
        /// <summary>
        /// 
        /// </summary>
        public DisallowAnonymousAttribute()
        {
            requiredRoles = new string[] { };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roles">A comma delimited list of RoleCodes allowed on this endpoint</param>
        public DisallowAnonymousAttribute(string roles)
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
            var usr = (AppUser) filterContext.HttpContext.Items["User"];

            bool hsrole = (usr.UserRoles.Select(x => x.Code).Intersect(requiredRoles.Distinct()).ToList().Distinct().Count() == requiredRoles.Length);

            if (!hsrole) { filterContext.Result = new ContentResult() { Content = "Insufficent Permissions", StatusCode = (int)HttpStatusCode.Forbidden }; };
            //if (filterContext.HttpContext.Session != null)
            //{
            //    //var user = filterContext.HttpContext.Session["User"] as User;
            //    //if (user != null && string.IsNullOrEmpty(user.FirstName))
            //    //    filterContext.Result = new RedirectResult("/home/firstname");
            //    //else
            //    //{
            //    //    //what ever you want, or nothing at all
            //    //}
            //}
        }
    }

}
