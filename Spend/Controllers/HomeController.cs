using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spend.Data;
using Spend.Models;

namespace Spend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISpendRepository _repository;
        private readonly string _sitePassword;

        private const string AuthCookie = "SpendAuth";

        public HomeController(ILogger<HomeController> logger, ISpendRepository repository, IConfiguration configuration)
        {
            _logger = logger;
            _repository = repository;
            _sitePassword = configuration["SitePassword"] ?? "";
        }

        private bool IsAuthenticated() =>
            Request.Cookies.TryGetValue(AuthCookie, out var val) && val == _sitePassword;

        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login");
            return View(await _repository.Get());
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string password)
        {
            if (password == _sitePassword)
            {
                Response.Cookies.Append(AuthCookie, _sitePassword, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                });
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Incorrect password.";
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete(AuthCookie);
            return RedirectToAction("Login");
        }

        public ActionResult Create()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login");
            return View();
        }

        // POST api/
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Entry entry)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login");
            entry.Entered = DateTime.Now;
            await _repository.Create(entry);
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
