using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    [Authorize(Roles = "Admin,Colaborator")]
    public class ProductFAQsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductFAQsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.ProductFAQs
                                .Include(f => f.Product)
                                .ThenInclude(p => p.Proposal)
                                .Where(f => f.Answer == null);

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(f => f.Product.Proposal.UserId == userId);
            }

            var faqs = await query.OrderByDescending(f => f.Id).ToListAsync();
            return View(faqs);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var faq = await _context.ProductFAQs
                                    .Include(f => f.Product)
                                    .ThenInclude(p => p.Proposal)
                                    .FirstOrDefaultAsync(f => f.Id == id);

            if (faq == null) return NotFound();

            if (!User.IsInRole("Admin") && faq.Product.Proposal.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(faq);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string answer)
        {
            var faq = await _context.ProductFAQs
                                    .Include(f => f.Product)
                                    .ThenInclude(p => p.Proposal)
                                    .FirstOrDefaultAsync(f => f.Id == id);

            if (faq == null) return NotFound();

            if (!User.IsInRole("Admin") && faq.Product.Proposal.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(answer))
            {
                ModelState.AddModelError("Answer", "Răspunsul este obligatoriu.");
                return View(faq);
            }

            faq.Answer = answer;
            await _context.SaveChangesAsync();

            TempData["message"] = "Răspunsul a fost salvat cu succes!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var faq = await _context.ProductFAQs
                                    .Include(f => f.Product)
                                    .ThenInclude(p => p.Proposal)
                                    .FirstOrDefaultAsync(f => f.Id == id);

            if (faq == null) return NotFound();

            if (!User.IsInRole("Admin") && faq.Product.Proposal.UserId != _userManager.GetUserId(User))
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți această întrebare.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction(nameof(Index));
            }

            _context.ProductFAQs.Remove(faq);
            await _context.SaveChangesAsync();

            TempData["message"] = "Întrebarea a fost ștearsă.";
            return RedirectToAction(nameof(Index));
        }
    }
}