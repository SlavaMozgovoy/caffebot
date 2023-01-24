using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaffeBot.Entities
{
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CartId { get; set; }
        public long ProfileId { get; set; }
        public Profile? Profile { get; set; }
        public ICollection<Position>? Positions { get; set; }
        public string? Desctiption { get; set; }
        public int Index { get; set; } = 0;
        public bool Confirmed { get; set; } = false;
    }
}