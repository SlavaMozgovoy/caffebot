using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaffeBot.Entities
{
    public class Promotion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PromotionId { get; set; }

        public string? Description { get; set; }

        public string? ImagePath { get; set; }

    }
}
