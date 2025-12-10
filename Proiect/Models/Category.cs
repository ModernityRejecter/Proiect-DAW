using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name field is required")]
        [StringLength(50, ErrorMessage = "Category name string must be at most 50 characters long")]
        public string Name { get; set; } = default!;
    }
}
