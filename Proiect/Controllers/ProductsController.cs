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

        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder)
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

            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> Show(int id)
        {
            var product = await db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .Include(p => p.Proposal)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

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

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator")]
        public IActionResult SoftDelete(int id)
        {
            Product? product = db.Products
                .Include(p => p.Proposal)
                .FirstOrDefault(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }
            if(product.Proposal.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                product.IsActive = false;
                db.SaveChanges();
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să stergeți produsul altui colaborator";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("MyProducts");
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
        public async Task<IActionResult> MyProducts()
        {
            var currentUserId = _userManager.GetUserId(User);

            var myProducts = await db.Products
                                     .Include(p => p.Category)
                                     .Include(p => p.Proposal)
                                     .Where(p => p.Proposal.UserId == currentUserId /*&& p.IsActive == true*/)
                                     .OrderByDescending(p => p.Id)
                                     .ToListAsync();

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
