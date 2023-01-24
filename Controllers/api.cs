using CaffeBot.Entities;
using CaffeBot.Models;
using CaffeBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace CaffeBot.Controllers
{
    [Authorize("Common")]
    public class ApiController : Controller
    {
        private readonly ApplicationContext _db;

        public ApiController(ApplicationContext db)
        {
            _db = db;
        }
        [Route("/api/getOrdersPolling")]
        public async Task<IActionResult> GetIncomleteOrders([FromServices] ApplicationContext _context)
        {
            var orders = _context.Orders
                .Include(o=>o.Profile)
                .Include(o=>o.Cart)
                .ThenInclude(c=>c.Positions)
                .ThenInclude(p=>p.Dish)
                .Where(o => o.IsConfirmed == OrderStatus.DEFAULT)
                .ToList();
            return PartialView("OrdersPartial", orders);
        }

        // GET: api/Details/5
        [Route("api/getOrdersByDate")]
        public async Task<IActionResult> GetOrders(string date)
        {
            try
            {
                DateTime time = DateTime.Parse(date);
                var orders = await _db.Orders
                    .Where(i => i.ConfirmTime.Value.Date == time.Date)
                    .Include(i => i.Profile)
                    .Select(i => new OrderByDateModel()
                    {
                        OrderId = i.OrderId,
                        ProfileId = i.ProfileId,
                        UserName = i.Profile.Name,
                        BonusChange = i.BonusChange > 0 ? "+" + i.BonusChange.ToString("F1") : i.BonusChange.ToString("F1"),
                        BonusTotal = i.BonusTotal,
                        ConfirmedTotalPrice = i.ConfirmedTotalPrice,
                        IsConfirmed = i.IsConfirmed,
                        Fixal = i.Fixal,
                        OrderTime = i.OrderTime,
                        ConfirmTime = i.ConfirmTime,

                    }).ToListAsync();
                return PartialView("UsersPartial",orders);
            }
            catch
            {
                return PartialView("UsersPartial", Enumerable.Empty<OrderByDateModel>());
            }

        }

        [Route("api/getPeriod")]
        public async Task<FileResult> GetPeriod(string from, string to)
        {
            Encoding utf = Encoding.UTF32;
            DateTime fromT = DateTime.Parse(from);
            DateTime toT = DateTime.Parse(to);
            toT = toT.AddDays(1);

            using MemoryStream ms = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(ms,utf);

            var orders = await _db.Orders.Include(o => o.Profile).Where(i=>i.ConfirmTime.Value >= fromT && i.ConfirmTime.Value <= toT && i.IsConfirmed == OrderStatus.CONFIRMED).ToListAsync();
            await streamWriter.WriteAsync("Id пользователя\tПользователь\tЦена\tОплата за бонусы\tИзменение бонусов\tФискальный чек\tВремя заказ\tВремя подтверждения заказа\n");
            foreach (var order in orders)
            {
                await streamWriter.WriteAsync($"{order.ProfileId}\t{order.Profile.Name}\t{order.ConfirmedTotalPrice}\t{order.IsPaidByBonuses}\t{order.BonusChange}\t{order.Fixal}\t{order.OrderTime}\t{order.ConfirmTime}\n");
            }

            streamWriter.Close();
            return File(ms.GetBuffer(), "text/csv", "отчет.csv");
        } 
    }
}
