using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaffeBot.Entities
{

    public class Position
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PositionId { get; set; }
        public long CartId { get; set; }  
        public Cart Cart { get; set; }
        public long DishId { get; set; }
        public Dish Dish { get; set; }
        public int Count { get; set; }  
        public decimal GetTotalPrice()
        {
            return Count * Dish?.Price ?? 0;
        }
    }
}
