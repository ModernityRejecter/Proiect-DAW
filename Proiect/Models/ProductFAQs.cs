using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class ProductFAQs
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Question field is required")]
        public string Question { get; set; }

        [Required(ErrorMessage = "Answer field is required")]
        public string Answer { get; set; }

        //------------------------------------------------------

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        //------------------------------------------------------
    }
}
