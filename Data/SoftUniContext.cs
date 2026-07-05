using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using soft_uni_mvc.Models;

namespace soft_uni_mvc.Data;

public partial class SoftUniContext : DbContext
{
    public SoftUniContext()
    {
    }

    public SoftUniContext(DbContextOptions<SoftUniContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeProject> EmployeeProjects { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Town> Towns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PRIMARY");

            entity.ToTable("addresses");

            entity.HasIndex(e => e.AddressId, "PK_Addresses").IsUnique();

            entity.HasIndex(e => e.TownId, "fk_addresses_towns");

            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.AddressText)
                .HasMaxLength(100)
                .HasColumnName("address_text");
            entity.Property(e => e.TownId).HasColumnName("town_id");

            entity.HasOne(d => d.Town).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.TownId)
                .HasConstraintName("fk_addresses_towns");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PRIMARY");

            entity.ToTable("departments");

            entity.HasIndex(e => e.DepartmentId, "PK_Departments").IsUnique();

            entity.HasIndex(e => e.ManagerId, "fk_departments_employees");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            entity.HasOne(d => d.Manager).WithMany(p => p.ManagedDepartments)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_departments_employees");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PRIMARY");

            entity.ToTable("employees");

            entity.HasIndex(e => e.FirstName, "CL_FirstName");

            entity.HasIndex(e => e.EmployeeId, "PK_Employees").IsUnique();

            entity.HasIndex(e => e.AddressId, "fk_employees_addresses");

            entity.HasIndex(e => e.DepartmentId, "fk_employees_departments");

            entity.HasIndex(e => e.ManagerId, "fk_employees_employees");

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.HireDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasColumnType("timestamp(6)")
                .HasColumnName("hire_date");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(50)
                .HasColumnName("job_title");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasColumnName("middle_name");
            entity.Property(e => e.Salary)
                .HasPrecision(19, 4)
                .HasColumnName("salary");

            entity.HasOne(d => d.Address).WithMany(p => p.Employees)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("fk_employees_addresses");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_employees_departments");

            entity.HasOne(d => d.Manager).WithMany(p => p.DirectReports)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("fk_employees_employees");
        });

        modelBuilder.Entity<EmployeeProject>(entity =>
        {
            entity.HasKey(e => new { e.EmployeeId, e.ProjectId }).HasName("PRIMARY");

            entity.ToTable("employees_projects");

            entity.HasIndex(e => new { e.EmployeeId, e.ProjectId }, "PK_EmployeesProjects").IsUnique();
            entity.HasIndex(e => e.ProjectId, "fk_employees_projects_projects");

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeProjects)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_employees_projects_employees");

            entity.HasOne(d => d.Project).WithMany(p => p.EmployeeProjects)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_employees_projects_projects");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PRIMARY");

            entity.ToTable("projects");

            entity.HasIndex(e => e.ProjectId, "PK_Projects").IsUnique();

            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp(6)")
                .HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.StartDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasColumnType("timestamp(6)")
                .HasColumnName("start_date");
        });

        modelBuilder.Entity<Town>(entity =>
        {
            entity.HasKey(e => e.TownId).HasName("PRIMARY");

            entity.ToTable("towns");

            entity.HasIndex(e => e.TownId, "PK_Towns").IsUnique();

            entity.Property(e => e.TownId).HasColumnName("town_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
