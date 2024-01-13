using System;
using System.Collections.Generic;
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

    public virtual DbSet<Administrator> Administrators { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomersProgram> CustomersPrograms { get; set; }

    public virtual DbSet<CustomersRegistration> CustomersRegistrations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=sql.bsite.net\\MSSQL2016;Initial Catalog=alperyurtdas_GymProject;Persist Security Info=False;User ID=alperyurtdas_GymProject;Password=8520Alper;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.AdministratorId).HasName("PK_Administrator");

            entity.Property(e => e.AdministratorId).HasMaxLength(30);
            entity.Property(e => e.AdministratorName).HasMaxLength(50);
            entity.Property(e => e.AdministratorSurname).HasMaxLength(50);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CustomerId).HasMaxLength(30);
            entity.Property(e => e.CustomerEmail).HasMaxLength(60);
            entity.Property(e => e.CustomerIdentityNumber).HasMaxLength(50);
            entity.Property(e => e.CustomerName).HasMaxLength(50);
            entity.Property(e => e.CustomerPhoneNumber).HasMaxLength(50);
            entity.Property(e => e.CustomerSurname).HasMaxLength(50);
        });

        modelBuilder.Entity<CustomersProgram>(entity =>
        {
            entity.HasKey(e => e.CustomerProgramId);

            entity.ToTable("Customers_Program");

            entity.Property(e => e.CustomerProgramId).HasMaxLength(30);
            entity.Property(e => e.CustomerId).HasMaxLength(30);
            entity.Property(e => e.ProgramEndDate).HasColumnType("datetime");
            entity.Property(e => e.ProgramStartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<CustomersRegistration>(entity =>
        {
            entity.HasKey(e => e.CustomerRegistrationId).HasName("PK_CustomerRegistratiom");

            entity.ToTable("Customers_Registration");

            entity.Property(e => e.CustomerRegistrationId).HasMaxLength(30);
            entity.Property(e => e.CustomerId).HasMaxLength(30);
            entity.Property(e => e.CustomerRegistrationFinishDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerRegistrationStartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(30);
            entity.Property(e => e.AdminastorId).HasMaxLength(30);
            entity.Property(e => e.CustomerId).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
