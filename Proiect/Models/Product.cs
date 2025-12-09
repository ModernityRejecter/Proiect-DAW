using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name can't be null")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Product description can't be null")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Product price ")]
        public double Price { get; set; }


    }
}
