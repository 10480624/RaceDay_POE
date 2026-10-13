
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USERS
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users", t =>
                t.HasCheckConstraint(
                    "CHK_User_Role",
                    "[role] IN ('Organiser', 'Participant')"));

            e.HasKey(x => x.UserId);

            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Email)
                .HasColumnName("email").HasColumnType("varchar(255)")
                .IsRequired();

            e.HasIndex(x => x.Email).IsUnique();

            e.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .HasColumnType("varchar(255)").IsRequired();

            e.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasColumnType("varchar(100)").IsRequired();

            e.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasColumnType("varchar(100)").IsRequired();

            e.Property(x => x.Role)
                .HasColumnName("role")
                .HasColumnType("varchar(20)")
                .HasDefaultValue("Participant").IsRequired();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");
        });

        // PROFILES
        modelBuilder.Entity<Profile>(e =>
        {
            e.ToTable("Profiles");
            e.HasKey(x => x.ProfileId);

            e.Property(x => x.ProfileId).HasColumnName("profile_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.DateOfBirth)
                .HasColumnName("date_of_birth").HasColumnType("date");

            e.Property(x => x.EmergencyContactName)
                .HasColumnName("emergency_contact_name")
                .HasColumnType("varchar(100)");

            e.Property(x => x.EmergencyContactPhone)
                .HasColumnName("emergency_contact_phone")
                .HasColumnType("varchar(20)");

            e.Property(x => x.MedicalConditions)
                .HasColumnName("medical_conditions")
                .HasColumnType("varchar(500)");

            e.Property(x => x.ProfilePictureUrl)
                .HasColumnName("profile_picture_url")
                .HasColumnType("varchar(500)");

            e.HasIndex(x => x.UserId).IsUnique();

            e.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Profile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EVENTS
        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("Events", t =>
                t.HasCheckConstraint(
                    "CHK_Event_Type",
                    "[event_type] IN ('run', 'walk', 'cycle')"));

            e.HasKey(x => x.EventId);

            e.Property(x => x.EventId).HasColumnName("event_id");
            e.Property(x => x.OrganiserId).HasColumnName("organiser_id");
            e.Property(x => x.Name)
                .HasColumnName("name").HasColumnType("varchar(200)")
                .IsRequired();

            e.Property(x => x.Description)
                .HasColumnName("description")
                .HasColumnType("varchar(1000)");

            e.Property(x => x.EventDate)
                .HasColumnName("event_date").HasColumnType("date");

            e.Property(x => x.Location)
                .HasColumnName("location").HasColumnType("varchar(200)")
                .IsRequired();

            e.Property(x => x.Distance)
                .HasColumnName("distance").HasPrecision(6, 2);

            e.Property(x => x.EventType)
                .HasColumnName("event_type")
                .HasColumnType("varchar(20)").IsRequired();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Organiser)
                .WithMany()
                .HasForeignKey(x => x.OrganiserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // CATEGORIES
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.CategoryId);

            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.EventId).HasColumnName("event_id");

            e.Property(x => x.CategoryName)
                .HasColumnName("category_name")
                .HasColumnType("varchar(100)").IsRequired();

            e.Property(x => x.AgeRange)
                .HasColumnName("age_range").HasColumnType("varchar(50)");

            e.Property(x => x.Distance)
                .HasColumnName("distance").HasPrecision(6, 2);

            e.HasOne(x => x.Event)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ENROLMENTS
        modelBuilder.Entity<Enrolment>(e =>
        {
            e.ToTable("Enrolments", t =>
                t.HasCheckConstraint(
                    "CHK_Enrolment_Status",
                    "[status] IN ('Pending', 'Confirmed', 'Cancelled', 'Completed', 'Archived')"));

            e.HasKey(x => x.EnrolmentId);

            e.Property(x => x.EnrolmentId).HasColumnName("enrolment_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.EventId).HasColumnName("event_id");
            e.Property(x => x.CategoryId).HasColumnName("category_id");

            e.Property(x => x.EnrolmentDate)
                .HasColumnName("enrolment_date")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            e.Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType("varchar(20)")
                .HasDefaultValue("Pending").IsRequired();

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Event)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Category)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // RESULTS
        modelBuilder.Entity<Result>(e =>
        {
            e.ToTable("Results");
            e.HasKey(x => x.ResultId);

            e.Property(x => x.ResultId).HasColumnName("result_id");
            e.Property(x => x.EnrolmentId).HasColumnName("enrolment_id");

            e.Property(x => x.FinishTime)
                .HasColumnName("finish_time").HasColumnType("time");

            e.Property(x => x.FinishingPosition)
                .HasColumnName("finishing_position");

            e.HasIndex(x => x.EnrolmentId).IsUnique();

            e.HasOne(x => x.Enrolment)
                .WithOne(x => x.Result)
                .HasForeignKey<Result>(x => x.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // NOTIFICATIONS
        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.HasKey(x => x.NotificationId);

            e.Property(x => x.NotificationId).HasColumnName("notification_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.EventId).HasColumnName("event_id");

            e.Property(x => x.Message)
                .HasColumnName("message")
                .HasColumnType("varchar(500)").IsRequired();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            e.Property(x => x.IsRead)
                .HasColumnName("is_read").HasDefaultValue(false);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Event)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
