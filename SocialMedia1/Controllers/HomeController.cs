using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia1.Data;
using SocialMedia1.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace SocialMedia1.Controllers {
    public class HomeController: Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly DataBaseContext _context;

        public HomeController(ILogger<HomeController> logger, DataBaseContext context) {
            _context = context;
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index() {
            return RedirectToAction("Account", "Home");
        }

        [HttpGet]
        public IActionResult SignIn() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(string Email, string Password) {
            LoginModel login = _context.LoginModel
                .FirstOrDefault(acc => acc.Email == Email && acc.Password == Password);

            if (login != null) {
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
        public IActionResult SignUp(LoginModel login, string Name, string LastName) {
            if (login.Email != null) {
                _logger.Log(LogLevel.Information, "creating account for email: " + login.Email);
                Account account = new Account(Name, LastName);
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
        public IActionResult Account(int? id) {
            if (id.HasValue) {
                return GetAccount(id.Value);
            } else {
                LoginModel login = _context.LoginModel
                    .FirstOrDefault(acc => acc.Email == User.Identity.Name);
                return GetAccount(login.AccountId);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult EditAccount() {
            int selfAccId = _context.GetSelfAccId();
            var account = _context.Account.FirstOrDefault(acc => acc.Id == selfAccId);
            ProfilePhotoSetter.SetPhotoOrDefault(ref account);
            ViewData["Name"] = account.Name;
            ViewData["LastName"] = account.LastName;
            ViewData["Location"] = account.Location;
            ViewData["Photo"] = account.ProfilePhoto;
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult EditAccount(Account newAccountData, IFormFile profilePhoto, bool deletePhoto) {
            _logger.Log(LogLevel.Information, "to delete is " + deletePhoto);
            int selfAccId = _context.GetSelfAccId();
            var account = _context.Account.Find(selfAccId);
            ProfilePhotoSetter.SetPhotoOrDefault(ref account);
            account.Name = newAccountData.Name;
            account.LastName= newAccountData.LastName;
            account.Location = newAccountData.Location;
            if (deletePhoto) {
                account.ProfilePhoto = null;
            }
            if (profilePhoto != null) {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(profilePhoto.OpenReadStream())) {
                    imageData = binaryReader.ReadBytes((int)profilePhoto.Length);
                }
                account.ProfilePhoto = imageData;
            }
            try {
                _context.SaveChanges();
            } catch {
                ProfilePhotoSetter.SetPhotoOrDefault(ref newAccountData);
                ViewData["Name"] = newAccountData.Name;
                ViewData["LastName"] = newAccountData.LastName;
                ViewData["Location"] = newAccountData.Location;
                ViewData["Photo"] = newAccountData.ProfilePhoto;
                return View(newAccountData);
            }
            return RedirectToAction("Account");
        }

        private IActionResult GetAccount(int id) {
            Account account = _context.Account
                    .FirstOrDefault(acc => acc.Id == id);
            if (account != null) {
                ProfilePhotoSetter.SetPhotoOrDefault(ref account);
                _logger.Log(LogLevel.Information, "enter account for " + account.Name);
                ViewData["Name"] = account.Name;
                ViewData["LastName"] = account.LastName;
                ViewData["Location"] = account.Location;
                ViewData["Photo"] = account.ProfilePhoto;
            } else {
                _logger.Log(LogLevel.Error, "couldn't find account with id: " + id);
            }
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Friends() { return View(); }

        [Authorize]
        [HttpGet]
        public IActionResult Chats() { return View(); }

        private async Task Authenticate(string email) {
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
