using System.ComponentModel.DataAnnotations;
using EduHub.Data.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduHub.Data.Entities;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    public UserType UserType { get; set; }

    // Navigation Properties (1:1 Shared Primary Key)
    public Instructor? InstructorProfile { get; set; }
    public Student? StudentProfile { get; set; }
}