namespace Proiect.Models
{
    public class ProductsList
    {
        public int Id { get; set; }
        
        public ICollection<Product> Products { get; set; } = new List<Product>();

        //-----------------------------------------------------

        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        //-----------------------------------------------------

    }
}
