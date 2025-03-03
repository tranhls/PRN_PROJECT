using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRN222_Assm.Models;

public partial class PrnassmContext : DbContext
{
    public PrnassmContext()
    {
    }

    public PrnassmContext(DbContextOptions<PrnassmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<Semmester> Semmesters { get; set; }

    public virtual DbSet<StudentClass> StudentClasses { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(config.GetConnectionString("DBContext"));
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.ToTable("Class");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsFixedLength();

            entity.HasOne(d => d.Semester).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_Class_Semmester");

            entity.HasOne(d => d.SubjectNavigation).WithMany(p => p.Classes)
                .HasForeignKey(d => d.Subject)
                .HasConstraintName("FK_Class_Subject");

            entity.HasOne(d => d.TearcherNavigation).WithMany(p => p.Classes)
                .HasForeignKey(d => d.Tearcher)
                .HasConstraintName("FK_Class_Account");
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.ToTable("Score");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Pt1).HasColumnName("PT1");
            entity.Property(e => e.Pt2).HasColumnName("PT2");
        });

        modelBuilder.Entity<Semmester>(entity =>
        {
            entity.ToTable("Semmester");

            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_StudentClass_1");

            entity.ToTable("StudentClass");

            entity.HasIndex(e => new { e.StudentId, e.ClassId, e.ScoreId }, "IX_StudentClass").IsUnique();

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Class).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentClass_Class1");

            entity.HasOne(d => d.Score).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.ScoreId)
                .HasConstraintName("FK_StudentClass_Score1");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_StudentClass_Account1");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsFixedLength();

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.Category)
                .HasConstraintName("FK_Subject_Category");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
