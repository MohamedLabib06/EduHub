using EduHub.Data.Context;
using EduHub.Data.Entities;
using EduHub.Data.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduHub.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EduHubDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // 1. Seed Roles
        string[] roles = { "Admin", "Instructor", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Default Instructor
        const string instructorEmail = "instructor@eduhub.com";
        var instructorUser = await userManager.FindByEmailAsync(instructorEmail);
        if (instructorUser == null)
        {
            instructorUser = new ApplicationUser
            {
                UserName = instructorEmail,
                Email = instructorEmail,
                FirstName = "Mohamed",
                LastName = "Mahmoud",
                UserType = UserType.Instructor,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(instructorUser, "EduHub@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(instructorUser, "Instructor");
                context.Instructors.Add(new Instructor
                {
                    Id = instructorUser.Id,
                    Headline = "Senior Full-Stack .NET & Web Architect",
                    Bio = "Building scalable enterprise web apps with ASP.NET Core, React, and SQL Server.",
                    TotalEarnings = 0
                });
                await context.SaveChangesAsync();
            }
        }

        // 3. Seed Default Student
        const string studentEmail = "student@eduhub.com";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);
        if (studentUser == null)
        {
            studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FirstName = "Sara",
                LastName = "Ahmed",
                UserType = UserType.Student,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(studentUser, "EduHub@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
                context.Students.Add(new Student
                {
                    Id = studentUser.Id,
                    Interests = "Web Development,Backend,UI/UX"
                });
                await context.SaveChangesAsync();
            }
        }

        // 4. Seed Categories & Courses
        if (!await context.Categories.AnyAsync())
        {
            var webCat = new Category { Name = "Web Design", Description = "Modern front-end engineering and UI architecture." };
            var backendCat = new Category { Name = "Backend & API", Description = "Scalable server-side systems with .NET and SQL." };
            var marketingCat = new Category { Name = "Marketing", Description = "Digital growth, SEO, and content strategy." };
            var financeCat = new Category { Name = "Finance", Description = "Financial modeling, analytics, and investment." };

            context.Categories.AddRange(webCat, backendCat, marketingCat, financeCat);
            await context.SaveChangesAsync();

            var courses = new List<Course>
            {
                new Course
                {
                    Title = "Full-Stack ASP.NET Core MVC Masterclass",
                    Price = 49.99m,
                    Level = CourseLevel.Intermediate,
                    Status = CourseStatus.Published,
                    AverageRating = 4.9,
                    InstructorId = instructorUser.Id,
                    CategoryId = backendCat.Id,
                    Sections = new List<Section>
                    {
                        new Section
                        {
                            Title = "N-Tier Architecture & EF Core",
                            OrderIndex = 1,
                            Lessons = new List<Lesson>
                            {
                                new Lesson { Title = "Setting up Solution Layers", VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", Duration = TimeSpan.FromMinutes(15), OrderIndex = 1 },
                                new Lesson { Title = "Fluent API & Migrations", VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", Duration = TimeSpan.FromMinutes(22), OrderIndex = 2 }
                            }
                        }
                    }
                },
                new Course
                {
                    Title = "Modern UI/UX & Responsive Web Design",
                    Price = 29.99m,
                    Level = CourseLevel.Beginner,
                    Status = CourseStatus.Published,
                    AverageRating = 4.8,
                    InstructorId = instructorUser.Id,
                    CategoryId = webCat.Id,
                    Sections = new List<Section>
                    {
                        new Section
                        {
                            Title = "Bootstrap 5 & Design Systems",
                            OrderIndex = 1,
                            Lessons = new List<Lesson>
                            {
                                new Lesson { Title = "Grid System & Typography", VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", Duration = TimeSpan.FromMinutes(18), OrderIndex = 1 }
                            }
                        }
                    }
                },
                new Course
                {
                    Title = "Digital Marketing & Growth Analytics",
                    Price = 19.99m,
                    Level = CourseLevel.Beginner,
                    Status = CourseStatus.Published,
                    AverageRating = 4.7,
                    InstructorId = instructorUser.Id,
                    CategoryId = marketingCat.Id
                },
                new Course
                {
                    Title = "Corporate Finance & Investment Analysis",
                    Price = 39.99m,
                    Level = CourseLevel.Advanced,
                    Status = CourseStatus.Published,
                    AverageRating = 4.9,
                    InstructorId = instructorUser.Id,
                    CategoryId = financeCat.Id
                }
            };

            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();
        }
    }
}