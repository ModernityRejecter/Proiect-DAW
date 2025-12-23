using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                PendingProposals = await _context.ProductProposals.CountAsync(p => p.Status == "Pending"),
                RecentOrders = await _context.Orders
                                    .Include(o => o.User)
                                    .OrderByDescending(o => o.Date)
                                    .Take(5)
                                    .ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "User";
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string id, string newRole)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                if (!string.IsNullOrEmpty(newRole))
                {
                    await _userManager.AddToRoleAsync(user, newRole);
                }

                TempData["message"] = $"Rolul utilizatorului {user.Email} a fost schimbat în {newRole}.";
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["message"] = "Utilizatorul a fost șters.";
            }
            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _context.Reviews
                                        .Include(r => r.User)
                                        .Include(r => r.Product)
                                        .OrderByDescending(r => r.Date)
                                        .ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["message"] = "Recenzia a fost ștearsă.";
            }
            return RedirectToAction("ManageReviews");
        }

        public async Task<IActionResult> ManageProposals()
        {
            var proposals = await _context.ProductProposals
                                          .Include(p => p.User)
                                          .Include(p => p.Category)
                                          .Where(p => p.Status == "Pending")
                                          .ToListAsync();
            return View(proposals);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveProposal(int id)
        {
            var proposal = await _context.ProductProposals.FindAsync(id);
            if (proposal == null) return NotFound();

            var newProduct = new Product
            {
                Name = proposal.Name,
                Description = proposal.Description,
                Price = (decimal)proposal.Price,
                Stock = proposal.Stock,
                ImagePath = proposal.ImagePath,
                CategoryId = proposal.CategoryId,
                ProposalId = proposal.Id,
                Rating = 0
            };

            _context.Products.Add(newProduct);
            proposal.Status = "Approved";

            await _context.SaveChangesAsync();
            TempData["message"] = "Produsul a fost aprobat și publicat.";
            return RedirectToAction("ManageProposals");
        }

        [HttpPost]
        public async Task<IActionResult> RejectProposal(int id)
        {
            var proposal = await _context.ProductProposals.FindAsync(id);
            if (proposal != null)
            {
                proposal.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["message"] = "Propunerea a fost respinsă.";
            }
            return RedirectToAction("ManageProposals");
        }

        [HttpGet]
        public async Task<IActionResult> AllProposals(string status)
        {
            var proposals = _context.ProductProposals
                                    .Include(p => p.User)
                                    .Include(p => p.Category)
                                    .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                proposals = proposals.Where(p => p.Status == status);
            }

            var result = await proposals.OrderByDescending(p => p.Id).ToListAsync();

            ViewBag.CurrentFilter = status;

            return View(result);
        }
    }
}