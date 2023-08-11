using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using CORE.DTOs.Authentications;

namespace MotorClaims.Models
{
    public class LevelSecurity : ActionFilterAttribute
    {
        private readonly AppSettings _appSettings;
        public LevelSecurity(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        public override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
        {
            //string name = (string)actionExecutingContext.RouteData.Values["Controller"];
            //if (name != "Authenticator" && name != "Payment" && name != "Managing")
            //{
            //    Users users = new Users();

            //    users = actionExecutingContext.HttpContext.Session.getSessionData<Users>("LoggedUser");
            //    if (users == null)
            //    {
            //        actionExecutingContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            //        {
            //            controller = "Authenticator",
            //            action = "Login"
            //        }));
            //    }

            //}
            //else
            //{
            //    actionExecutingContext.HttpContext.Session.SetSessionData("CopyRight", _appSettings.CopyRight);
            //}
        }
    }
}
