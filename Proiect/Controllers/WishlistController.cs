using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var wishlist = await _context.Wishlists
                                         .Include(w => w.Items)
                                         .ThenInclude(i => i.Product)
                                         .FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = user.Id,
                    Items = new List<WishlistItem>()
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return View(wishlist);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var wishlist = await _context.Wishlists
                                         .Include(w => w.Items)
                                         .FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = user.Id
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            var exists = wishlist.Items.Any(i => i.ProductId == productId);

            if (!exists)
            {
                var item = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = productId
                };
                _context.WishlistItems.Add(item);
                await _context.SaveChangesAsync();

                TempData["message"] = "Produsul a fost salvat la favorite!";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Produsul este deja în lista de favorite.";
                TempData["messageType"] = "alert-info";
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int itemId)
        {
            var item = await _context.WishlistItems.FindAsync(itemId);
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["message"] = "Produsul a fost eliminat din favorite.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCart(int itemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var wishlistItem = await _context.WishlistItems
                                             .Include(i => i.Product)
                                             .FirstOrDefaultAsync(i => i.Id == itemId);

            if (wishlistItem == null) return NotFound();

            if (wishlistItem.Product.Stock <= 0)
            {
                TempData["message"] = "Produsul nu este în stoc.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var cart = await _context.ShoppingCarts
                                     .Include(c => c.Items)
                                     .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = user.Id };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == wishlistItem.ProductId);

            if (cartItem != null)
            {
                if (wishlistItem.Product.Stock > cartItem.Quantity)
                {
                    cartItem.Quantity++;
                }
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = wishlistItem.ProductId,
                    Quantity = 1,
                    Price = wishlistItem.Product.Price
                };
                _context.CartItems.Add(cartItem);
            }

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            TempData["message"] = "Produsul a fost mutat în coș!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index");
        }
    }
}