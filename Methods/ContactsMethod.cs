using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace CaffeBot.Methods
{
    public class ContactsMethod
    {
        public string ImagePath;

        private ITelegramBotClient bot;
        private IConfiguration configuration;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment env;

        public ContactsMethod(ITelegramBotClient bot, IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            this.bot = bot;
            this.configuration = configuration;

            ImagePath = env.WebRootPath + configuration.GetSection("Images")["Contacts"];
            this.env = env;
        }

        public async Task ContactsMessage(Message message)
        {
            long chatId = message.Chat.Id;

            var text =
@"Мы находимся по адресу г.Алчевск, ул. Ленина, д. 64
Наши контакты:";
            var markup = InlineFactory.GetContacts();

            using Stream stream = System.IO.File.OpenRead(ImagePath);
            InputOnlineFile photo = new InputOnlineFile(stream, "Наше кафе");

            await bot.SendPhotoAsync(chatId, photo, text, replyMarkup: markup);
        }


        public async Task Lugacom(CallbackQuery callbackQuery)
        {
            await bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "+380721747643");
            return;
        }
        public async Task Vodafone(CallbackQuery callbackQuery)
        {
            await bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "+380997407729");
            return;
        }
    }
}
