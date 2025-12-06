using Microsoft.AspNetCore.Identity;


namespace Proiect.Models
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Colaborator", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUser = new ApplicationUser
            {
                UserName = "admin@test.com",
                Email = "admin@test.com",
                FirstName = "Admin",
                LastName = "Principal",
                EmailConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(adminUser.Email);
            if (user == null)
            {
                var createAdmin = await userManager.CreateAsync(adminUser, "Admin1!");
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var collabUser = new ApplicationUser
            {
                UserName = "colab@test.com",
                Email = "colab@test.com",
                FirstName = "Colaborator",
                LastName = "Activ",
                EmailConfirmed = true
            };

            user = await userManager.FindByEmailAsync(collabUser.Email);
            if (user == null)
            {
                var createCollab = await userManager.CreateAsync(collabUser, "Colab1!");
                if (createCollab.Succeeded)
                {
                    await userManager.AddToRoleAsync(collabUser, "Colaborator");
                }
            }

            var standardUser = new ApplicationUser
            {
                UserName = "user@test.com",
                Email = "user@test.com",
                FirstName = "User",
                LastName = "Client",
                EmailConfirmed = true
            };

            user = await userManager.FindByEmailAsync(standardUser.Email);
            if (user == null)
            {
                var createUser = await userManager.CreateAsync(standardUser, "User1!");
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(standardUser, "User");
                }
            }
        }
    }
}