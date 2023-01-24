using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CaffeBot.Entities;
using CaffeBot.Methods;

namespace CaffeBot.Services
{
    public class HandleUpdateService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HandleUpdateService> _logger;
        private readonly IConfiguration _configuration;

        private readonly DbService _db;
        private readonly CartMethod _cartMethod;
        private readonly ContactsMethod _contactsMethod;
        private readonly MenuMethod _menuMethod;
        private readonly ProfileMethod _profileMethod;
        private readonly PromoMethod _promoMethod;
        private readonly StartMethod _startMethod;
        private readonly TimeService _timeService;

        public HandleUpdateService(
                ITelegramBotClient botClient, ILogger<HandleUpdateService> logger,
                IConfiguration configuration, CartMethod cartMethod,
                ContactsMethod contactsMethod, DbService db,
                MenuMethod menuMethod, ProfileMethod profileMethod,
                PromoMethod promoMethod, StartMethod startMethod,
                TimeService timeService
            )
        {
            _botClient = botClient;
            _logger = logger;
            _configuration = configuration;
            _cartMethod = cartMethod;
            _contactsMethod = contactsMethod;
            _menuMethod = menuMethod;
            _profileMethod = profileMethod;
            _promoMethod = promoMethod;
            _startMethod = startMethod;
            _db = db;
            _timeService = timeService;
        }

