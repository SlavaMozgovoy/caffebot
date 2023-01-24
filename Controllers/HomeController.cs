using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CaffeBot.Models;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.InputFiles;
using CaffeBot.Entities;
using Microsoft.AspNetCore.Diagnostics;

namespace CaffeBot.Controllers
{
    [Authorize("Common")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationContext _context;
        private readonly ITelegramBotClient _bot;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _host;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ApplicationContext context, ITelegramBotClient bot, Microsoft.AspNetCore.Hosting.IHostingEnvironment host)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _bot = bot;
            _host = host;
        }

        public IActionResult Index()
        {
           return View();
        }
        [Authorize("Head")]
        public async Task<IActionResult> AddPromo(Message message)
        {
            Promotion promotion = new Promotion();
            if (message.File != null) { 
                string imgPath = _host.WebRootPath + "/PicturesNk/" + message.GetHashCode() + ".jpg";
                using FileStream fs = new FileStream(imgPath, FileMode.Create);
                await message.File.CopyToAsync(fs);
                promotion.ImagePath = "/PicturesNk/" + message.GetHashCode() + ".jpg";
            }
            promotion.Description = message.Text;

            _context.Update(promotion);
            await _context.SaveChangesAsync();

            return RedirectToAction("Spam");
        }

        [Authorize("Head")]
        public async Task<IActionResult> RemovePromo(long Id)
        {
            var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == Id);
            if (promo != null)
            { 
                _context.Promotions.Remove(promo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Spam");
        }

        [Authorize("Head")]
        public async Task<IActionResult> Spam()
        {
            var promo = await _context.Promotions.ToListAsync();
            ViewBag.Promotions = promo;
            return View();
        }

        [HttpPost]
        [Authorize("Head")]
        public async Task<IActionResult> SendMessage(Message message, string ReturnUrl)
        {
            if (message?.Text is null && message?.File is null)
            {
                TempData["Error"] = "Отправка сообщений прервалась";
                return Redirect(ReturnUrl);
            }

            _logger.LogInformation("Start Spam Sedning: " + DateTime.Now.ToString());
            
            MemoryStream stream = new MemoryStream();

            if (message.File is not null) { 
                await message.File.CopyToAsync(stream);
            }

            InputOnlineFile file = null;

            if (stream.Length > 0)
            {
                file = new InputOnlineFile(stream);
            }

            var profiles = await _context.Profiles.AsNoTracking().Where(p => p.Subscribed).ToListAsync();

            foreach (var p in profiles)
            {
                try 
                { 
                    if (file is null) 
                    { 
                        await _bot.SendTextMessageAsync(p.ChatId, message.Text);
                    }
                    else
                    {
                        stream.Position = 0;
                        await _bot.SendPhotoAsync(p.ChatId, file, message.Text ?? "");
                    }
                }
                catch(Exception ex)
                {
                    TempData["Error"] = "Отправка сообщений прервалась";
                    _logger.LogError(ex.Message + "\nProfile: " + p.ProfileId);
                }
            }
            stream.Close();
            _logger.LogInformation("End Spam Sending: " + DateTime.Now.ToString());
            return Redirect(ReturnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            var mes = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error.Message;
            await _bot.SendTextMessageAsync(1000055102, mes.ToString());
            await _bot.SendTextMessageAsync(1012613112, mes.ToString());
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}