using CaffeBot.Entities;
using CaffeBot.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace CaffeBot.Methods
{
    public class CartMethod
    {
        private ITelegramBotClient bot;
        private IConfiguration configuration;
        private DbService db;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment env;
        private TimeService timeService;
        private NotifyService notifyService;
        public CartMethod(ITelegramBotClient bot, IConfiguration configuration, DbService db, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, TimeService timeService, NotifyService notifyService)
        {
            this.bot = bot;
            this.configuration = configuration;
            this.db = db;
            this.env = env;
            this.timeService = timeService;
            this.notifyService = notifyService;
        }

        public async Task CartMessage(Message message)
        {
            long chatId = message.Chat.Id;

            var cart = await db.GetCartAsync(chatId);
            var positions = await db.GetPositionsAsync(chatId);

            if (positions.Count == 0)
            {
                await bot.SendTextMessageAsync(chatId, "В корзине ничего нет. Все блюда находятся в /menu");
                return;
            }
            Console.WriteLine(cart.Index);
            var dish = positions[cart.Index].Dish;

            var markup = InlineFactory.GetCartMarkup(chatId, positions, cart.Index);
            Stream stream = null;
            if (dish.ImgPath == null || !System.IO.File.Exists(env.WebRootPath + dish.ImgPath))
            {
                string DefaultImagePath = configuration.GetSection("Images")["DefaultImage"];
                stream = System.IO.File.OpenRead(env.WebRootPath + DefaultImagePath);
            }
            else
            {
                stream = System.IO.File.OpenRead(env.WebRootPath + dish.ImgPath);
            }
            using (stream)
            {
                string text = $"<strong>{dish.Name}</strong>\n{dish.Description} \n<strong>{dish.Price}&#x20bd; * {positions[cart.Index].Count} = {dish.Price * positions[cart.Index].Count}&#x20bd;</strong>";
                InputOnlineFile photo = new InputOnlineFile(stream, dish.Name ?? "_");
                await bot.SendTextMessageAsync(chatId, "Корзина", replyMarkup: KeyboardFactory.LilKeyboard());
                await bot.SendPhotoAsync(message.Chat.Id, photo, text, replyMarkup: markup, parseMode: ParseMode.Html);
            }
        }

        private async Task CartUpdateMessage(CallbackQuery callbackQuery)
        {
            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;

            var cart = await db.GetCartAsync(chatId);
            var positions = await db.GetPositionsAsync(chatId);

            if (positions.Count == 0)
            {
                await bot.SendTextMessageAsync(chatId, "В корзине ничего нет. Все блюда находятся в /menu");

                return;
            }

            var dish = positions[cart.Index].Dish;

            var markup = InlineFactory.GetCartMarkup(chatId, positions, cart.Index);

            var ImgPath = env.WebRootPath + dish.ImgPath;
            if (dish.ImgPath == null || !System.IO.File.Exists(ImgPath))
            {
                string DefaultImagePath = configuration.GetSection("Images")["DefaultImage"];
                ImgPath = env.WebRootPath + DefaultImagePath;
            }

            using Stream stream = System.IO.File.OpenRead(ImgPath);
            InputMediaPhoto photom = new InputMediaPhoto(new InputMedia(stream, "dish"));
            photom.Caption = $"<strong>{dish.Name}</strong>\n{dish.Description} \n<strong>{dish.Price}&#x20bd; * {positions[cart.Index].Count} = {dish.Price * positions[cart.Index].Count}&#x20bd;</strong>";
            photom.ParseMode = ParseMode.Html;
            
            await bot.EditMessageMediaAsync(chatId, messageId, photom, replyMarkup: markup);
        }

        public async Task CartClear(CallbackQuery update)
        {
            long chatId = update.Message.Chat.Id;
            int messageId = update.Message.MessageId;

            await db.ClearCartAsync(chatId);
           
            await bot.DeleteMessageAsync(chatId, messageId);
            await bot.SendTextMessageAsync(chatId, "Корзина была очищена", replyMarkup: KeyboardFactory.LilKeyboard());
        }

        public async Task PositionNext(CallbackQuery update)
        {
            long chatId = update.Message.Chat.Id;
            await db.IncrementCartIndexAsync(chatId);
            await CartUpdateMessage(update);
        }

        public async Task PositionPrevious(CallbackQuery update)
        {
            long chatId = update.Message.Chat.Id;
            await db.DecrementCartIndexAsync(chatId);
            await CartUpdateMessage(update);
        }

        public async Task IncrementPosition(CallbackQuery update)
        {
            var positionId = Convert.ToInt64(update.Data.Split('?')[2]);
            await db.IncrementPositionAsync(positionId);
            await CartUpdateMessage(update);
        }

        public async Task DecrementPosition(CallbackQuery update)
        {
            var positionId = Convert.ToInt64(update.Data.Split('?')[2]);
            await db.DecrementPositionAsync(positionId);
            await CartUpdateMessage(update);
        }

        public async Task RemovePosition(CallbackQuery update)
        {
            var chatId = update.Message.Chat.Id;
            var positionId = Convert.ToInt64(update.Data.Split('?')[2]);
            await db.RemovePositionAsync(positionId);
            await db.ChangeCartIndexAsync(chatId);
            if ((await db.GetPositionsAsync(chatId)).Count == 0)
            {
                var messageId = update.Message.MessageId;
                await bot.DeleteMessageAsync(chatId, messageId);
            }
            await CartUpdateMessage(update); 
        }
        public async Task CartConfirm(CallbackQuery update)
        {
            var chatId = update.Message.Chat.Id;
            var profile = await db.GetProfileAsync(chatId);
            var cart = await db.GetCartAsync(chatId);

            if (profile.PhoneNumber == null || profile.Address == null)
            {
                await bot.SendTextMessageAsync(chatId, "У Вас не заполнены данные о номере телефона и/или о адресе. Заполните их в разделе /profile");
                return;
            }

            if (await HasUnavailableSendMessage(chatId))
            {
                return;
            }
            bool bonuses = profile.Bonus > (await db.GetPositionsAsync(chatId)).Sum(p => p.Count * p.Dish.Price);
            var markup = InlineFactory.GetConfirmMarkup(bonuses);
            await bot.SendTextMessageAsync(chatId, await GetTotalAsync(chatId), ParseMode.Html, replyMarkup: markup);
        }

        public async Task AddOrderDescription(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            await db.SetProfileStatusAsync(chatId, ProfileStatus.ORDER_DESCRIPTION);

            await bot.SendTextMessageAsync(chatId, "Примечание к заказу:", replyMarkup: KeyboardFactory.GetEmpty());

        }

        public async Task OrderConfirm(CallbackQuery callbackQuery, bool bonus)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            if (timeService.IsAllowed(chatId, DateTime.Now))
            {
                return;
            }
            if (await HasUnavailableSendMessage(chatId))
            {
                return;
            }
            if (bonus)
            {
                var profile = await db.GetProfileAsync(chatId);
                var positions = await db.GetPositionsAsync(chatId);
                var totalPrice = positions.Sum(p => p.Count * p.Dish.Price);

                if (profile.Bonus < totalPrice)
                {
                    await bot.SendTextMessageAsync(chatId, "Недостаточно бонусов для заказа");
                    return;
                }
            }
            bool isConfirmed = await db.ConfirmOrderAsync(chatId, bonus);
            if (isConfirmed) 
            { 
                await bot.SendTextMessageAsync(chatId, "Спасибо за заказ! В ближайшее время с вами свяжется администратор");
            }
            else
            {
                await bot.SendTextMessageAsync(chatId, "Заказ не содержит ни одной позиции");
            }
            await notifyService.Notify();
        }

        private async Task<bool> HasUnavailableSendMessage(long chatId)
        {
            var cart = await db.GetCartAsync(chatId);
            var notAvailable = await db.GetNotAvailableDishesAsync(chatId);

            if (notAvailable.Count > 0)
            {
                string na = "К сожалению, некоторые позиции стали недоступными, и были убраны из корзины:\n";
                foreach (var position in notAvailable)
                {
                    na += $"<b>{position.Dish.Name}</b>\n";
                    await db.RemovePositionAsync(position.PositionId);
                }
                na += "Замените их чем-нибудь из /menu";
                await bot.SendTextMessageAsync(chatId, na, ParseMode.Html);
                return true;
            }
            return false;
        }

        private async Task<string> GetTotalAsync(long ChatId)
        {
            var profile = await db.GetProfileAsync(ChatId);
            var cart = await db.GetCartAsync(ChatId);
            var positions = await db.GetPositionsAsync(ChatId);
            string s = $"<b>Адрес доставки:</b> <i>{profile.Address}</i>\n" +
                $"<b>Номер телефона:</b> <i>{profile.PhoneNumber}</i>\n\n";

            if (cart.Desctiption is not null)
            {
                s += $"Примечание:\n {cart.Desctiption}\n\n";
            }

            foreach (var position in positions)
            {
                s += $"<b>{position.Dish.Name}</b>   <i>{position.Dish.Price}₽ * {position.Count}шт.</i>   <i>{position.Dish.Price * position.Count}₽</i>\n";
            }
            var deliveryPrice = Convert.ToInt32(configuration["DeliveryPrice"]);
            var freeOrder = Convert.ToInt32(configuration["FreeOrder"]);
            var totalPrice = positions.Sum(p => p.Count * p.Dish.Price);
            string addition = totalPrice >= freeOrder ? "" : "+" + deliveryPrice + "₽ за доставку"; 
            s += $"\nИтого: {totalPrice}₽ {addition}\n\n";

            return s;
        }
    }
}
