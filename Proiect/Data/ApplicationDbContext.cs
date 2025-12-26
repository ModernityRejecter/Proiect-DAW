using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proiect.Models;

namespace Proiect.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //unicitate nume + precizie pret
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>()
                .HasIndex(p => p.Name)
                .IsUnique();

            builder.Entity<OrderItem>()
                .Property(o => o.Price)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<ProductProposal>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            // rezolvare problema la delete
            builder.Entity<Product>()
                .HasOne(p => p.Proposal)
                .WithOne()
                .HasForeignKey<Product>(p => p.ProposalId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProposalFeedback>()
                .HasOne(p => p.Proposal)
                .WithMany(collection => collection.Feedbacks)
                .HasForeignKey(p => p.ProposalId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductProposal> ProductProposals { get; set; }
        public DbSet<ProposalFeedback> ProposalFeedbacks { get; set; }
        public DbSet<ProductFAQs> ProductFAQs { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

    }
}
