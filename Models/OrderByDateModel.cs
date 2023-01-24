using CaffeBot.Entities;

namespace CaffeBot.Models
{
    public class OrderByDateModel
    {
        public long? OrderId { get; set; } = 0;
        public long? ProfileId { get; set; } = 0;
        public string? UserName { get; set; } = string.Empty;
        public DateTime? OrderTime { get; set; } = DateTime.Now;
        public string? Fixal { get; set; } = string.Empty;
        public DateTime? ConfirmTime { get; set; } = DateTime.Now;
        public OrderStatus? IsConfirmed { get; set; } = OrderStatus.DEFAULT;
        public decimal? ConfirmedTotalPrice { get; set; } = 0;
        public string? BonusChange { get; set; } = "0";
        public decimal? BonusTotal { get; set; } = 0;
    }
}
