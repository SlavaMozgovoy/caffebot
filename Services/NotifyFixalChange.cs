using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace CaffeBot.Services
{
    public class NotifyFixalChange
    {
        public NotifyFixalChange(ITelegramBotClient bot, ApplicationContext context)
        {
            _bot = bot;
            _context = context;
        }
        public async Task Notify(long orderId, string orderDate, string oldNumber, string newNumber)
        {
            var markup = InlineFactory.GetNotifyMarkup();
            var users = await _context.Profiles.Where(p => p.IsAdmin).ToListAsync();
            foreach (var user in users)
            {
                await _bot.SendTextMessageAsync(user.ChatId, $"Изменен номер фискального чека заказа №{orderId} от {orderDate}.\nБыл: {oldNumber};\nСтал: {newNumber};", replyMarkup: markup);
            }
        }
        public async Task NotifyDuplicate(string fixal, DateTime date)
        {
            var markup = InlineFactory.GetNotifyMarkup();
            var users = await _context.Profiles.Where(p => p.IsAdmin).ToListAsync();
            var orders = await _context.Orders.Where(o => o.IsConfirmed == Entities.OrderStatus.CONFIRMED && o.Fixal == fixal && o.ConfirmTime.Value.Date == date.Date).ToListAsync();

            string message = $"ДУБЛИКАТЫ ФИСКАЛЬНЫХ ЧЕКОВ\nот {date.ToString("dd.MM.yyyy")}\n\n";

            foreach (var order in orders)
            {
                message += $"Заказ №{order.OrderId} - чек №{order.Fixal} - дата №{order.ConfirmTime.Value.ToString("dd.MM.yyyy")} ";
            }

            foreach (var user in users)
            {
                await _bot.SendTextMessageAsync(user.ChatId,message
                    , replyMarkup: markup);
            }
        }
        private ITelegramBotClient _bot { get; set; }
        private ApplicationContext _context { get; set; }
    }
}
