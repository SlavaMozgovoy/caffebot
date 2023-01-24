using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace CaffeBot.Services
{
    public class NotifyService
    {

        public NotifyService(ITelegramBotClient bot, ApplicationContext context)
        {
            _bot = bot;
            _context = context;
        }
        public async Task Notify()
        {
            var markup = InlineFactory.GetNotifyMarkup();
            var users = await _context.Profiles.Where(p => p.Notified).ToListAsync();
            foreach (var user in users)
            {
                await _bot.SendTextMessageAsync(user.ChatId, "Поступил новый заказ!", replyMarkup: markup);
            }
        }
        private ITelegramBotClient _bot { get; set; } 
        private ApplicationContext _context { get; set; }
    }
}
