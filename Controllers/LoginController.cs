using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CaffeBot.Controllers
{
    public class LoginController : Controller
    {

        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationContext _context;

        public LoginController(ILogger<LoginController> logger, IConfiguration configuration, ApplicationContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }
        public IActionResult Denied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Auth()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Auth(string Login, string Password)
        {
            _logger.LogCritical($"Login try: {Login}; Time: {DateTime.Now}");
            CaffeBot.Entities.Account? account = null;

            try {
                account = await _context.Accounts.FirstOrDefaultAsync(acc =>
                    acc.Login == Login && acc.Password == Password);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Database error while attenmping to sign in.\nError message: {ex.Message};");
            }

            if (account is null) { 
                _logger.LogCritical($"Login fail: {Login}; Time: {DateTime.Now}");
                ViewBag.Error = "Ошибка авторизации";
                return View();
            }

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, account.Login));

            switch (account.Role)
            {
                case CaffeBot.Entities.AccountRole.Developer:
                    claims.Add(new Claim(ClaimTypes.Role, "Developer"));
                    goto case CaffeBot.Entities.AccountRole.Head;
                case CaffeBot.Entities.AccountRole.Head:
                    claims.Add(new Claim(ClaimTypes.Role, "Head"));
                    goto case CaffeBot.Entities.AccountRole.Admin;
                case CaffeBot.Entities.AccountRole.Admin:
                    claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                    goto case CaffeBot.Entities.AccountRole.Common;
                case CaffeBot.Entities.AccountRole.Common:
                    claims.Add(new Claim(ClaimTypes.Role, "Common"));
                    break;
            }

            var identity = new ClaimsIdentity(claims, "Cookie");
            var principal = new ClaimsPrincipal(identity);

            try { 
                await HttpContext.SignInAsync("Cookie", principal);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error. Login: {account.Login}; Error message:\n{ex.Message};");
            }

            _logger.LogCritical($"Login success: {account.Login}; Role: {account.Role}; Time: {DateTime.Now}");
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> LogOut()
        {
            try
            {
                await HttpContext.SignOutAsync("Cookie");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sign out error. Error message:\n{ex.Message};");
            }
            return RedirectToAction("Auth", "Login");
        }

    }
}
