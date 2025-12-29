using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;
using System.Runtime.InteropServices;

namespace Proiect.Controllers
{
    public class ProposalFeedbacksController(ApplicationDbContext context, UserManager<ApplicationUser>
        userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        //[Authorize(Roles = "Admin,Colaborator")]

        //public IActionResult Index()
        //{
        //    ICollection<ProposalFeedback> texts = new List<ProposalFeedback>();
        //    texts = db.ProposalFeedbacks.ToList();
        //    return View(texts);
        //}
        [Authorize(Roles = "Admin,Colaborator")]
        public async Task<IActionResult> Chat(int id)
        {
            ProductProposal? proposal = await db.ProductProposals
                                            .Include(p => p.User)
                                            .Include(p => p.Feedbacks)
                                                .ThenInclude(f => f.User)
                                            .FirstOrDefaultAsync(p => p.Id == id);
            if (proposal is null)
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);

            if (proposal.UserId != currentUserId && !User.IsInRole("Admin")) 
            {
                return Forbid();
            }

            ICollection<ProposalFeedback> unreadTexts = await db.ProposalFeedbacks
                                                            .Where(f => f.UserId != currentUserId && f.IsRead == false)
                                                            .ToListAsync();
            if (unreadTexts.Count > 0)
            {
                foreach (ProposalFeedback text in unreadTexts)
                {
                    text.IsRead = true;
                }
                db.SaveChangesAsync();
            }
            proposal.Feedbacks = proposal.Feedbacks.
                OrderBy(f => f.Date).
                ToList();

            return View(proposal);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator")]
        public async Task<IActionResult> Chat([FromForm] int proposalId, [FromForm] string message)
        {
            if (message == "" || message is null)
            {
                return Redirect("/ProposalFeedbacks/Chat/" + proposalId);
            }

            var proposal = await db.ProductProposals.FindAsync(proposalId);
            if (proposal == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            if (proposal.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var feedback = new ProposalFeedback
            {
                ProposalId = proposalId,
                UserId = currentUserId,
                Message = message,
                Date = DateTime.Now,
                IsRead = false
            };

            db.ProposalFeedbacks.Add(feedback);
            await db.SaveChangesAsync();

            return Redirect("/ProposalFeedbacks/Chat/" + proposalId);
        }
        //------------------------------------------------------------------------
        // metode interne
        //private ICollection<ProposalFeedback> GetAllTexts()
        //{


        //}

    }
}
