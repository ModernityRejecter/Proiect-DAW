using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        
        [Range(1, 5)]
        public int? Rating { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        //-----------------------------------------------------
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        //-----------------------------------------------------
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        //-----------------------------------------------------
    }
}
