using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        //-----------------------------------------

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        //-----------------------------------------



    }
}
