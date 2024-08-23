using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia1.Data;
using SocialMedia1.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SocialMedia1.Validators;


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

        public IActionResult LogOut() {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Account(int? id) {
            if (id.HasValue) {
                return GetAccount(id.Value);
            } 
            LoginModel login = _context.LoginModel
                .FirstOrDefault(acc => acc.Email == User.Identity.Name);
            return GetAccount(login.AccountId);
        }

        [Authorize]
        [HttpGet]
        public IActionResult EditAccount() {
            int selfAccId = _context.GetSelfAccId();
            var account = _context.Account.FirstOrDefault(acc => acc.Id == selfAccId);
            account.SetPhotoOrDefault();
            return View(new EditAccountModel(account));
        }

        [Authorize]
        [HttpPost]
        public IActionResult EditAccount(EditAccountModel data) {
            if (!ModelState.IsValid) {
                data.AccountData.SetPhotoOrDefault();
                return View(data);
            }
            _logger.Log(LogLevel.Information, "to delete is " + data.DeletePhoto);
            int selfAccId = _context.GetSelfAccId();
            var account = _context.Account.Find(selfAccId);
            account.SetPhotoOrDefault();
            account.Name = data.AccountData.Name;
            account.LastName= data.AccountData.LastName;
            account.Location = data.AccountData.Location;
            if (data.DeletePhoto.HasValue && data.DeletePhoto.Value) {
                account.ProfilePhoto = null;
            }
            if (data.ProfilePhoto != null) {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(data.ProfilePhoto.OpenReadStream())) {
                    imageData = binaryReader.ReadBytes((int)data.ProfilePhoto.Length);
                }
                account.ProfilePhoto = imageData;
            }
            try {
                _context.SaveChanges();
            } catch {
                return View(data);
            }
            return RedirectToAction("Account");
        }

        private IActionResult GetAccount(int id) {
            Account account = _context.Account
                    .FirstOrDefault(acc => acc.Id == id);
            if (account != null) {
                account.SetPhotoOrDefault();
                _logger.Log(LogLevel.Information, "enter account for " + account.Name);
            } else {
                _logger.Log(LogLevel.Error, "couldn't find account with id: " + id);
            }
            return View(account);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Posts() { return View(); }

        [Authorize]
        [HttpGet]
        public IActionResult Friends() { return View(); }

        [Authorize]
        [HttpGet]
        public IActionResult Chats() { return View(); }

        private async Task Authenticate(string email) {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, email)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }

    public class EditAccountModel {
        public Account AccountData { get; set; }
        [MaxFileSize(GlobalVariebles.maxFileSize)]
        public IFormFile? ProfilePhoto { get; set; }
        public bool? DeletePhoto { get; set; }

        public EditAccountModel() { }

        public EditAccountModel(Account accountData) {
            AccountData = accountData;
        }

    }
}
