using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = await _context.ShoppingCarts
                                     .Include(c => c.Items)
                                     .ThenInclude(i => i.Product)
                                     .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = user.Id,
                    Items = new List<CartItem>()
                };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            ViewBag.TotalPrice = cart.Items.Sum(i => i.Quantity * i.Price);

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity)
            {
                TempData["message"] = "Produsul nu este disponibil în cantitatea selectată.";
                TempData["messageType"] = "alert-danger";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            var cart = await _context.ShoppingCarts
                                     .Include(c => c.Items)
                                     .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = user.Id
                };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                if (product.Stock >= cartItem.Quantity + quantity)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    TempData["message"] = "Stoc insuficient pentru a adăuga mai multe produse.";
                    TempData["messageType"] = "alert-warning";
                }
            }
            else
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    CartId = cart.Id,
                    Quantity = quantity,
                    Price = product.Price
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            TempData["message"] = "Produsul a fost adăugat în coș!";
            TempData["messageType"] = "alert-success";

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["message"] = "Produsul a fost eliminat din coș.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            var cartItem = await _context.CartItems
                                         .Include(ci => ci.Product)
                                         .FirstOrDefaultAsync(ci => ci.Id == itemId);

            if (cartItem != null)
            {
                if (quantity > 0 && quantity <= cartItem.Product.Stock)
                {
                    cartItem.Quantity = quantity;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["message"] = "Cantitate invalidă sau stoc insuficient.";
                    TempData["messageType"] = "alert-danger";
                }
            }

            return RedirectToAction("Index");
        }
    }
}