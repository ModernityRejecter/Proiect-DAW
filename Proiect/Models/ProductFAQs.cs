using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class ProductFAQs
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Este necesar să inserați o întrebare")]
        public string Question { get; set; }

        public string? Answer { get; set; }

        //------------------------------------------------------

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        //------------------------------------------------------
    }
}
