using Microsoft.AspNetCore.Identity;

namespace Proiect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public virtual ICollection<ProductProposal> Proposals { get; set; } = new List<ProductProposal>();
        public virtual ICollection<ProposalFeedback> Feedbacks { get; set; } = new List<ProposalFeedback>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ShoppingCart? ShoppingCart { get; set; }
        public virtual Wishlist? Wishlist { get; set; }
    }
}
