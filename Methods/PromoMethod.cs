using CaffeBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace CaffeBot.Methods
{
    public class PromoMethod
    {
        private ITelegramBotClient bot;
        private IConfiguration configuration;
        private DbService db;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment env;
        public PromoMethod(ITelegramBotClient bot, IConfiguration configuration, DbService db, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            this.bot = bot;
            this.configuration = configuration;
            this.db = db;
            this.env = env;
        }

        public async Task PromoMessage(Message message)
        {
            var chatId = message.Chat.Id;
            var profile = await db.GetProfileAsync(chatId);
            var promotions = await db.GetPromotionsAsync();
            if (promotions.Count == 0)
            {
                await bot.SendTextMessageAsync(chatId, "На данный момент акций нет");
                return;
            }
            var promo = promotions[profile?.PromoIndex ?? 0];


            var markup = InlineFactory.GetPromo(profile?.PromoIndex ?? 0, promotions.Count);
            string imgPath;
            if (promo.ImagePath == null)
            {
                imgPath = env.WebRootPath + configuration.GetSection("Images")["Promo"];
            }
            else
            {
                imgPath = env.WebRootPath + promo.ImagePath;
            }
            using Stream s = System.IO.File.OpenRead( imgPath);
            InputOnlineFile photo = new InputOnlineFile(s, "PROMO");
            
            await bot.SendPhotoAsync(chatId, photo, promo.Description ?? " ", replyMarkup: markup);
        }

        private async Task PromoUpdate(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var profile = await db.GetProfileAsync(chatId);
            var promotions = await db.GetPromotionsAsync();
            
            if (promotions.Count == 0)
            {
                await bot.SendTextMessageAsync(chatId, "На данный момент акций нет");
                return;
            }

            var promo = promotions[profile.PromoIndex];
            var markup = InlineFactory.GetPromo(profile.PromoIndex, promotions.Count);

            string imgPath;
            if (promo.ImagePath == null)
            {
                imgPath = env.WebRootPath + configuration.GetSection("Images")["Promo"];
            }
            else
            {
                imgPath = env.WebRootPath + promo.ImagePath;
            }
            using Stream s = System.IO.File.OpenRead(imgPath);
            
            InputMediaPhoto photo = new InputMediaPhoto(new InputMedia(s, "PROMO"));
            photo.Caption = promo.Description;

            await bot.EditMessageMediaAsync(chatId, callbackQuery.Message.MessageId, photo, replyMarkup: markup);
        }

        public async Task PromoPrevious(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            await db.DecrementPromoIndexAsync(chatId);

            await PromoUpdate(callbackQuery);
        }

        public async Task PromoNext(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            
            await db.IncrementPromoIndexAsync(chatId);

            await PromoUpdate(callbackQuery);
        }
    }
}
