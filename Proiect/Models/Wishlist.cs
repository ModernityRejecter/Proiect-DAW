namespace Proiect.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        //-----------------------------------------

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        //-----------------------------------------

        public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();

    }
}
