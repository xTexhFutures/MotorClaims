using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MotorClaims.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguagesController : ControllerBase
    {

        private readonly IStringLocalizer<LanguagesController> language;
        public LanguagesController(IStringLocalizer<LanguagesController> language)
        {
            this.language = language;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(language["langs"].Value);
        }

    }
}
