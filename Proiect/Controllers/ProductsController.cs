using Google.GenAI.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;
using Proiect.Services;

namespace Proiect.Controllers
{
    public class ProductsController(ApplicationDbContext context, UserManager<ApplicationUser>
        userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IWebHostEnvironment _env = env;

        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder, int? page)
        {
            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString)
                                            || p.Description.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "rating_desc":
                    products = products.OrderByDescending(p => p.Rating);
                    break;
                default:
                    products = products.OrderByDescending(p => p.Id);
                    break;
            }

            int _perPage = 16;
            int totalItems = await products.CountAsync();

            var currentPage = page ?? 1;
            var offset = (currentPage - 1) * _perPage;

            var paginatedProducts = await products.Skip(offset).Take(_perPage).ToListAsync();

            ViewBag.lastPage = (int)Math.Ceiling((double)totalItems / _perPage);
            ViewBag.currentPage = currentPage;

            string paginationBaseUrl = "/Products/Index/?";
            if (!string.IsNullOrEmpty(searchString)) paginationBaseUrl += "searchString=" + searchString + "&";
            if (categoryId.HasValue) paginationBaseUrl += "categoryId=" + categoryId + "&";
            if (!string.IsNullOrEmpty(sortOrder)) paginationBaseUrl += "sortOrder=" + sortOrder + "&";

            ViewBag.PaginationBaseUrl = paginationBaseUrl + "page";

            ViewBag.Categories = await db.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sortOrder;

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.WishlistProductIds = await db.WishlistItems
                    .Where(w => w.Wishlist.UserId == userId)
                    .Select(w => w.ProductId)
                    .ToListAsync();
            }
            else
            {
                ViewBag.WishlistProductIds = new List<int>();
            }

            return View(paginatedProducts);
        }

        public async Task<IActionResult> Show(int id, int? page)
        {
            var product = await db.Products
                .Include(p => p.Category)
                .Include(p => p.Proposal)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            int pageSize = 5;
            var reviewsQuery = db.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.Date);

            int totalReviews = await reviewsQuery.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            var paginatedReviews = await reviewsQuery
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ReviewsList = paginatedReviews;
            ViewBag.TotalReviews = totalReviews;
            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.PaginationBaseUrl = $"/Products/Show/{id}?page";

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var existsInWishlist = await db.WishlistItems
                    .AnyAsync(w => w.Wishlist.UserId == userId && w.ProductId == id);
                ViewBag.IsInWishlist = existsInWishlist;
            }
            else
            {
                ViewBag.IsInWishlist = false;
            }

            return View(product);
        }

        //[HttpPost]
        //[Authorize(Roles = "Admin,Colaborator")]
        //public IActionResult SoftDelete(int id)
        //{
        //    Product? product = db.Products
        //        .Include(p => p.Proposal)
        //        .FirstOrDefault(p => p.Id == id);

        //    if (product is null)
        //    {
        //        return NotFound();
        //    }
        //    if(product.Proposal.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
        //    {
        //        product.IsActive = false;
        //        db.SaveChanges();
        //    }
        //    else
        //    {
        //        TempData["message"] = "Nu aveți dreptul să stergeți produsul altui colaborator";
        //        TempData["messageType"] = "alert-danger";
        //    }

        //    return Redirect(Request.Headers.Referer.ToString());
        //}

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            Product? product = await db.Products
                .Include(p => p.Proposal)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }
            string? proposalUserId = product.Proposal?.UserId;
            string currentUserId = _userManager.GetUserId(User);
            if (User.IsInRole("Admin") || (proposalUserId != null && proposalUserId == currentUserId))
            {
                product.IsActive = !product.IsActive;
                await context.SaveChangesAsync();
                TempData["message"] = $"Produsul {product.Name} este acum {(product.IsActive ? "Activ" : "Inactiv")}.";
                TempData["messageType"] = "alert-info";
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să stergeți produsul altui colaborator";
                TempData["messageType"] = "alert-danger";
            }
            return Redirect(Request.Headers.Referer.ToString());
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Colaborator")]
        public async Task<IActionResult> Edit(int id)
        {
            Product? product = await db.Products
                                       .Include(p => p.Category)
                                       .Include(p => p.Proposal)
                                       .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }
            product.Categ = GetAllCategories();

            if (product.Proposal == null)
            {
                if (User.IsInRole("Admin"))
                {
                    TempData["message"] = "Acest produs nu mai are o propunere asociată (autor șters). Editarea directă nu este implementată încă.";
                    TempData["messageType"] = "alert-warning";
                    return RedirectToAction("Show", new { id = product.Id });
                }
                else
                {
                    TempData["message"] = "Acest produs nu mai poate fi modificat deoarece autorul a fost șters.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("MyProducts");
                }
            }
            if (product.Proposal.UserId == _userManager.GetUserId(User))
            {

                var proposal = product.Proposal;

                proposal.Name = product.Name;
                proposal.Description = product.Description;
                proposal.Price = product.Price;
                proposal.Stock = product.Stock;
                proposal.CategoryId = (int)product.CategoryId;
                proposal.ImagePath = product.ImagePath;



                db.Update(proposal);
                await db.SaveChangesAsync();

                return Redirect("/ProductProposals/Edit/" + proposal.Id);
            }
            else
            {
                TempData["message"] = "Nu dețineți drepturile necesare ca să modificați acest produs";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("MyProducts");
            }
        }
        public IActionResult Search(string searchString)
        {
            return RedirectToAction("Index", new { searchString = searchString });
        }

        [HttpPost]
        public async Task<IActionResult> AskProductAssistant(
            [FromServices] Proiect.Services.GeminiService geminiService,
            [FromForm] int productId,
            [FromForm] string question)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question))
                {
                    return Json(new { success = false, answer = "Te rog scrie o întrebare." });
                }

                var product = await db.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                    return Json(new { success = false, answer = "Produsul nu a fost găsit." });

                var existingFaqs = await db.ProductFAQs
                    .Where(f => f.ProductId == productId)
                    .ToListAsync();

                string aiResponse = await geminiService.GetAnswerAsync(product, existingFaqs, question);

                if (aiResponse != "NU_STIU")
                {
                    bool alreadyExists = existingFaqs.Any(f => f.Question.Equals(question, StringComparison.OrdinalIgnoreCase));

                    if (!alreadyExists)
                    {
                        var newFaq = new Proiect.Models.ProductFAQs
                        {
                            ProductId = productId,
                            Question = question,
                            Answer = aiResponse
                        };
                        db.ProductFAQs.Add(newFaq);
                        await db.SaveChangesAsync();
                    }
                    return Json(new { success = true, answer = aiResponse });
                }
                else
                {
                    var newFaq = new Proiect.Models.ProductFAQs
                    {
                        ProductId = productId,
                        Question = question,
                        Answer = null
                    };
                    db.ProductFAQs.Add(newFaq);
                    await db.SaveChangesAsync();

                    return Json(new { success = true, answer = "Momentan nu avem detalii despre acest aspect." });
                }
            }
            catch
            {
                return Json(new { success = false, answer = "A apărut o eroare internă." });
            }
        }

        //---------------------------------------------------------------------------
        // metode interne
        [Authorize(Roles = "Colaborator,Admin")]
        public async Task<IActionResult> MyProducts(int? page, string searchString, int? categoryId, string statusFilter, string sortOrder)
        {
            var currentUserId = _userManager.GetUserId(User);
            int pageSize = 8;

            var query = db.Products
                .Include(p => p.Category)
                .Include(p => p.Proposal)
                .Where(p => p.Proposal.UserId == currentUserId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
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

            switch (sortOrder)
            {
                case "PriceAsc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "PriceDesc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
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

            var myProducts = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = await db.Categories.OrderBy(c => c.Name).ToListAsync();

            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentStatus = statusFilter;
            ViewBag.CurrentSort = sortOrder;

            string paginationBaseUrl = "/Products/MyProducts/?";
            if (!string.IsNullOrEmpty(searchString)) paginationBaseUrl += $"searchString={searchString}&";
            if (categoryId.HasValue) paginationBaseUrl += $"categoryId={categoryId}&";
            if (!string.IsNullOrEmpty(statusFilter)) paginationBaseUrl += $"statusFilter={statusFilter}&";
            if (!string.IsNullOrEmpty(sortOrder)) paginationBaseUrl += $"sortOrder={sortOrder}&";

            ViewBag.PaginationBaseUrl = paginationBaseUrl + "page";

            return View(myProducts);
        }

        private IEnumerable<SelectListItem> GetAllCategories()
        {
            var list = new List<SelectListItem>();
            var categories = db.Categories.OrderBy(c => c.Name).ToList();
            foreach (var category in categories)
            {
                list.Add(new SelectListItem { 
                    Value = category.Id.ToString(), 
                    Text = category.Name }
                );
            }
            return list;
        }

        private bool IsValidImageSignature(IFormFile file)
        {
            return true;
        }
    
    }
}
