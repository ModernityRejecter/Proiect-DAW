using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    public class ProductProposalsController(ApplicationDbContext context, UserManager<ApplicationUser>
        userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        [Authorize(Roles = "Admin,Colaborator")]
        public IActionResult Index()
        {
            var proposals = db.ProductProposals
                                .Include(p => p.Category)
                                .Include(p => p.User);

            ViewBag.Proposals = proposals;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.MessageType = TempData["messageType"];
            }
            return View();
        }

        [Authorize(Roles = "Colaborator,Admin")]
        public async Task<IActionResult> MyProposals()
        {
            var currentUserId = _userManager.GetUserId(User);

            var proposals = await GetOwnProposals();

            ViewBag.Proposals = proposals;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.MessageType = TempData["messageType"];
            }

            return View();
        }

        [Authorize(Roles = "Admin,Colaborator")]
        public IActionResult New()
        {
            ProductProposal proposal = new ProductProposal();
            proposal.Categ = GetAllCategories();
            return View(proposal);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator")]
        public IActionResult New(ProductProposal proposal)
        {
            proposal.UserId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                db.ProductProposals.Add(proposal);
                db.SaveChanges();
                TempData["message"] = "Product added succesfully";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                proposal.Categ = GetAllCategories();
                return View(proposal);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Colaborator")]
        public IActionResult Edit(int id)
        {
            ProductProposal proposal = db.ProductProposals
                                            .Include(p => p.Category)
                                            .Where(p => p.Id == id)
                                            .FirstOrDefault();
            if (proposal is null)
            {
                return NotFound();
            }
            proposal.Categ = GetAllCategories();

            if (proposal.UserId == _userManager.GetUserId(User))
            {
                return View(proposal);
            }
            else
            {
                TempData["message"] = "Nu dețineți drepturile necesare ca să modificați propunerea altui utilizator";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator")]
        public IActionResult Edit(ProductProposal requestedProposal, int id)
        {
            ProductProposal? proposal = db.ProductProposals.Find(id);

            if(proposal is null)
            {
                return NotFound();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if(requestedProposal.UserId == _userManager.GetUserId(User))
                    {
                        proposal.Stock = requestedProposal.Stock;
                        proposal.Price = requestedProposal.Price;
                        proposal.ImagePath = requestedProposal.ImagePath;
                        proposal.Name = requestedProposal.Name;
                        proposal.Description = requestedProposal.Description;
                        proposal.CategoryId = requestedProposal.CategoryId;
                        db.SaveChanges();
                        TempData["message"] = "Propunerea a fost actualizată";
                        TempData["messageType"] = "alert-success";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu dețineți drepturile necesare ca să modificați propunerea altui utilizator";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("Index");
                    }
                }
                else 
                {
                    requestedProposal.Categ = GetAllCategories();
                    return View(requestedProposal);
                }
            }

        }

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator")]
        public IActionResult Delete(int id)
        {
            ProductProposal? proposal = db.ProductProposals.Find(id);

            //nu cred ca este posibil sa fie null vreodata
            if(proposal is null)
            {
                return NotFound();
            }
            
            if(proposal.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.ProductProposals.Remove(proposal);
                db.SaveChanges();
                TempData["message"] = "Propunerea a fost stearsă";
                TempData["messageType"] = "alertSuccess";
            }
            else
            {
                TempData["message"] = "Nu dețineți drepturile necesare ca să ștergeți propunerea altui utilizator";
                TempData["messageType"] = "alert-danger";
            }
            return RedirectToAction("Index");
        }

        //------------------------------------------------------
        // metode interne

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var list = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                list.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name
                });
            }
            return list;
        }

        [NonAction]
        public async Task<ICollection<ProductProposal>> GetOwnProposals()
        {
            var currentUserId = _userManager.GetUserId(User);
            if(currentUserId is null)
            {
                return new List<ProductProposal>();
            }
            ICollection<ProductProposal> ownProposals = await db.ProductProposals
                                                                .Where(op => op.UserId == currentUserId)
                                                                .Include(op => op.Category)
                                                                .OrderByDescending(op => op.Id)
                                                                .ToListAsync();

            return ownProposals;
        }
    }
}
