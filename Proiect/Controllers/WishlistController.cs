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

        public async Task<IActionResult> Index(int? page)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            int pageSize = 12;

            var wishlist = await _context.Wishlists
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

            var query = _context.WishlistItems
                .Include(i => i.Product)
                .ThenInclude(p => p.Category)
                .Where(i => i.WishlistId == wishlist.Id);

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            wishlist.Items = items;

            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.PaginationBaseUrl = "/Wishlist/Index/?page";

            return View(wishlist);
        }


        [HttpPost]
        public async Task<IActionResult> Remove(int itemId)
        {
            var item = await _context.WishlistItems.FindAsync(itemId);
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["message"] = "Produsul a fost eliminat din favorite";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var wishlist = await _context.Wishlists
                                         .Include(w => w.Items)
                                         .FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = user.Id };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                _context.WishlistItems.Remove(existingItem);
                await _context.SaveChangesAsync();
                TempData["message"] = "Produsul a fost eliminat din favorite";
                TempData["messageType"] = "alert-info";
            }
            else
            {
                var item = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = productId
                };
                _context.WishlistItems.Add(item);
                await _context.SaveChangesAsync();
                TempData["message"] = "Produsul a fost salvat la favorite";
                TempData["messageType"] = "alert-success";
            }

            return Redirect(Request.Headers.Referer.ToString());
        }
    }
}