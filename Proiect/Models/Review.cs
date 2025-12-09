using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "Review title must be at most 50 characters long")]
        public string? Title { get; set; }

        [StringLength(400, ErrorMessage = "Review description must be at most 400 characters long")]
        public string? Description { get; set; }
        
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
