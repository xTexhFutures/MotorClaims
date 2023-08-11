using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using CORE.DTOs.APIs.Authenticator;

namespace MotorClaims.Models
{
    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {
        public string Roles { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!string.IsNullOrEmpty(Roles))
            {

                //if user isn't logged in.
                //LoginObj User = filterContext.HttpContext.Session.getSessionData<LoginObj>("loginObj");
                //string[] RolesList = Roles.Split(',');
                //bool access = false;
                //foreach (string Role in RolesList)
                //{
                //    if (User.Roles.Name.ToString().ToUpper() == Role.ToUpper())
                //    {
                //        access = true;
                //        break;
                //    }
                //    //Check user rights here

                //}
                //if (!access)
                //{
                //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                //    {
                //        controller = "Authenticator",
                //        action = "UnAuthorized"
                //    }));
                //}


            }
        }
    }
}
