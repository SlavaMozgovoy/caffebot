using CaffeBot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace CaffeBot.Controllers
{
    [Authorize("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<UserController> _logger;
        public UserController(ApplicationContext context, ITelegramBotClient bot, ILogger<UserController> logger)
        {
            _context = context;
            _bot = bot;
            _logger = logger;
        }
        public async Task<IActionResult> Users()
        {
            var users = await _context.Profiles.ToListAsync();
            return View(users);
        }
        [Authorize("Developer")]
        public async Task<IActionResult> ChangeNotifyStatus(long Id)
        {
            var user = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId == Id);
            user.Notified = !user.Notified;
            await _context.SaveChangesAsync();
            return RedirectToAction("UserProfile", "User", new { Id = Id });    
        }
        public async Task<IActionResult> UnblockUser(long Id)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId == Id);
            profile.IsBlocked = false;
            await _bot.SendTextMessageAsync(profile.ChatId, "Ваш профиль был разблокирован");
            await _context.SaveChangesAsync();
            return RedirectToAction("UserProfile", "User", new { Id = Id });
        }
        [Authorize("Developer")]
        public async Task<IActionResult> FixalChangeNotify(long Id)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId == Id);
            profile.IsAdmin = !profile.IsAdmin;
            await _context.SaveChangesAsync();
            return RedirectToAction("UserProfile", "User", new { Id = Id });
        }
        public async Task<IActionResult> BlockUser(long Id, string Reason)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId == Id);
            profile.IsBlocked = true;
            await _bot.SendTextMessageAsync(profile.ChatId, "Ваш профиль был заблокирован. Причина: " + Reason);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserProfile", "User", new {Id = Id});
        }
        public async Task<IActionResult> UserProfile(long Id)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId == Id);
            ViewBag.Orders = await _context.Orders
                .Where(o => o.ProfileId == profile.ProfileId && o.IsConfirmed == Entities.OrderStatus.CONFIRMED)
                .ToListAsync();
            return View(profile);
        }

        public async Task<IActionResult> SendPrivateMessage(Message message, string ReturnUrl)
        {
            if (message.Text == null && message.File == null)
            {
                TempData["Error"] = "Ошибка отправки сообщения";
                return Redirect(ReturnUrl);
            }
            try 
            { 
                if (message.File == null)
                    await _bot.SendTextMessageAsync(message.ChatId, message.Text ?? string.Empty);
                else
                {
                    using MemoryStream ms = new MemoryStream();
                    message.File.CopyTo(ms);
                    ms.Position = 0;
                    InputOnlineFile image = new InputOnlineFile(ms);
                    await _bot.SendPhotoAsync(message.ChatId, image, message.Text ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData["Error"] = "Ошибка отправки сообщения";
            }
            return Redirect(ReturnUrl);
        }
    }
}
