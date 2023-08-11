using Microsoft.AspNetCore.Mvc;

namespace MotorClaims.Controllers
{
    public class ClaimsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
