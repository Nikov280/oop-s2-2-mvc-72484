using FoodSafety.Domain.Entities;
using static FoodSafety.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FoodSafety.MVC.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(ApplicationDbContext context)
        {
            // Here we check if there is data. If there is, don't seed again.
            if (context.Premises.Any()) return;

            // 1. Create 12 Premises 
            var premises = new List<Premises>
            {
                new Premises { Name = "The Golden Grill", Address = "10 Main St", Town = "Bristol", RiskRating = RiskRating.High },
                new Premises { Name = "Pizza Express", Address = "22 High St", Town = "Bristol", RiskRating = RiskRating.Medium },
                new Premises { Name = "Sushi World", Address = "55 Sea View", Town = "Bath", RiskRating = RiskRating.High },
                new Premises { Name = "Taco Bell", Address = "99 West Rd", Town = "Bath", RiskRating = RiskRating.Low },
                new Premises { Name = "Burger King", Address = "1 Central Sq", Town = "London", RiskRating = RiskRating.Medium },
                new Premises { Name = "The Green Salad", Address = "12 Leafy Ln", Town = "London", RiskRating = RiskRating.Low },
                new Premises { Name = "Indian Spice", Address = "88 Curry Ln", Town = "Bristol", RiskRating = RiskRating.High },
                new Premises { Name = "Pasta Perfect", Address = "34 Italian Way", Town = "Bath", RiskRating = RiskRating.Medium },
                new Premises { Name = "Steak House", Address = "7 Meat Blvd", Town = "London", RiskRating = RiskRating.High },
                new Premises { Name = "Vegan Corner", Address = "2 Veggie St", Town = "Bristol", RiskRating = RiskRating.Low },
                new Premises { Name = "Coffee Hub", Address = "33 Bean Rd", Town = "Bath", RiskRating = RiskRating.Low },
                new Premises { Name = "Kebab Stop", Address = "44 Night St", Town = "London", RiskRating = RiskRating.Medium }
            };

            context.Premises.AddRange(premises);
            await context.SaveChangesAsync();

            // 2. Create 25 Inspections 
            var random = new Random();
            var inspections = new List<Inspection>();

            foreach (var p in premises)
            {
                for (int i = 0; i < 2; i++) // 2 inspections per premises
                {
                    int score = random.Next(30, 100);
                    inspections.Add(new Inspection
                    {
                        PremisesId = p.Id,
                        InspectionDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                        Score = score,
                        Outcome = score < 60 ? Outcome.Fail : Outcome.Pass,
                        Notes = "Standard food hygiene inspection."
                    });
                }
            }
            var extraInspection = new Inspection
            {
                PremisesId = premises.First().Id, // Assign to the first premises
                InspectionDate = DateTime.Now.AddDays(-5),
                Score = 45,
                Outcome = Outcome.Fail, 
                Notes = "Additional random inspection to meet the seed quota of 25."
            };
            inspections.Add(extraInspection);

            context.Inspections.AddRange(inspections);
            await context.SaveChangesAsync();

            // 3. Create 10 Follow-ups (Some overdue, some closed)
            if (!context.FollowUps.Any())
            {
                // Take the first 10 inspections to link them to a follow-up
                var testInspections = context.Inspections.Take(10).ToList();
                var followUps = new List<FollowUp>();

                for (int i = 0; i < 10; i++)
                {
                    var inspection = testInspections[i];

                    if (i < 5)
                    {
                        // First 5: OVERDUE (Status Open + DueDate in the past)
                        followUps.Add(new FollowUp
                        {
                            InspectionId = inspection.Id,
                            DueDate = DateTime.Now.AddDays(-10), // Date in the past
                            Status = "Open",
                            ClosedDate = null
                        });
                    }
                    else
                    {
                        // Last 5: CLOSED (Status Closed + ClosedDate provided)
                        followUps.Add(new FollowUp
                        {
                            InspectionId = inspection.Id,
                            DueDate = DateTime.Now.AddDays(5),
                            Status = "Closed",
                            ClosedDate = DateTime.Now.AddDays(-1)
                        });
                    }
                }

                context.FollowUps.AddRange(followUps);
                await context.SaveChangesAsync();
            }


            // Selecting failed inspections for follow-up
            var failures = inspections.Where(i => i.Outcome == Outcome.Fail).Take(10).ToList();
            foreach (var fail in failures)
            {
                context.FollowUps.Add(new FollowUp
                {
                    InspectionId = fail.Id,
                    DueDate = DateTime.Now.AddDays(random.Next(-5, 10)),
                    Status = "Open"
                });
            }
            await context.SaveChangesAsync();
        }

        


public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
    {
        // Getting the identity managers from the service provider
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // 1. Create Roles if they do not exist in the AspNetRoles table
        string[] roleNames = { "Admin", "Inspector", "Viewer" };
        foreach (var roleName in roleNames)
        {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Define our Test Users
            var users = new[]
            {
                new { Email = "admin@food.com", Role = "Admin" },
                new { Email = "inspector@food.com", Role = "Inspector" },
                new { Email = "manager@food.com", Role = "Viewer" }
            };

            foreach (var userData in users)
            {
                var user = await userManager.FindByEmailAsync(userData.Email);
                if (user == null)
                {
                    var newUser = new IdentityUser
                    {
                        UserName = userData.Email,
                        Email = userData.Email,
                        EmailConfirmed = true
                    };

                    // Using a standard strong password for all test accounts
                    // Requirements: Uppercase, Lowercase, Number, and Special Character
                    string password = "Password123!";

                    var createResult = await userManager.CreateAsync(newUser, password);

                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, userData.Role);
                    }
                }
            }
        }
}
}