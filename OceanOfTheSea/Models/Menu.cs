using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OceanOfTheSea.Models
{
    [Table("Menu")]
    public class Menu
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? DishName { get; set; }
        [Required]
        public double Price { get; set; }
        public int GenreId { get; set; }

        public Genre Genre { get; set; }
        public List<OrderDetail> OrderDetail { get; set; }
        public List<CartDetail> CartDetail { get; set; }
    }
}
