using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia1.Data;
using SocialMedia1.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace SocialMedia1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataBaseContext _context;

        public HomeController(ILogger<HomeController> logger, DataBaseContext context)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            return RedirectToAction("Account", "Home");
        }

        [HttpGet]
        public IActionResult SignIn() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(string Email, string Password)
        {
            LoginModel login = _context.LoginModel
                .FirstOrDefault(acc => acc.Email == Email && acc.Password == Password);

            if (login != null)
            {
                _logger.Log(LogLevel.Information, "found account for email: " + Email);
                await Authenticate(Email);
                return RedirectToAction("Account", "Home");
            }
            _logger.Log(LogLevel.Error, "couldn't find account for email: " + Email);
            return View();
        }
        [HttpGet]
        public IActionResult SignUp() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(LoginModel login, string Name)
        {
            if (login.Email != null)
            {
                _logger.Log(LogLevel.Information, "creating account for email: " + login.Email);
                Account account = new Account();
                account.Name = Name;
                _context.Account.Add(account);
                _context.SaveChanges();
                login.AccountId = account.Id;
                _context.LoginModel.Add(login);
                _context.SaveChanges();
                Authenticate(login.Email);
                return RedirectToAction("Account", "Home");
            }
            return View();
        }

        [Authorize]
        public IActionResult Account()
        {
            LoginModel login = _context.LoginModel
                .FirstOrDefault(acc => acc.Email == User.Identity.Name);
            if (login != null)
            {
                Account account = _context.Account
                    .FirstOrDefault(acc => acc.Id == login.AccountId);
                _logger.Log(LogLevel.Information, "enter account for " + account.Name);
                ViewData["Name"] = account.Name;
                ViewData["Email"] = login.Email;
            }
            else
            {
                _logger.Log(LogLevel.Error, "couldn't find account for email: " + User.Identity.Name);
            }
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Friends() { return View(); }

        [Authorize]
        [HttpGet]
        public IActionResult Chats() { return View(); }

        private async Task Authenticate(string email)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, email)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}
