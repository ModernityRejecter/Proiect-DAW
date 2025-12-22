using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;
using Microsoft.AspNetCore.Hosting;

namespace Proiect.Controllers
{
    public class ProductProposalsController(ApplicationDbContext context, UserManager<ApplicationUser>
        userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IWebHostEnvironment _env = env;

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
        public async Task<IActionResult> New(ProductProposal proposal)
        {
            proposal.UserId = _userManager.GetUserId(User);

            if (proposal.ImageFile != null)
            {
                if (!IsValidImageSignature(proposal.ImageFile))
                {
                    ModelState.AddModelError("ImageFile", "Fișierul nu este o imagine validă sau este corupt.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (proposal.ImageFile != null && proposal.ImageFile.Length > 0)
                    {
                        var storagePath = Path.Combine(_env.WebRootPath, "images", "products");
                        if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(proposal.ImageFile.FileName);
                        var filePath = Path.Combine(storagePath, fileName);

                        using (var memoryStream = new MemoryStream())
                        {
                            await proposal.ImageFile.CopyToAsync(memoryStream);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                memoryStream.Position = 0;
                                await memoryStream.CopyToAsync(fileStream);
                            }
                        }
                        proposal.ImagePath = "/images/products/" + fileName;
                    }

                    db.ProductProposals.Add(proposal);
                    await db.SaveChangesAsync();

                    TempData["message"] = "Produsul a fost propus cu succes!";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("MyProposals");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Eroare tehnică la salvare: " + ex.Message);
                }
            }
            proposal.Categ = GetAllCategories();
            return View(proposal);
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
                return RedirectToAction("MyProposals");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator")]
        public async Task<IActionResult> Edit(ProductProposal requestedProposal, int id)
        {
            ProductProposal? proposal = await db.ProductProposals.FindAsync(id);
            if(proposal is null)
            {
                return NotFound();
            }
            else
            {
                if (requestedProposal.ImageFile == null)
                {
                    ModelState.Remove("ImageFile");
                }
                if (ModelState.IsValid)
                {
                    var currentUserId = _userManager.GetUserId(User);
                    if (proposal.UserId == currentUserId)
                    {
                        proposal.Stock = requestedProposal.Stock;
                        proposal.Price = requestedProposal.Price;
                        proposal.Name = requestedProposal.Name;
                        proposal.Description = requestedProposal.Description;
                        proposal.CategoryId = requestedProposal.CategoryId;

                        if (requestedProposal.ImageFile != null && requestedProposal.ImageFile.Length > 0)
                        {

                            var storagePath = Path.Combine(_env.WebRootPath, "images", "products");
                            if (!Directory.Exists(storagePath))
                            {
                                Directory.CreateDirectory(storagePath);
                            }

                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(requestedProposal.ImageFile.FileName);
                            var filePath = Path.Combine(storagePath, fileName);

                            using (var memoryStream = new MemoryStream())
                            {
                                await requestedProposal.ImageFile.CopyToAsync(memoryStream);
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    memoryStream.Position = 0;
                                    await memoryStream.CopyToAsync(fileStream);
                                }
                            }

                            if (!string.IsNullOrEmpty(proposal.ImagePath))
                            {
                                var oldPath = Path.Combine(_env.WebRootPath, proposal.ImagePath.TrimStart('/'));
                                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                            }
                            proposal.ImagePath = "/images/products/" + fileName;
                        }

                        await db.SaveChangesAsync();
                        TempData["message"] = "Propunerea a fost actualizată";
                        TempData["messageType"] = "alert-success";
                        return RedirectToAction("MyProposals");
                    }
                    else
                    {
                        TempData["message"] = "Nu dețineți drepturile necesare ca să modificați propunerea altui utilizator";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("MyProposals");
                    }
                }
                else 
                {
                    requestedProposal.Categ = GetAllCategories();
                    requestedProposal.ImagePath = proposal.ImagePath;
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
                if(proposal.ImagePath is not null || proposal.ImagePath != "")
                {
                    var filePath = Path.Combine(_env.WebRootPath, proposal.ImagePath.TrimStart('/').Replace('/', '\\'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                db.ProductProposals.Remove(proposal);
                db.SaveChanges();
                TempData["message"] = "Propunerea a fost stearsă";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Nu dețineți drepturile necesare ca să ștergeți propunerea altui utilizator";
                TempData["messageType"] = "alert-danger";
            }
            return RedirectToAction("MyProposals");
        }

        [Authorize(Roles = "Admin,Colaborator")]
        public IActionResult Show(int id)
        {
            ProductProposal? proposal = db.ProductProposals
                                            .Include(pp => pp.Category)
                                            .Include(pp => pp.User)
                                            .FirstOrDefault(p => p.Id == id);

            if(proposal is null)
            {
                return NotFound();
            }
            if(proposal.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(proposal);
            }
            else
            {
                TempData["message"] = "Nu dețineți drepturile necesare ca să ștergeți propunerea altui utilizator";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("MyProposals");
            }
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

        [NonAction]
        private bool IsValidImageSignature(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var reader = new BinaryReader(stream);
                var headerBytes = reader.ReadBytes(10);

                var signatures = new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF }, // JPEG
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, // PNG
                    new byte[] { 0x52, 0x49, 0x46, 0x46 }, // WEBP
                };

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
            catch
            {
                return false;
            }
        }
    }
}
