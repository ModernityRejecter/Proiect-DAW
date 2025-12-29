using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "Lungimea titlului trebuie să fie de maxim 100 caractere")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Lungimea descrierii trebuie să fie de maxim 1000 caractere")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Este necesar să acordati o notă")]
        [Range(1, 5, ErrorMessage = "Nota trebuie sa fie între 1 și 5")]
        public int? Rating { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        //-----------------------------------------------------
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        //-----------------------------------------------------
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        //-----------------------------------------------------
    }
}
