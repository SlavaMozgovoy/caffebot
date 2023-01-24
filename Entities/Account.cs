using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaffeBot.Entities
{
    public enum AccountRole
    {
        Common = 0,
        Admin = 1,
        Head = 2,
        Developer = 3,
    }
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public AccountRole Role { get; set; } = AccountRole.Common;
    }
}