        public async Task EchoAsync(Update update)
        {
            long chatId = -1;
            Task handler = update.Type switch
            {
                UpdateType.Message => Task.Run(async () => {
                    chatId = update.Message.Chat.Id;
                    await _db.InitializeProfileAsync(update.Message!);
                    if  (await IsUserBlocked(chatId))
                    {
                        return;
                    }
                    await BotOnMessageReceived(update.Message!);
                }),
                UpdateType.CallbackQuery => Task.Run(async () => {
                    chatId = update.CallbackQuery.Message.Chat.Id;
                    await _db.InitializeProfileAsync(update.CallbackQuery!.Message!);
                    if (await IsUserBlocked(chatId))
                    {
                        return;
                    }
                    await BotOnCallbackQueryReceived(update.CallbackQuery);
                }),
            };
            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception, chatId);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            long chatId = message.Chat.Id;
            var text = message.Text!;
            var profile = await _db.GetProfileAsync(chatId);    
            if (!await CheckSpecialMessageStatus(message)) { 
                var handler = text switch
                {
                    "/off" => Task.Run(async ()=> 
                    {
                        await _db.SetProfileUnsubscribedAsync(chatId);
                        await _botClient.SendTextMessageAsync(message.Chat.Id, "Вы отписались от рассылки");
                    }),
                    "/on" => Task.Run(async () => {
                        await _db.SetProfileSubscribedAsync(chatId);
                        await _botClient.SendTextMessageAsync(message.Chat.Id, "Вы подписались на рассылку");
                    }),
                    "/home" or "🏠 Домой" => _botClient.SendTextMessageAsync(chatId, "🏠", replyMarkup: KeyboardFactory.GetStart(profile.Notified, profile.IsAdmin)),
                    "/start" => _startMethod.StartMessage(message),
                    "/profile" or "🙂 Профиль" => _profileMethod.ProfileMessage(message),
                    "/menu" or "🍽 Меню" => _menuMethod.MenuMessage(message),
                    "/promo" or "💵 Акции" => _promoMethod.PromoMessage(message),
                    "/cart" or "🛒 Корзина" => _cartMethod.CartMessage(message),
                    "/contacts" or "📱 Контакты" => _contactsMethod.ContactsMessage(message),
                    "/admin" or "🖥 Панель управления" => _startMethod.AdminPanel(message),
                    _ => _startMethod.StartMessage(message),
                };
                await handler;
            }
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            Task handler = callbackQuery.Data switch
            {
                "/address" => _profileMethod.ChangeAddressAsync(callbackQuery),
                "/phoneNumber" => _profileMethod.ChangePhoneNumberAsync(callbackQuery),

                "/lugacom" => _contactsMethod.Lugacom(callbackQuery),
                "/vodafone" => _contactsMethod.Vodafone(callbackQuery),
                
                "/empty" => Task.CompletedTask,

                _ when callbackQuery.Data.StartsWith("/cart") => _menuMethod.ChangePositionInCart(callbackQuery),

                _ when callbackQuery.Data.StartsWith(InlineFactory.OrderDescription) => _cartMethod.AddOrderDescription(callbackQuery),
                _ when callbackQuery.Data.StartsWith(InlineFactory.OrderConfirm) => _cartMethod.OrderConfirm(callbackQuery, false),
                _ when callbackQuery.Data.StartsWith(InlineFactory.OrderBonusConfirm) => _cartMethod.OrderConfirm(callbackQuery, true),

                _ when callbackQuery.Data.StartsWith(InlineFactory.PositionNext) => _cartMethod.PositionNext(callbackQuery),
                _ when callbackQuery.Data.StartsWith(InlineFactory.PositionPrevious) => _cartMethod.PositionPrevious(callbackQuery),
                _ when callbackQuery.Data.StartsWith(InlineFactory.DecrementPosition) => _cartMethod.DecrementPosition(callbackQuery),
                _ when callbackQuery.Data.StartsWith(InlineFactory.IncrementPosition) => _cartMethod.IncrementPosition(callbackQuery),
                _ when callbackQuery.Data.StartsWith(InlineFactory.RemovePosition) => _cartMethod.RemovePosition(callbackQuery),
                _ when callbackQuery.Data.StartsWith(InlineFactory.ConfirmData) => _cartMethod.CartConfirm(callbackQuery),
                _ when callbackQuery.Data.StartsWith(InlineFactory.ClearData) => _cartMethod.CartClear(callbackQuery),

                _ when callbackQuery.Data.StartsWith(InlineFactory.PromoNext) => _promoMethod.PromoNext(callbackQuery),
                _ when callbackQuery.Data.StartsWith(InlineFactory.PromoPrevious) => _promoMethod.PromoPrevious(callbackQuery),
                
                _ => _menuMethod.GetDishesByCategory(callbackQuery)
            };
            await handler;
        }
        public async Task<bool> IsUserBlocked(long ChatId)
        {
            var profile = await _db.GetProfileAsync(ChatId);
            if (profile.IsBlocked)
            {
                await _botClient.SendTextMessageAsync(profile.ChatId, "Ваш профиль был заблокирован.");
                return true;
            }
            return false;
        }
        public async Task<bool> CheckSpecialMessageStatus(Message message)
        {
            var chatId = message.Chat.Id;

            var profile = await _db.GetProfileAsync(chatId);
            var messageText = message.Text ?? string.Empty;
            bool HasSpecialStatus = false;

            if (profile == null)
                return HasSpecialStatus;

            switch (profile.ProfileStatus)
            {
                case ProfileStatus.ADDRESS:
                    await _db.SetProfileAddressAsync(chatId, messageText);
                    await _db.SetProfileStatusAsync(chatId, ProfileStatus.DEFAULT);
                    HasSpecialStatus = true;
                    break;
                case ProfileStatus.PHONE_NUMBER:
                    await _db.SetProfilePhoneNumberAsync(chatId, messageText);
                    await _db.SetProfileStatusAsync(chatId, ProfileStatus.DEFAULT);
                    HasSpecialStatus = true;
                    break;
                case ProfileStatus.ORDER_DESCRIPTION:
                    await _db.SetCartDescriptionAsync(chatId, messageText);
                    await _db.SetProfileStatusAsync(chatId, ProfileStatus.DEFAULT);
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Примечание добавлено", replyMarkup: KeyboardFactory.LilKeyboard());
                    await _cartMethod.CartConfirm(new CallbackQuery
                    {
                        Message = message
                    });

                    HasSpecialStatus = true;
                    return HasSpecialStatus;
                default:
                    return HasSpecialStatus;
            }

            var markup = KeyboardFactory.GetStart(profile.Notified, profile.IsAdmin);
            await _botClient.SendTextMessageAsync(message.Chat.Id, "Информация успешно изменена", replyMarkup: markup);
            return HasSpecialStatus;    
        }

        public async Task HandleErrorAsync(Exception exception, long ChatId)
        {
            Type error = exception.GetType();
            await _botClient.SendTextMessageAsync(1000055102, exception.Message);
            await _botClient.SendTextMessageAsync(1012613112, exception.Message);
            //await _botClient.SendTextMessageAsync(ChatId, exception.Message);
            if (error == typeof(ArgumentOutOfRangeException) || error == typeof(IndexOutOfRangeException) )
            {
                await _db.ResetIndexesAsync(ChatId);
            }

            _logger.LogWarning(exception.Message);
        }
    }
}
