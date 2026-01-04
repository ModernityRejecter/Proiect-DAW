using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using System.Text.Json;

namespace Proiect.Models
{
    public static class SeedData
    {
        private class ProposalSeedDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public string CategoryName { get; set; }
            public string ImageFile { get; set; }
            public string Status { get; set; }
        }

        // funny reviews because yes
        private static Dictionary<string, List<Review>> FunnyReviews = new Dictionary<string, List<Review>>
        {
            {
                "Haine pentru pește", new List<Review>
                {
                    new Review { Title = "Fashion Week în acvariu", Description = "Guppy-ul meu arată fabulos, dar cred că i-am luat o mărime prea mare la aripioara dorsală. Înoată doar în cercuri acum.", Rating = 4 },
                    new Review { Title = "Nu recomand", Description = "Materialul nu respiră sub apă. Peștele meu transpiră.", Rating = 1 },
                    new Review { Title = "Divertisment pur", Description = "Pisica se uită la acvariu de parcă se uită la Netflix. 10/10.", Rating = 5 }
                }
            },
            {
                "Desenele copilului meu", new List<Review>
                {
                    new Review { Title = "O capodoperă", Description = "Această girafă cu 5 picioare reprezintă angoasa societății moderne. Picasso ar fi gelos.", Rating = 5 },
                    new Review { Title = "Suspect", Description = "Am cumpărat desenele propriului copil de aici pentru că le pierdusem pe cele originale. Soția nu și-a dat seama.", Rating = 5 }
                }
            },
            {
                "Smart Watch cu Bunica", new List<Review>
                {
                    new Review { Title = "Prea multă grijă", Description = "M-a notificat la 3 dimineața să-mi pun șosete în picioare. E incredibil.", Rating = 5 },
                    new Review { Title = "Funcție lipsă", Description = "Nu măsoară caloriile, măsoară cât de 'tras la față' ești. Îmi tot zice să mai mănânc.", Rating = 4 }
                }
            },
            {
                "Scutece pentru hamster", new List<Review>
                {
                    new Review { Title = "Dificil de montat", Description = "Hamsterul m-a mușcat de 3 ori și acum arată ca un luptător de sumo mic și păros.", Rating = 3 }
                }
            },
            {
                "Covrig pentru camera copilului", new List<Review>
                {
                    new Review { Title = "Delicios... vizual", Description = "Copilul a încercat să îl mănânce. Eu am încercat să îl mănânc. Câinele a reușit.", Rating = 4 }
                }
            },
             {
                "Vreau sa aud numai bine", new List<Review>
                {
                    new Review { Title = "Functioneaza!", Description = "Seful tipa la mine, dar eu auzeam sunet de valuri si pescarusi. M-a concediat, dar am plecat zambind.", Rating = 5 }
                }
            },
            {
                "Bibelou Bunica", new List<Review>
                {
                    new Review { Title = "Frica", Description = "L-am pus pe masa si acum merg prin casa pe varfuri involuntar. Imi este frica sa respir langa el.", Rating = 5 }
                }
            },
             {
                "Cos Cumparaturi eMAG", new List<Review>
                {
                    new Review { Title = "Prea realist", Description = "Mi-a trimis notificare ca am uitat sa iau lapte. Eu nici nu beau lapte.", Rating = 4 }
                }
            }
        };


        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roleNames = { "Admin", "Colaborator", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // admini
            await CreateUser(userManager, "Admin", "Principal", "admin@test.com", "Admin1!", "Admin");
            await CreateUser(userManager, "Elena", "Popescu", "elena.popescu@admin.com", "Admin1!", "Admin");
            await CreateUser(userManager, "Mihai", "Ionescu", "mihai.ionescu@admin.com", "Admin1!", "Admin");

            // colaboratori
            await CreateUser(userManager, "Colaborator", "Activ", "colab@test.com", "Colab1!", "Colaborator");
            await CreateUser(userManager, "Andrei", "Radu", "andrei.tech@store.com", "Colab1!", "Colaborator");
            await CreateUser(userManager, "Ioana", "Dumitrescu", "ioana.fashion@boutique.com", "Colab1!", "Colaborator");
            await CreateUser(userManager, "Robert", "Negoiță", "robert.sport@fit.com", "Colab1!", "Colaborator");
            await CreateUser(userManager, "Maria", "Stancu", "maria.deco@home.com", "Colab1!", "Colaborator");

            // clienti
            await CreateUser(userManager, "User", "Client", "user@test.com", "User1!", "User");
            await CreateUser(userManager, "Cristian", "Vasile", "cristi.v@gmail.com", "User1!", "User");
            await CreateUser(userManager, "Ana", "Marin", "ana.marin@yahoo.com", "User1!", "User");
            await CreateUser(userManager, "George", "Popa", "george.popa@outlook.com", "User1!", "User");
            await CreateUser(userManager, "Diana", "Constantin", "diana.c@gmail.com", "User1!", "User");
            await CreateUser(userManager, "Vlad", "Munteanu", "vlad.munteanu@test.com", "User1!", "User");
            await CreateUser(userManager, "Simona", "Dobre", "simona.dobre@test.com", "User1!", "User");

            await SeedCategories(context);
            await SeedProposalsAndProducts(context, userManager);
            await SeedReviews(context);
        }

