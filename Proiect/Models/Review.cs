using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public double Rating { get; set; }
        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
