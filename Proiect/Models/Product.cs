using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        // probabil nu o sa fie nevoie de MULTE din validarile care urmeaza pentru ca o sa preluam datele din ProductProposals unde o sa avem aceleasi validari
        [Required(ErrorMessage = "Numele produsului este un câmp obligatoriu")]
        [StringLength(100, ErrorMessage = "Lungimea numelui trebuie să fie de maxim 100 caractere")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Descrierea produsului este un câmp obligatoriu")]
        [StringLength(4000, ErrorMessage = "Lungimea descrierii trebuie să fie de maxim 4000 caractere")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Prețul produsului este un câmp obligatoriu")]
        [Range(0, double.MaxValue, ErrorMessage = "Prețul produsului trebuie sa fie un număr real pozitiv")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stockul produsului este un câmp obligatoriu")]
        [Range(0, double.MaxValue, ErrorMessage = "Stockul produsului trebuie sa fie un număr întreg pozitiv")]
        public int Stock { get; set; }

        // daca un produs n-are review-uri, rating-ul o sa fie null
        [Range(1, 5)]
        public double? Rating { get; set; } = null;

        // posibil sa o fac colectie pentru a stoca mai multe cai pentru imagini
        // nu ma convinge prea tare inserarea cailor pentru folosirea imaginilor dar momentan asta este
        public string? ImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Imagine Produs")]
        [Required(ErrorMessage = "Este necesară încarcarea unei imagini")]
        [MaxFileSize(10 * 1024 * 1024)] // 10 MB
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".webp" })]
        public IFormFile? ImageFile { get; set; }

        // cand stergem un produs, il dezactivam, un fel de soft delete
        public bool IsActive { get; set; } = true;

        //-------------------------------------------------------------
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        //-------------------------------------------------------------
        public int? ProposalId { get; set; }
        public virtual ProductProposal? Proposal { get; set; }

        //-------------------------------------------------------------

        [NotMapped]
        public IEnumerable<SelectListItem> Categ { get; set; } = Enumerable.Empty<SelectListItem>();

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
