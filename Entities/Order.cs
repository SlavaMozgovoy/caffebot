using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaffeBot.Entities
{
    public enum OrderStatus
    {
        DEFAULT = 0,
        CONFIRMED = 1,
        DECLINED = 2,
    }
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderId { get; set; }
        public long? ProfileId { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }    
        public Profile? Profile {get; set; } 
        public long? CartId { get; set; }
        public Cart? Cart { get; set; }  
        public DateTime? OrderTime { get; set; }
        public string? Fixal { get; set; }
        public DateTime? ConfirmTime { get; set; }
        public OrderStatus IsConfirmed { get; set; } = OrderStatus.DEFAULT;
        public decimal ConfirmedTotalPrice { get; set; }
        public decimal BonusChange { get; set; } = 0;
        public decimal BonusTotal { get; set; } = 0;
        public bool IsPaidByBonuses { get; set; } = false;

    }
}
