using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    [Authorize(Roles = "Colaborator,Admin")]
    public class CollaboratorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CollaboratorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var viewModel = new CollaboratorDashboardViewModel
            {
                TotalProposals = await _context.ProductProposals
                                    .CountAsync(p => p.UserId == currentUserId),

                ApprovedProducts = await _context.ProductProposals
                                    .CountAsync(p => p.UserId == currentUserId && p.Status == "Approved"),

                PendingProposals = await _context.ProductProposals
                                    .CountAsync(p => p.UserId == currentUserId && p.Status == "Pending"),

                RejectedProposals = await _context.ProductProposals
                                    .CountAsync(p => p.UserId == currentUserId && p.Status == "Rejected"),

                RecentProposals = await _context.ProductProposals
                                    .Include(p => p.Category)
                                    .Where(p => p.UserId == currentUserId)
                                    .OrderByDescending(p => p.Id)
                                    .Take(5)
                                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}