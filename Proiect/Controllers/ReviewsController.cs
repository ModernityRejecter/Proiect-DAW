using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    public class ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        //public IActionResult Index()
        //{
        //    return View();
        //}
        [Authorize(Roles = "Admin,Colaborator,User")]
        public IActionResult Edit(int id)
        {
            Review? review = db.Reviews.Find(id);
            if (review is null)
            {
                return NotFound();
            }
            else
            {
                if(review.UserId == _userManager.GetUserId(User))
                {
                    return View(review);
                }
                else
                {
                    TempData["message"] = "Nu aveți dreptul să editați recenzia altui utilizator";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", "Products");
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator,User")]
        public IActionResult Edit(int id, Review editedReview)
        {
            Review? review = db.Reviews.Find(id);
            if (review is null)
            {
                return NotFound();
            }
            else
            {
                if(review.UserId == _userManager.GetUserId(User))
                {
                    if (string.IsNullOrWhiteSpace(editedReview.Title) && editedReview.Rating.HasValue)
                    {
                        editedReview.Title = GetLabelByRating(editedReview.Rating.Value);
                    }

                    if (ModelState.IsValid)
                    {
                        review.Title = editedReview.Title;
                        review.Description = editedReview.Description;
                        review.Rating = editedReview.Rating;
                        review.Date = DateTime.Now;

                        db.SaveChanges();
                        return Redirect("/Products/Show/" + review.ProductId);
                    }
                    else
                    {
                        TempData["message"] = "Recenzia a fost actualizată";
                        TempData["messageType"] = "alert-success";
                        return View(editedReview);
                    }
                }
                else
                {
                    TempData["message"] = "Nu aveți dreptul să editați recenzia altui utilizator";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Products/Show/" + review.ProductId);
                }
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator,User")]
        public IActionResult Delete(int id)
        {
            Review? review = db.Reviews.Find(id);
            if(review is null)
            {
                return NotFound();
            }
            if(review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(review);
                db.SaveChanges();
                TempData["message"] = "Recenzia a fost ștearsă";
                TempData["messageType"] = "alert-success";
                return Redirect(Request.Headers.Referer.ToString());
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți recenzia altui utilizator";
                TempData["messageType"] = "alert-danger";
                return Redirect(Request.Headers.Referer.ToString());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator,User")]
        public async Task<IActionResult> New(Review review)
        {
            review.Date = DateTime.Now;
            review.UserId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(review.Title) && review.Rating.HasValue)
            {
                review.Title = GetLabelByRating(review.Rating.Value);
            }

            if (ModelState.IsValid)
            {
                db.Reviews.Add(review);
                await db.SaveChangesAsync();

                var product = await db.Products
                    .Include(p => p.Reviews)
                    .FirstOrDefaultAsync(p => p.Id == review.ProductId);

                if (product != null && product.Reviews.Any())
                {
                    product.Rating = product.Reviews.Average(r => r.Rating);
                    await db.SaveChangesAsync();
                }

                TempData["message"] = "Recenzia a fost adăugată!";
                TempData["messageType"] = "alert-success";

                return Redirect("/Products/Show/" + review.ProductId);
            }
            else
            {
                TempData["message"] = "Nu s-a putut adăuga recenzia. Verifică datele introduse.";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Products/Show/" + review.ProductId);
            }
        }

        private string GetLabelByRating(int rating)
        {
            return rating switch
            {
                1 => "Foarte slab",
                2 => "Slab",
                3 => "Mediu",
                4 => "Bun",
                5 => "Excelent",
                _ => "Recenzie"
            };
        }
    }
}
