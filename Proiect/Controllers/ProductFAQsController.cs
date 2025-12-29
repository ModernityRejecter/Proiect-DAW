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
    }
}