using CaffeBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CaffeBot.Methods
{
    public class ProfileMethod
    {
        private ITelegramBotClient bot;
        private DbService db;

        public ProfileMethod(ITelegramBotClient bot, DbService db)
        {
            this.bot = bot;
            this.db = db;
        }

        public async Task ProfileMessage(Message message)
        {
            long chatId = message.Chat.Id;

            var profile = await db.GetProfileAsync(chatId);
            var markup = InlineFactory.GetProfile();

            var text =
                $"<strong>ВАШ ПРОФИЛЬ</strong>" +
                $"\nИмя: {profile.Name};" +
                $"\nНомер телефона: {profile.PhoneNumber};" +
                $"\nАдрес: {profile.Address};" +
                $"\nБонусы: {profile.Bonus}₽;";

            await bot.SendTextMessageAsync(
                chatId,
                text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: markup);
        }

        public async Task ChangeAddressAsync(CallbackQuery update)
        {
            long chatId = update.Message!.Chat.Id;
            await db.SetProfileStatusAsync(chatId, Entities.ProfileStatus.ADDRESS);
            await bot.SendTextMessageAsync(chatId, "Введите новый адрес: ", replyMarkup: KeyboardFactory.GetEmpty());
        }

        public async Task ChangePhoneNumberAsync(CallbackQuery update)
        {
            long chatId = update.Message!.Chat.Id;
            await db.SetProfileStatusAsync(chatId, Entities.ProfileStatus.PHONE_NUMBER);
            await bot.SendTextMessageAsync(chatId, "Введите новый номер телефона: ", replyMarkup: KeyboardFactory.GetEmpty());
        }
    }
}
