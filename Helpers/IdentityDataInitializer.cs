using Microsoft.AspNetCore.Identity;
using FoodDelivery.Models;
using MongoDB.Bson;

namespace FoodDelivery.Helpers
{
    public static class IdentityDataInitializer
    {
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var adminPassword = "Admin@123";

            // Check if the roles exist
            foreach (var roleName in Enum.GetValues(typeof(RoleEnum)).Cast<RoleEnum>())
            {
                if (!await roleManager.RoleExistsAsync(roleName.ToString()))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName.ToString(),
                        Id = ObjectId.GenerateNewId()
                    };
                    var roleResult = await roleManager.CreateAsync(role);
                    if (!roleResult.Succeeded)
                    {
                        Console.WriteLine($"Failed to create role '{roleName}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
            var adminEmail = "admin@fooddelivery.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    UserRole = RoleEnum.Admin,
                    Id = ObjectId.GenerateNewId() // Присваиваем новый ObjectId
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    Console.WriteLine($"Admin user '{adminEmail}' created successfully with role '{RoleEnum.Admin}'.");
                }
                else
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                if (adminUser.UserRole != RoleEnum.Admin)
                {
                    adminUser.UserRole = RoleEnum.Admin;
                    await userManager.UpdateAsync(adminUser);
                    Console.WriteLine($"Role '{RoleEnum.Admin}' added to existing user '{adminEmail}'.");
                }
                else
                {
                    Console.WriteLine($"User '{adminEmail}' already has role '{RoleEnum.Admin}'.");
                }
            }
        }
    }
}
