using System;
using System.Collections.Concurrent;
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
        private const int MaxAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(60);

        // ip -> (failCount, lockoutUntil)
        private static readonly ConcurrentDictionary<string, (int Count, DateTime LockedUntil)> _loginAttempts = new();

        public HomeController(ILogger<HomeController> logger, ISpendRepository repository, IConfiguration configuration)
        {
            _logger = logger;
            _repository = repository;
            _sitePassword = configuration["SitePassword"] ?? "";
        }

        private bool IsAuthenticated() =>
            Request.Cookies.TryGetValue(AuthCookie, out var val) && val == _sitePassword;

        private string ClientIp() =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login");
            return View(await _repository.Get());
        }

        [HttpGet]
        public IActionResult Login()
        {
            var ip = ClientIp();
            if (_loginAttempts.TryGetValue(ip, out var state) && DateTime.UtcNow < state.LockedUntil)
            {
                var seconds = (int)(state.LockedUntil - DateTime.UtcNow).TotalSeconds;
                ViewBag.Error = $"Too many failed attempts. Try again in {seconds}s.";
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string password)
        {
            var ip = ClientIp();

            if (_loginAttempts.TryGetValue(ip, out var state) && DateTime.UtcNow < state.LockedUntil)
            {
                var seconds = (int)(state.LockedUntil - DateTime.UtcNow).TotalSeconds;
                ViewBag.Error = $"Too many failed attempts. Try again in {seconds}s.";
                return View();
            }

            if (password == _sitePassword)
            {
                _loginAttempts.TryRemove(ip, out _);
                Response.Cookies.Append(AuthCookie, _sitePassword, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                });
                return RedirectToAction("Index");
            }

            // Record failed attempt
            _loginAttempts.AddOrUpdate(ip,
                (1, DateTime.MinValue),
                (_, prev) =>
                {
                    var newCount = prev.Count + 1;
                    var lockedUntil = newCount >= MaxAttempts ? DateTime.UtcNow.Add(LockoutDuration) : prev.LockedUntil;
                    return (newCount, lockedUntil);
                });

            _loginAttempts.TryGetValue(ip, out var current);
            var remaining = MaxAttempts - current.Count;
            ViewBag.Error = remaining > 0
                ? $"Incorrect password. {remaining} attempt{(remaining == 1 ? "" : "s")} remaining."
                : $"Too many failed attempts. Try again in {(int)LockoutDuration.TotalSeconds}s.";

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
