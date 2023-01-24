using CaffeBot.Entities;
using CaffeBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace CaffeBot.Controllers
{
    [Authorize("Common")]
    public class OrderController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderController> _logger;
        private readonly ITelegramBotClient _bot;
        private readonly NotifyFixalChange _notifyFixal;

        private readonly int BonusPercent = 0;
        public OrderController(ApplicationContext context, IConfiguration configuration, ILogger<OrderController> logger, ITelegramBotClient bot, NotifyFixalChange notifyFixal)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _bot = bot;
            _notifyFixal = notifyFixal;
            BonusPercent = Convert.ToInt32(_configuration["BonusPercent"]);
            
        }
        [Authorize("Head")]
        public async Task<IActionResult> SetFixal(long Id, string Fixal)
        {
            if (Fixal == null || Fixal == string.Empty)
            {
                TempData["Error"] = "ОШИБКА. Неверный номер фискального чека";
                return RedirectToAction("OrderInfo", "Order", new { Id = Id });
            }
            var order = await _context.Orders.FirstOrDefaultAsync(i => i.OrderId == Id);

            string oldFixal = order.Fixal;
            order.Fixal = Fixal;
            await _context.SaveChangesAsync();
            string newFixal = order.Fixal;
            await _notifyFixal.Notify(Id, order.ConfirmTime.ToString(), oldFixal, newFixal);
            var list = await _context.Orders.Where(o => o.IsConfirmed == OrderStatus.CONFIRMED
                && o.Fixal == Fixal 
                && o.ConfirmTime.Value.Date.Date == order.ConfirmTime.Value.Date.Date)
                .ToListAsync();
            if (list.Count > 1)
            {
                await _notifyFixal.NotifyDuplicate(Fixal, order.ConfirmTime.Value.Date);
            }
            return RedirectToAction("OrderInfo","Order",new {Id = Id});
        }

        [Authorize("Admin")]
        public async Task<IActionResult> AddPosition(long Id, long dishId, int Count)
        {
            var order = await _context.Orders
                .Include(o => o.Cart)
                .ThenInclude(c => c.Positions)
                .ThenInclude(p =>p.Dish)
                .FirstOrDefaultAsync(o => o.OrderId == Id);
            if (order == null) { return NotFound(); }
            if (order.IsConfirmed != OrderStatus.DEFAULT)
            {
                return RedirectToAction("Index", "Home");
            }

            var position = order.Cart.Positions.FirstOrDefault(p => p.DishId == dishId);

            if (position != null)
            {
                position.Count += Count;
            }
            else
            {
                var newPosition = new Position()
                {
                    CartId = order.CartId.Value,
                    Count = Count,
                    DishId = dishId
                };
                _context.Positions.Add(newPosition);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("OrderInfo", "Order", new { id = Id });
        }

        [Authorize("Admin")]
        public async Task<IActionResult> EditDishCount(long Id, long OrderId, int Count)
        {
            var order = await _context.Orders.Include(o=>o.Cart).ThenInclude(c=>c.Positions).FirstOrDefaultAsync(o => o.OrderId == OrderId);
            var position = order.Cart.Positions.FirstOrDefault(p => p.DishId == Id);
            if (position != null)
            {
                if (Count <= 0)
                    _context.Positions.Remove(position);
                else
                    position.Count = Count;

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("OrderInfo", "Order", new { id = OrderId });
        }

        [Authorize("Admin")]
        public async Task<IActionResult> ConfirmOrder(long Id, string Date, string Fixal)
        {

            if (Fixal == null || Fixal == string.Empty)
            {
                TempData["Error"] = "ОШИБКА. Неверный номер фискального чека";
                return RedirectToAction("OrderInfo", "Order", new { Id = Id });
            }

            var order = await _context.Orders
                .Include(o => o.Profile)
                .Include(c=>c.Cart)
                .ThenInclude(c=>c.Positions)
                .ThenInclude(c=>c.Dish)
                .FirstOrDefaultAsync(i => i.OrderId == Id);
            if (order.IsConfirmed != OrderStatus.DEFAULT)
            {
                return RedirectToAction("Index", "Home");
            }
            order.IsConfirmed = Entities.OrderStatus.CONFIRMED;
            order.ConfirmedTotalPrice = order.Cart.Positions.Sum(s => s.Count * s.Dish.Price);
            order.ConfirmTime = DateTime.Parse(Date);
            order.Fixal = Fixal;

            if (order.IsPaidByBonuses)
            {
                if (order.ConfirmedTotalPrice > order.Profile.Bonus)
                {
                    TempData["Error"] = "ОШИБКА. У пользователя недостаточно бонусов для оплаты бонусами";
                    return RedirectToAction("OrderInfo", "Order", new { Id = Id });
                }
                order.BonusChange = -1 * order.ConfirmedTotalPrice;
                order.Profile.Bonus += order.BonusChange;
                await _bot.SendTextMessageAsync(order.Profile.ChatId, $"Заказ был оплачен бонусами. Вычтено {order.BonusChange}. Текущее количестов бонусов: {order.Profile.Bonus}");
            }
            else
            {
                order.BonusChange = (int)order.ConfirmedTotalPrice * BonusPercent / 100;
                order.Profile.Bonus += order.BonusChange;
                await _bot.SendTextMessageAsync(order.Profile.ChatId, $"На ваш счет были зачислены бонусы {order.BonusChange}. Текущее количестов бонусов: {order.Profile.Bonus}");

            }
            order.BonusTotal = order.Profile.Bonus;
            await _context.SaveChangesAsync();
            var list = await _context.Orders.Where(o => o.IsConfirmed == OrderStatus.CONFIRMED
                        && o.Fixal == Fixal
                        && o.ConfirmTime.Value.Date.Date == order.ConfirmTime.Value.Date.Date)
                        .ToListAsync();
            if (list.Count > 1)
            {
                await _notifyFixal.NotifyDuplicate(Fixal, order.ConfirmTime.Value.Date);
            }


            return RedirectToAction("Index", "Home");
        }

        [Authorize("Admin")]
        public async Task<IActionResult> DeclineOrder(long Id, string DeclineReason)
        {
            var order = await _context.Orders.Include(o=>o.Profile).FirstOrDefaultAsync(i => i.OrderId == Id);
            if (order != null)
            {
                if (order.IsConfirmed != OrderStatus.DEFAULT)
                {
                    return RedirectToAction("Index", "Home");
                }
                string s = "Ваш заказ был отклонен. ";
                order.IsConfirmed = Entities.OrderStatus.DECLINED;
                await _context.SaveChangesAsync();
                if (DeclineReason != null)
                {
                    s += "Причина отказа: " + DeclineReason;
                }
                await _bot.SendTextMessageAsync(order.Profile.ChatId, s);
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> OrderInfo(long Id)
        {
            var order = await _context.Orders.Include(o=>o.Profile)
                .Include(o=>o.Cart)
                .ThenInclude(c=>c.Positions)
                .ThenInclude(p=>p.Dish)
                .FirstOrDefaultAsync(o => o.OrderId == Id);

            var dishes = await _context.Dishes.ToListAsync();

            ViewBag.Dishes = dishes;
            return View(order);
        }
    }
}
