using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GIGLite.Auth.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {

        }
        [HttpGet("home/index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}