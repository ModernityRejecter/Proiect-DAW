using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class ProductProposal
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele produsului este un câmp obligatoriu")]
        [StringLength(100, ErrorMessage = "Lungimea numelui trebuie să fie de maxim 100 caractere")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Descrierea produsului este un câmp obligatoriu")]
        [StringLength(4000, ErrorMessage = "Lungimea descrierii trebuie să fie de maxim 4000 caractere")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Prețul produsului este un câmp obligatoriu")]
        [Range(0, double.MaxValue, ErrorMessage = "Prețul trebuie să fie un număr real pozitiv")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stockul produsului este un câmp obligatoriu")]
        [Range(0, double.MaxValue, ErrorMessage = "Stockul trebuie să fie un număr întreg pozitiv")]
        public int Stock { get; set; }
        public string? ImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Imagine produs")]
        [Required(ErrorMessage = "Este necesară încarcarea unei imagini")]
        [MaxFileSize(10 * 1024 * 1024)] // 10 MB
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".webp" })]
        public IFormFile ImageFile { get; set; }

        public string Status { get; set; } = "Pending";

        //-------------------------------------------------------------
        
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        //-------------------------------------------------------------

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        //-------------------------------------------------------------
        [NotMapped]
        public IEnumerable<SelectListItem> Categ { get; set; } = Enumerable.Empty<SelectListItem>();

        public virtual ICollection<ProposalFeedback> Feedbacks { get; set; } = new List<ProposalFeedback>();

        //-------------------------------------------------------------
    }
}
