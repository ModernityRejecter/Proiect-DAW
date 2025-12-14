using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        // probabil nu o sa fie nevoie de MULTE din validarile care urmeaza pentru ca o sa preluam datele din ProductProposals unde o sa avem aceleasi validari
        [Required(ErrorMessage = "Product name field is required")]
        [StringLength(50, ErrorMessage = "Product name must be at most 50 characters long")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Product description field is required")]
        [StringLength(400, ErrorMessage = "Product description must be at most 400 characters long")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Product price field is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Product price must be a positive number")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Product stock field is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Product stock must be a positive number")]
        public int Stock { get; set; }

        [Range(1, 5)]
        public double? Rating { get; set; } = null;

        // posibil sa o fac colectie pentru a stoca mai multe cai pentru imagini
        // nu ma convinge prea tare inserarea cailor pentru folosirea imaginilor dar momentan asta este
        [Required(ErrorMessage = "Product image path is required")]
        public string ImagePath { get; set; }

        //-------------------------------------------------------------
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        //-------------------------------------------------------------
        public int? ProposalId { get; set; }
        public virtual ProductProposal? Proposal { get; set; }

        //-------------------------------------------------------------
    }
}
