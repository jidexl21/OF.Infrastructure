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
    public class MachineOnlyAttribute : ActionFilterAttribute
    {
        private readonly IAuthSettings settings; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roles">A comma delimited list of RoleCodes allowed on this endpoint</param>
        public MachineOnlyAttribute(IAuthSettings settings)
        {
            this.settings = settings;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Items.ContainsKey("Machine"))
            {
                filterContext.Result = new ContentResult() { Content = "Request failed", StatusCode = (int)HttpStatusCode.Unauthorized };
                return;
            }
            var usr = (AppUser) filterContext.HttpContext.Items["Machine"];
            bool hsrole = (settings.AppID == usr.UserName);
                //(usr.UserRoles.Select(x => x.Code).Intersect(r/*equiredRoles.Distinct()*/).ToList().Distinct().Count() == requiredRoles.Length);

            if (!hsrole) { filterContext.Result = new ContentResult() { Content = "Insufficent Permissions", StatusCode = (int) HttpStatusCode.Unauthorized }; };
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
