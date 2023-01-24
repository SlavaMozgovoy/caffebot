using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.ReplyMarkups;
using CaffeBot.Services;

namespace CaffeBot.Methods
{
    public class StartMethod
    {
        private ITelegramBotClient bot;
        private DbService db;
        private IConfiguration configuration;
        public StartMethod(ITelegramBotClient bot, DbService db, IConfiguration configuration)
        {
            this.bot = bot;
            this.db = db;
            this.configuration = configuration;
        }
        public async Task StartMessage(Message message)
        {
            long chatId = message.Chat.Id;
            var profile = await db.GetProfileAsync(chatId);
            var bonus = configuration["BonusPercent"];
            var markup = KeyboardFactory.GetStart(profile.Notified, profile.IsAdmin);
            await bot.SendTextMessageAsync(message.Chat.Id, $"Привет! Я бот Кафе «Наше», помогу тебе сделать заказ. \r\nДля начала заполни профиль \r\n/profile 😉\r\n\r\nПри заказе через меня, ты получишь кэшбэк {bonus}% 💵\r\n\r\nСмотри заработанные бонусы в /profile и используй их для бесплатного заказа 🆓", replyMarkup: markup);
        }

        public async Task AdminPanel(Message message)
        {
            long chatId = message.Chat.Id;
            var profile = await db.GetProfileAsync(chatId);
            if (profile.Notified || profile.IsAdmin) { 
                var markup = InlineFactory.GetNotifyMarkup();
                await bot.SendTextMessageAsync(message.Chat.Id, "Административная панель", replyMarkup: markup);
            }
        }
    }
}
