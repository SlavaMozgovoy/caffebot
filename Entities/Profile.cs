using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CaffeBot.Entities
{
    public enum ProfileStatus 
    {
        DEFAULT = 0,
        ADDRESS = 1,
        PHONE_NUMBER = 2,
        ORDER_DESCRIPTION = 3
    }

    public class Profile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProfileId { get; set; }
        public long ChatId { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public int PromoIndex { get; set; } = 0;
        public ProfileStatus ProfileStatus { get; set; } = ProfileStatus.DEFAULT;
        public decimal Bonus { get; set; } = default;
        public bool Subscribed { get; set; } = true;
        public bool IsBlocked { get; set; } = false;   
        public bool Notified { get; set; } = false;
        public bool IsAdmin { get; set; } = false;
    }
}
