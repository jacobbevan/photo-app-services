using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using photo_api.Models;
using photo_api.Services;

namespace photo_api.Controllers
{
    [Route("")]
    public class HealthController : Controller
    {
        [HttpGet()]
        public IActionResult ConfirmRunning()
        {
            return Ok();
        }
    }
}
