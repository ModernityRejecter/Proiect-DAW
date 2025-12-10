using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(50, ErrorMessage = "Item status must be at most 50 characters long")]
        public string Status { get; set; } = "In progress";

        //-----------------------------------------

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        //-----------------------------------------

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
