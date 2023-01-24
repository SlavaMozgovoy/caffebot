using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaffeBot.Entities
{
    public class Dish
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DishId { get; set; }
        public string? Name { get; set; }
        public string? ImgPath { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; } = default;
        public long CategoryId { get; set; }
        public Category Category { get; set; }
        public bool Available { get; set; } = true;
    }
}
