using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        //uhhhhhhhhhhhhhhhhhhhhhhhh hmmmmmmmmmmmmmmmmm nu stiu hmmmmmmmmmmmmm poate o sa sterg???
        [StringLength(50, ErrorMessage = "Statusul produsului este un câmp obligatoriu")]
        public string Status { get; set; } = "În curs de desfășurare";

        //-----------------------------------------

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        //-----------------------------------------

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
