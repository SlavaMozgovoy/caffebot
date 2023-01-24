using CaffeBot.Entities;
using CaffeBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace CaffeBot.Methods
{
    public class MenuMethod
    {
        private ITelegramBotClient bot;
        private IConfiguration configuration;
        private DbService db;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment host;
        public MenuMethod(ITelegramBotClient bot, IConfiguration configuration, DbService db, Microsoft.AspNetCore.Hosting.IHostingEnvironment host)
        {
            this.bot = bot;
            this.configuration = configuration;
            this.db = db;
            this.host = host;
        }

        public async Task MenuMessage(Message message)
        {
            long chatId = message.Chat.Id;

            var categories = await db.GetCategoriesAsync();
            var nonEmpty = categories.Where(c=> c.Dishes.Where(d=>d.Available).Count() > 0).ToList();
            var markup = InlineFactory.GetMenu(nonEmpty);
            await bot.SendTextMessageAsync(chatId, "Меню", replyMarkup: KeyboardFactory.LilKeyboard());
            await bot.SendTextMessageAsync(chatId, "Выберите категорию", replyMarkup: markup);
        }

        public async Task GetDishesByCategory(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            var data = callbackQuery.Data.Split("?");
     
            var categoryId = Convert.ToInt64(data[0]);
            var @from = Convert.ToInt32(data[1]);
            var @to = Convert.ToInt32(data[2]);

            var category = await db.GetCategoryAsync(categoryId);
            var dishes = await db.GetDishesByCategoryAsync(categoryId, @from, @to);

            var imgPath = "";
            foreach (var dish in dishes)
            {
                try { 
                    var position = await db.GetPositionAsync(chatId, dish.DishId);
                    var count = position?.Count ?? 0;
                    var totalPrice = position?.GetTotalPrice() ?? 0;

                    var markup = InlineFactory.GetDishCountMarkup(dish.DishId, count, totalPrice);

                    string message = $"<b>{dish.Name}</b>\n{dish.Description}\n<b>{dish.Price}&#x20bd;</b>";
                    imgPath = host.WebRootPath + dish.ImgPath;
                    if (dish.ImgPath == null || !System.IO.File.Exists(imgPath))
                    {
                        string DefaultImagePath = configuration.GetSection("Images")["DefaultImage"];
                        imgPath = host.WebRootPath + DefaultImagePath;
                    }

                    using Stream stream = System.IO.File.OpenRead(imgPath);
                    InputOnlineFile photo = new InputOnlineFile(stream, dish.Name ?? "_") { FileName = dish.DishId.ToString() };

                    await bot.SendPhotoAsync(callbackQuery.Message.Chat.Id, photo, message, replyMarkup: markup, parseMode: ParseMode.Html);
                }
                catch (Exception ex) 
                {
                    await bot.SendTextMessageAsync(chatId, ex.Message);
                }
            }

            var getLeftItems = await db.CountDishesByCategoryAsync(categoryId);
            var it = getLeftItems - from - to;
            
            if (it >= 5) {
                await bot.SendTextMessageAsync(chatId,"Следующие блюда", replyMarkup: InlineFactory.GetMenuNext(categoryId, from + to, to, getLeftItems));
            }
            else if (it > 0 && it < 5)
            {
                await bot.SendTextMessageAsync(chatId, "Следующие блюда", replyMarkup: InlineFactory.GetMenuNext(categoryId, from + to, it, getLeftItems));
            }
        }

        public async Task ChangePositionInCart(CallbackQuery callbackQuery)
        {
            long chatId = callbackQuery.Message.Chat.Id;

            var dishId = Int64.Parse(callbackQuery.Data.Split('?')[2]);

            if (callbackQuery.Data.Contains("add"))
            {
                await db.IncrementPositionAsync(chatId, dishId);
            }
            else
            {
                await db.DecrementPositionAsync(chatId, dishId);
            }
            try 
            { 
                var position = await db.GetPositionAsync(chatId, dishId);
                if (position == null)
                {
                    await bot.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id,
                      callbackQuery.Message.MessageId,
                      InlineFactory.GetDishCountMarkup(dishId,
                                      0,
                                      0));
                    return;
                }
                    
                await bot.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, 
                callbackQuery.Message.MessageId,
                InlineFactory.GetDishCountMarkup(dishId, 
                                position.Count, 
                                position.GetTotalPrice()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
