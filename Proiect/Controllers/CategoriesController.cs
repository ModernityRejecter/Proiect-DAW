using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;
using System.Linq;


namespace Proiect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task<IActionResult> Index(int? page)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"]?.ToString();
            }

            int pageSize = 40;
            var query = db.Categories.OrderBy(c => c.Name);

            int totalItems = await query.CountAsync();
            int currentPage = page ?? 1;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var categories = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.lastPage = totalPages;
            ViewBag.currentPage = currentPage;
            ViewBag.PaginationBaseUrl = "/Categories/Index/?page";
            ViewBag.Categories = categories;

            return View();
        }

        public ActionResult Show(int id)
        {
            Category? category = db.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                TempData["message"] = "Categoria a fost adaugata";
                return RedirectToAction("Index");
            }
            else
            {
                return View(cat);
            }
        }

        public ActionResult Edit(int id)
        {
            Category? category = db.Categories.Find(id);

            if (category is null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category? category = db.Categories.Find(id);

            if (category is null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                category.Name = requestCategory.Name;
                db.SaveChanges();
                TempData["message"] = "Categoria a fost modificata!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestCategory);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Category? category = db.Categories.Find(id);

            if (category is null)
            {
                return NotFound();
            }
            else
            {
                var hasProducts = db.Products.Any(p => p.CategoryId == category.Id);
                if (hasProducts)
                {
                    TempData["message"] = "Categoria nu poate fi stearsa deoarece are produse asociate.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
                db.Categories.Remove(category);
                TempData["message"] = "Categoria a fost stearsa";
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
    }
}