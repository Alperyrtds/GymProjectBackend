using Microsoft.EntityFrameworkCore;

namespace GymProject.Models;

public partial class AlperyurtdasGymProjectContext : DbContext
{
    public AlperyurtdasGymProjectContext()
    {
    }

    public AlperyurtdasGymProjectContext(DbContextOptions<AlperyurtdasGymProjectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaseUser> Users { get; set; }

    public virtual DbSet<Administrator> Administrators { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomersProgram> CustomersPrograms { get; set; }

    public virtual DbSet<ProgramMovement> ProgramMovements { get; set; }

    public virtual DbSet<CustomersRegistration> CustomersRegistrations { get; set; }

    public virtual DbSet<Trainer> Trainers { get; set; }

    public virtual DbSet<Movement> Movements { get; set; }

    public virtual DbSet<WorkoutLog> WorkoutLogs { get; set; }

    public virtual DbSet<WorkoutSession> WorkoutSessions { get; set; }

    public virtual DbSet<Goal> Goals { get; set; }

    // Connection string is configured in Program.cs via dependency injection

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base User configuration
        modelBuilder.Entity<BaseUser>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(30);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.UserPassword).HasMaxLength(200);
        });

        // Table Per Type (TPT) Inheritance Configuration
        // Key sadece BaseUser'da tanımlı, derived class'larda sadece ToTable ve property'ler
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.ToTable("Administrators");
            entity.Property(e => e.AdministratorName).HasMaxLength(50);
            entity.Property(e => e.AdministratorSurname).HasMaxLength(50);
            entity.Property(e => e.IsPasswordChanged).HasDefaultValue(false);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.Property(e => e.CustomerEmail).HasMaxLength(60);
            entity.Property(e => e.CustomerIdentityNumber).HasMaxLength(50);
            entity.Property(e => e.CustomerName).HasMaxLength(50);
            entity.Property(e => e.CustomerPhoneNumber).HasMaxLength(50);
            entity.Property(e => e.CustomerSurname).HasMaxLength(50);
            entity.Property(e => e.IsPasswordChanged).HasDefaultValue(false);
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.ToTable("Trainers");
            entity.Property(e => e.TrainerName).HasMaxLength(50);
            entity.Property(e => e.TrainerSurname).HasMaxLength(50);
            entity.Property(e => e.TrainerPhoneNumber).HasMaxLength(50);
            entity.Property(e => e.TrainerEmail).HasMaxLength(60);
            entity.Property(e => e.IsPasswordChanged).HasDefaultValue(false);
        });

        modelBuilder.Entity<Movement>(entity =>
        {
            entity.HasKey(e => e.MovementId).HasName("PK_Movement");

            entity.ToTable("Movements");

            entity.Property(e => e.MovementId).HasMaxLength(30);
            entity.Property(e => e.MovementName).HasMaxLength(100);
            entity.Property(e => e.MovementDescription).HasMaxLength(500);
            entity.Property(e => e.MovementVideoUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.HasKey(e => e.WorkoutSessionId).HasName("PK_WorkoutSession");

            entity.ToTable("WorkoutSessions");

            entity.Property(e => e.WorkoutSessionId).HasMaxLength(30);
            entity.Property(e => e.CustomerId).HasMaxLength(30);
            entity.Property(e => e.TrainerId).HasMaxLength(30);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.WorkoutDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TotalDuration); // Dakika cinsinden
            
            // Foreign key to Customer
            entity.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .HasPrincipalKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Trainer (optional)
            entity.HasOne<Trainer>()
                .WithMany()
                .HasForeignKey(e => e.TrainerId)
                .HasPrincipalKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkoutLog>(entity =>
        {
            entity.HasKey(e => e.WorkoutLogId).HasName("PK_WorkoutLog");

            entity.ToTable("WorkoutLogs");

            entity.Property(e => e.WorkoutLogId).HasMaxLength(30);
            entity.Property(e => e.WorkoutSessionId).HasMaxLength(30);
            entity.Property(e => e.CustomerId).HasMaxLength(30); // Backward compatibility
            entity.Property(e => e.MovementId).HasMaxLength(30);
            entity.Property(e => e.MovementName).HasMaxLength(100);
            entity.Property(e => e.TrainerId).HasMaxLength(30); // Backward compatibility
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Weight).HasColumnType("numeric(10,2)");
            entity.Property(e => e.WorkoutDate).HasColumnType("timestamp without time zone"); // Backward compatibility
            entity.Property(e => e.WorkoutDuration); // Backward compatibility
            
            // Foreign key to WorkoutSession
            entity.HasOne<WorkoutSession>()
                .WithMany()
                .HasForeignKey(e => e.WorkoutSessionId)
                .HasPrincipalKey(s => s.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Customer (backward compatibility)
            entity.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .HasPrincipalKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            
            // Foreign key to Movement (optional)
            entity.HasOne<Movement>()
                .WithMany()
                .HasForeignKey(e => e.MovementId)
                .HasPrincipalKey(m => m.MovementId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Foreign key to Trainer (backward compatibility)
            entity.HasOne<Trainer>()
                .WithMany()
                .HasForeignKey(e => e.TrainerId)
                .HasPrincipalKey(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CustomersProgram>(entity =>
        {
            entity.HasKey(e => e.CustomerProgramId);

            entity.ToTable("Customers_Program");

            entity.Property(e => e.CustomerProgramId).HasMaxLength(30);
            entity.Property(e => e.CustomerId).HasMaxLength(30);
            entity.Property(e => e.MovementId).HasMaxLength(30); // Backward compatibility
            entity.Property(e => e.MovementName).HasMaxLength(100); // Backward compatibility
            entity.Property(e => e.ProgramName).HasMaxLength(200);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(30);
            entity.Property(e => e.CreatedByName).HasMaxLength(100);
            entity.Property(e => e.ProgramEndDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ProgramStartDate).HasColumnType("timestamp without time zone");
            
            // Foreign key to Customer (which now uses UserId)
            entity.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .HasPrincipalKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Movement (optional - backward compatibility)
            entity.HasOne<Movement>()
                .WithMany()
                .HasForeignKey(e => e.MovementId)
                .HasPrincipalKey(m => m.MovementId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProgramMovement>(entity =>
        {
            entity.HasKey(e => e.ProgramMovementId).HasName("PK_ProgramMovement");

            entity.ToTable("ProgramMovements");

            entity.Property(e => e.ProgramMovementId).HasMaxLength(30);
            entity.Property(e => e.CustomerProgramId).HasMaxLength(30);
            entity.Property(e => e.MovementId).HasMaxLength(30);
            entity.Property(e => e.MovementName).HasMaxLength(100);
            
            // Foreign key to CustomersProgram
            entity.HasOne<CustomersProgram>()
                .WithMany()
                .HasForeignKey(e => e.CustomerProgramId)
                .HasPrincipalKey(p => p.CustomerProgramId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Movement (optional)
            entity.HasOne<Movement>()
                .WithMany()
                .HasForeignKey(e => e.MovementId)
                .HasPrincipalKey(m => m.MovementId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CustomersRegistration>(entity =>
        {
            entity.HasKey(e => e.CustomerRegistrationId).HasName("PK_CustomerRegistratiom");

            entity.ToTable("Customers_Registration");

            entity.Property(e => e.CustomerRegistrationId).HasMaxLength(30);
            entity.Property(e => e.CustomerId).HasMaxLength(30);
            entity.Property(e => e.CustomerRegistrationFinishDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CustomerRegistrationStartDate).HasColumnType("timestamp without time zone");
            
            // Foreign key to Customer (which now uses UserId)
            entity.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .HasPrincipalKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId).HasName("PK_Goal");

            entity.ToTable("Goals");

            entity.Property(e => e.GoalId).HasMaxLength(30);
            entity.Property(e => e.CustomerId).HasMaxLength(30);
            entity.Property(e => e.GoalType).HasMaxLength(50);
            entity.Property(e => e.GoalName).HasMaxLength(200);
            entity.Property(e => e.TrainerId).HasMaxLength(30);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.TargetValue).HasColumnType("numeric(10,2)");
            entity.Property(e => e.CurrentValue).HasColumnType("numeric(10,2)");
            entity.Property(e => e.TargetDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CompletedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            
            // Foreign key to Customer
            entity.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .HasPrincipalKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Trainer (optional)
            entity.HasOne<Trainer>()
                .WithMany()
                .HasForeignKey(e => e.TrainerId)
                .HasPrincipalKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
