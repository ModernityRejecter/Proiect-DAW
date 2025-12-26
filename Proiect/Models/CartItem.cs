using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }

        //-----------------------------------------

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        //-----------------------------------------

        public int CartId { get; set; }

        public virtual ShoppingCart Cart { get; set; }

        //-----------------------------------------
    }
}