        private static async Task CreateUser(UserManager<ApplicationUser> userManager, string firstName, string lastName, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };
                var createUser = await userManager.CreateAsync(newUser, password);
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role);
                }
            }
        }

        private static async Task SeedCategories(ApplicationDbContext context)
        {
            string[] categories = {
                "Electronice",
                "Electrocasnice",
                "Imbracaminte & Incaltaminte",
                "Decoratiuni",
                "Jucarii, Copii & Bebe",
                "Petshop",
                "Carti & Birotica"
            };

            foreach (var categoryName in categories)
            {
                var category = context.Categories.FirstOrDefault(c => c.Name == categoryName);
                if (category == null)
                {
                    context.Categories.Add(new Category { Name = categoryName });
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedProposalsAndProducts(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (await context.ProductProposals.AnyAsync())
            {
                return;
            }

            var collaborator = await userManager.FindByEmailAsync("colab@test.com");
            if (collaborator == null) return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "proposals.json");
            if (!File.Exists(filePath)) return;

            var jsonData = await File.ReadAllTextAsync(filePath);
            var proposalsDto = JsonSerializer.Deserialize<List<ProposalSeedDto>>(jsonData);

            if (proposalsDto == null) return;

            foreach (var dto in proposalsDto)
            {
                var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == dto.CategoryName);

                if (category != null)
                {
                    var proposal = new ProductProposal
                    {
                        Name = dto.Title,
                        Description = dto.Description,
                        Price = dto.Price,
                        CategoryId = category.Id,
                        UserId = collaborator.Id,
                        ImagePath = "/images/products/" + dto.ImageFile,
                        Status = dto.Status ?? "Pending",
                        Stock = 100
                    };

                    context.ProductProposals.Add(proposal);

                    if (dto.Status == "Approved")
                    {
                        var product = new Product
                        {
                            Name = proposal.Name,
                            Description = proposal.Description,
                            Price = proposal.Price,
                            CategoryId = category.Id,
                            ImagePath = proposal.ImagePath,
                            Stock = 100,
                            Proposal = proposal,
                            Rating = null
                        };
                        context.Products.Add(product);
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedReviews(ApplicationDbContext context)
        {
            if (await context.Reviews.AnyAsync()) return;

            var products = await context.Products.ToListAsync();
            if (!products.Any()) return;

            // luam un user client pentru a pune review-urile pe numele lui
            var clientUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "cristi.v@gmail.com")
                             ?? await context.Users.FirstOrDefaultAsync();

            if (clientUser == null) return;

            var reviewsToAdd = new List<Review>();
            var random = new Random();

            foreach (var product in products)
            {
                // funny products
                if (FunnyReviews.ContainsKey(product.Name))
                {
                    foreach (var rev in FunnyReviews[product.Name])
                    {
                        reviewsToAdd.Add(new Review
                        {
                            Title = rev.Title,
                            Description = rev.Description,
                            Rating = rev.Rating,
                            Date = DateTime.Now.AddDays(-random.Next(1, 30)),
                            ProductId = product.Id,
                            UserId = clientUser.Id
                        });
                    }
                }
                // produse serioase (generate automat)
                else
                {
                    int numberOfReviews = random.Next(1, 5);
                    for (int i = 0; i < numberOfReviews; i++)
                    {
                        bool isPositive = random.Next(0, 10) > 2; // 80% pozitive
                        var autoReview = GenerateGenericReview(isPositive);

                        autoReview.ProductId = product.Id;
                        autoReview.UserId = clientUser.Id;
                        reviewsToAdd.Add(autoReview);
                    }
                }
            }

            if (reviewsToAdd.Any())
            {
                context.Reviews.AddRange(reviewsToAdd);
                await context.SaveChangesAsync();

                await UpdateProductRatings(context);
            }
        }

        // generator text generic
        private static Review GenerateGenericReview(bool isPositive)
        {
            var rnd = new Random();

            string[] positiveTitles = {
                "Excelent", "Mulțumit", "Recomand", "Super produs", "Merită banii", "Calitate top"
            };

            string[] negativeTitles = {
                "Dezamăgitor", "Se putea mai bine", "Nu recomand", "Scump", "Probleme", "Nesatisfăcător"
            };

            string[] positiveDesc = {
                "Bateria ține mult și se încarcă repede.",
                "Ecran superb, culori vii.",
                "Se mișcă foarte rapid, nu are lag.",
                "Livrare rapidă, a ajuns a doua zi.",
                "Materiale de calitate, se simte premium.",
                "Raport calitate-preț excelent."
            };

            string[] negativeDesc = {
                "Se încălzește cam tare în utilizare intensă.",
                "Cutia a ajuns puțin îndoită.",
                "Prețul este cam mare pentru ce oferă.",
                "Meniul este puțin complicat.",
                "Mă așteptam la mai mult de la acest brand.",
                "Produsul nu corespunde cu descrierea complet."
            };

            string title;
            string desc;
            int rating;

            // logica de selectie in functie de pozitiv/negativ
            if (isPositive)
            {
                title = positiveTitles[rnd.Next(positiveTitles.Length)];
                desc = positiveDesc[rnd.Next(positiveDesc.Length)];
                rating = rnd.Next(4, 6); // 4 sau 5
            }
            else
            {
                title = negativeTitles[rnd.Next(negativeTitles.Length)];
                desc = negativeDesc[rnd.Next(negativeDesc.Length)];
                rating = rnd.Next(1, 4);
            }

            return new Review
            {
                Title = title,
                Description = desc,
                Rating = rating,
                Date = DateTime.Now.AddDays(-rnd.Next(1, 100))
            };
        }

        private static async Task UpdateProductRatings(ApplicationDbContext context)
        {
            var products = await context.Products.Include(p => p.Reviews).ToListAsync();
            foreach (var p in products)
            {
                if (p.Reviews.Any())
                {
                    p.Rating = p.Reviews.Average(r => (double)r.Rating);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}