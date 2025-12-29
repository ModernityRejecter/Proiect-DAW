using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele categoriei este un câmp obligatoriu")]
        [StringLength(100, ErrorMessage = "Lungimea numelui trebuie să fie de maxim 100 de caractere")]
        public string Name { get; set; } = default!;
    }
}
