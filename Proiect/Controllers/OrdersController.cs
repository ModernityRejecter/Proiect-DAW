using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            int pageSize = 9;
            var query = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.Date);

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var orders = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.PaginationBaseUrl = "/Orders/Index/?page";

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                                      .Include(o => o.User)
                                      .Include(o => o.Items)
                                      .ThenInclude(i => i.Product)
                                      .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (order.UserId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = await _context.ShoppingCarts
                                     .Include(c => c.Items)
                                     .ThenInclude(i => i.Product)
                                     .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                TempData["message"] = "Coșul este gol.";
                TempData["messageType"] = "alert-warning";
                return RedirectToAction("Index", "Cart");
            }

            foreach (var item in cart.Items)
            {
                if (!item.Product.IsActive)
                {
                    TempData["message"] = $"Produsul {item.Product.Name} nu mai este disponibil.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Cart");
                }

                if (item.Product.Stock < item.Quantity)
                {
                    TempData["message"] = $"Stoc insuficient pentru produsul: {item.Product.Name}";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var order = new Order
            {
                UserId = user.Id,
                Date = DateTime.Now,
                Status = "Inregistrata"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                var product = item.Product;
                product.Stock -= item.Quantity;

                _context.OrderItems.Add(orderItem);
            }

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            TempData["message"] = "Comanda a fost plasată cu succes!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Details", new { id = order.Id });
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                                      .Include(o => o.Items)
                                      .ThenInclude(i => i.Product)
                                      .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (order.UserId != user.Id) return Forbid();

            if (order.Status == "Inregistrata")
            {
                order.Status = "Anulat";

                foreach (var item in order.Items)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["message"] = "Comanda a fost anulată cu succes.";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Comanda nu mai poate fi anulată deoarece a fost deja procesată.";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index");
        }
    }
}