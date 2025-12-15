using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class ProductProposal
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name field is required")]
        [StringLength(50, ErrorMessage = "Product name must be at most 50 characters long")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Product description field is required")]
        [StringLength(400, ErrorMessage = "Product description must be at most 400 characters long")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Product price field is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Product price must be a positive number")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Product stock field is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Product stock must be a positive number")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Product image path is required")]
        public string ImagePath { get; set; }

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
    }
}
