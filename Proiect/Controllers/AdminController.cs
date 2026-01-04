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

        public async Task<IActionResult> ManageUsers(int? page, string searchString, string roleFilter)
        {
            int pageSize = 16;
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.Email.Contains(searchString) ||
                                         u.FirstName.Contains(searchString) ||
                                         u.LastName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                var role = await _roleManager.FindByNameAsync(roleFilter);
                if (role != null)
                {
                    var userIdsInRole = await _context.UserRoles
                        .Where(ur => ur.RoleId == role.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();

                    query = query.Where(u => userIdsInRole.Contains(u.Id));
                }
            }

            query = query.OrderBy(u => u.Email);

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var users = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "User";
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentRole = roleFilter;

            string paginationBaseUrl = "/Admin/ManageUsers/?";
            if (!string.IsNullOrEmpty(searchString)) paginationBaseUrl += $"searchString={searchString}&";
            if (!string.IsNullOrEmpty(roleFilter)) paginationBaseUrl += $"roleFilter={roleFilter}&";

            ViewBag.PaginationBaseUrl = paginationBaseUrl + "page";

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
                // gasim toate propunerile
                var userProposals = await _context.ProductProposals
                                            .Where(p => p.UserId == id)
                                            .ToListAsync();

                // verificam daca exista produs legat de fiecare propunere
                foreach (var proposal in userProposals)
                {
                    var linkedProduct = await _context.Products
                                                .FirstOrDefaultAsync(p => p.ProposalId == proposal.Id);

                    // daca exista produs, rupem legatura si dezactivam produsul
                    if (linkedProduct != null)
                    {
                        linkedProduct.ProposalId = null;
                        linkedProduct.IsActive = false; 
                    }
                }

                await _context.SaveChangesAsync();

                await _userManager.DeleteAsync(user);

                TempData["message"] = "Utilizatorul a fost șters, iar produsele asociate au fost dezactivate.";
            }
            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> ManageReviews(int? page, int? ratingFilter)
        {
            int pageSize = 9;
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.Date)
                .AsQueryable();

            if (ratingFilter.HasValue)
            {
                query = query.Where(r => r.Rating == ratingFilter.Value);
            }

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var reviews = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.CurrentRating = ratingFilter;

            string paginationBaseUrl = "/Admin/ManageReviews/?";
            if (ratingFilter.HasValue) paginationBaseUrl += $"ratingFilter={ratingFilter}&";
            ViewBag.PaginationBaseUrl = paginationBaseUrl + "page";

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

        public async Task<IActionResult> ManageProposals(int? page, string searchString, string statusFilter, string sortOrder)
        {
            int pageSize = 9;
            var query = _context.ProductProposals
                .Include(p => p.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) ||
                                         p.User.Email.Contains(searchString) ||
                                         p.Description.Contains(searchString));
            }

            if (string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(p => p.Status == "Pending" || p.Status == "Rejected");
            }

            switch (sortOrder)
            {
                case "Oldest":
                    query = query.OrderBy(p => p.Id);
                    break;
                default:
                    query = query.OrderByDescending(p => p.Id);
                    break;
            }

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var proposals = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;
            ViewBag.CurrentSort = sortOrder;

            string paginationBaseUrl = "/Admin/ManageProposals/?";
            if (!string.IsNullOrEmpty(searchString)) paginationBaseUrl += $"searchString={searchString}&";
            if (!string.IsNullOrEmpty(statusFilter)) paginationBaseUrl += $"statusFilter={statusFilter}&";
            if (!string.IsNullOrEmpty(sortOrder)) paginationBaseUrl += $"sortOrder={sortOrder}&";

            ViewBag.PaginationBaseUrl = paginationBaseUrl + "page";

            return View(proposals);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveProposal(int id)
        {
            var proposal = await _context.ProductProposals.FindAsync(id);
            Product? product = await _context.Products
                                    .FirstOrDefaultAsync(p => p.ProposalId == id);

            if (proposal == null) return NotFound();
            if (product is null)
            {
                var newProduct = new Product
                {
                    Name = proposal.Name,
                    Description = proposal.Description,
                    Price = proposal.Price,
                    Stock = proposal.Stock,
                    ImagePath = proposal.ImagePath,
                    CategoryId = proposal.CategoryId,
                    ProposalId = proposal.Id,
                    Rating = 0
                };
                _context.Products.Add(newProduct);
                TempData["message"] = "Produsul a fost aprobat și publicat.";
            }
            else
            {
                product.Name = proposal.Name;
                product.Description = proposal.Description;
                product.Price = proposal.Price;
                product.Stock = proposal.Stock;
                product.ImagePath = proposal.ImagePath;
                product.CategoryId = proposal.CategoryId;
                product.IsActive = true;
                TempData["message"] = "Produsul a fost actualizat.";
            }
            proposal.Status = "Approved";

            await _context.SaveChangesAsync();
            return Redirect(Request.Headers.Referer.ToString());
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
            return Redirect(Request.Headers.Referer.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> AllProposals(string searchString, string status, string sortOrder, int? page)
        {
            int pageSize = 16;
            var query = _context.ProductProposals
                                .Include(p => p.User)
                                .Include(p => p.Category)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) ||
                                         p.Description.Contains(searchString) ||
                                         p.User.Email.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(p => p.Status == status);
            }

            switch (sortOrder)
            {
                case "Oldest":
                    query = query.OrderBy(p => p.Id);
                    break;
                default:
                    query = query.OrderByDescending(p => p.Id);
                    break;
            }

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentFilter = status;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;

            string baseUrl = "/Admin/AllProposals/?";
            if (!string.IsNullOrEmpty(searchString)) baseUrl += $"searchString={searchString}&";
            if (!string.IsNullOrEmpty(status)) baseUrl += $"status={status}&";
            if (!string.IsNullOrEmpty(sortOrder)) baseUrl += $"sortOrder={sortOrder}&";

            ViewBag.PaginationBaseUrl = baseUrl + "page";

            return View(result);
        }
        public async Task<IActionResult> ManageOrders(int? page, string searchString, string statusFilter, string timeFilter, string sortOrder)
        {
            int pageSize = 9;
            var query = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var cleanSearch = searchString.Replace("#", "").Trim();

                if (int.TryParse(cleanSearch, out int orderId))
                {
                    query = query.Where(o => o.Id == orderId);
                }
                else
                {
                    query = query.Where(o => o.User.Email.Contains(searchString) || o.User.UserName.Contains(searchString));
                }
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(timeFilter))
            {
                DateTime now = DateTime.Now;
                switch (timeFilter)
                {
                    case "Today":
                        query = query.Where(o => o.Date.Date == now.Date);
                        break;
                    case "Last7Days":
                        query = query.Where(o => o.Date >= now.AddDays(-7));
                        break;
                    case "Last30Days":
                        query = query.Where(o => o.Date >= now.AddDays(-30));
                        break;
                }
            }

            switch (sortOrder)
            {
                case "Oldest":
                    query = query.OrderBy(o => o.Date);
                    break;
                default:
                    query = query.OrderByDescending(o => o.Date);
                    break;
            }

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var orders = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = currentPage;
            ViewBag.LastPage = totalPages;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;
            ViewBag.CurrentTime = timeFilter;
            ViewBag.CurrentSort = sortOrder;

            string paginationBaseUrl = "/Admin/ManageOrders/?";
            if (!string.IsNullOrEmpty(searchString)) paginationBaseUrl += $"searchString={searchString}&";
            if (!string.IsNullOrEmpty(statusFilter)) paginationBaseUrl += $"statusFilter={statusFilter}&";
            if (!string.IsNullOrEmpty(timeFilter)) paginationBaseUrl += $"timeFilter={timeFilter}&";
            if (!string.IsNullOrEmpty(sortOrder)) paginationBaseUrl += $"sortOrder={sortOrder}&";

            ViewBag.PaginationBaseUrl = paginationBaseUrl + "page";

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
                TempData["message"] = $"Comanda #{id} a fost actualizată la statusul: {newStatus}";
                TempData["messageType"] = "alert-success";
            }
            return RedirectToAction("ManageOrders");
        }
        public async Task<IActionResult> ManageProducts(int? page, string searchString, int? categoryId, string statusFilter)
        {
            int pageSize = 9;
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Proposal)
                .ThenInclude(pr => pr.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Active")
                {
                    query = query.Where(p => p.IsActive);
                }
                else if (statusFilter == "Inactive")
                {
                    query = query.Where(p => !p.IsActive);
                }
            }

            query = query.OrderByDescending(p => p.Id);

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var products = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentStatus = statusFilter;

            string paginationBaseUrl = "/Admin/ManageProducts/?";
            if (!string.IsNullOrEmpty(searchString)) paginationBaseUrl += $"searchString={searchString}&";
            if (categoryId.HasValue) paginationBaseUrl += $"categoryId={categoryId}&";
            if (!string.IsNullOrEmpty(statusFilter)) paginationBaseUrl += $"statusFilter={statusFilter}&";

            ViewBag.PaginationBaseUrl = paginationBaseUrl + "page";

            return View(products);
        }

    }
}