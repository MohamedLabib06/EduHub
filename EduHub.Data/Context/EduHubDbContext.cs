using EduHub.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Data.Context;

public class EduHubDbContext : IdentityDbContext<ApplicationUser>
{
    public EduHubDbContext(DbContextOptions<EduHubDbContext> options) : base(options)
    {
    }

    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<LessonProgress> LessonProgresses { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. 1:1 Shared Primary Key Relationships
        builder.Entity<Instructor>()
            .HasOne(i => i.User)
            .WithOne(u => u.InstructorProfile)
            .HasForeignKey<Instructor>(i => i.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.StudentProfile)
            .HasForeignKey<Student>(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // 2. Decimal Precision Configurations
        builder.Entity<Instructor>()
            .Property(i => i.TotalEarnings)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Course>()
            .Property(c => c.Price)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Enrollment>()
            .Property(e => e.AmountPaid)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Quiz>()
            .Property(q => q.PassingScore)
            .HasColumnType("decimal(5,2)");

        builder.Entity<QuizAttempt>()
            .Property(qa => qa.Score)
            .HasColumnType("decimal(5,2)");

        // 3. Unique Constraints & Preventing Multiple Cascade Paths in SQL Server
        
        // Enrollment: Student cannot enroll in the same course twice
        builder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        builder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Review: Student cannot review the same course twice
        builder.Entity<Review>()
            .HasIndex(r => new { r.StudentId, r.CourseId })
            .IsUnique();

        builder.Entity<Review>()
            .HasOne(r => r.Student)
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Review>()
            .HasOne(r => r.Course)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // LessonProgress: Unique per Student & Lesson
        builder.Entity<LessonProgress>()
            .HasIndex(lp => new { lp.StudentId, lp.LessonId })
            .IsUnique();

        builder.Entity<LessonProgress>()
            .HasOne(lp => lp.Student)
            .WithMany(s => s.LessonProgresses)
            .HasForeignKey(lp => lp.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<LessonProgress>()
            .HasOne(lp => lp.Lesson)
            .WithMany(l => l.LessonProgresses)
            .HasForeignKey(lp => lp.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // QuizAttempt: Prevent Multiple Cascade Paths
        builder.Entity<QuizAttempt>()
            .HasOne(qa => qa.Student)
            .WithMany(s => s.QuizAttempts)
            .HasForeignKey(qa => qa.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<QuizAttempt>()
            .HasOne(qa => qa.Quiz)
            .WithMany(q => q.QuizAttempts)
            .HasForeignKey(qa => qa.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course -> Instructor (NoAction to avoid cycle with ApplicationUser)
        builder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany(i => i.Courses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}