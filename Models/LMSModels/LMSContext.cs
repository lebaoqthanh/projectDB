using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class LMSContext : DbContext
    {
        public LMSContext()
        {
        }

        public LMSContext(DbContextOptions<LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrator> Administrators { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<AssignmentCategory> AssignmentCategories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<EnrollmentGrade> EnrollmentGrades { get; set; } = null!;
        public virtual DbSet<Professor> Professors { get; set; } = null!;
        public virtual DbSet<Sshkey> Sshkeys { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySql("server=atr.eng.utah.edu;database=Team58LMS;uid=u1545345;password=changeme", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.16-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.HasKey(e => e.Uid)
                    .HasName("PRIMARY");

                entity.Property(e => e.Uid)
                    .HasMaxLength(8)
                    .HasColumnName("uid")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("dob");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .HasColumnName("last_name");
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasIndex(e => e.CatId, "cat_id");

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("assignment_id");

                entity.Property(e => e.AssignmentName)
                    .HasMaxLength(100)
                    .HasColumnName("assignment_name");

                entity.Property(e => e.CatId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("cat_id");

                entity.Property(e => e.Contents)
                    .HasColumnType("text")
                    .HasColumnName("contents");

                entity.Property(e => e.DueDate)
                    .HasColumnType("datetime")
                    .HasColumnName("due_date");

                entity.Property(e => e.MaxPoints)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("max_points");

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.CatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignments_ibfk_1");
            });

            modelBuilder.Entity<AssignmentCategory>(entity =>
            {
                entity.HasKey(e => e.CatId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => new { e.ClassId, e.CatName }, "class_id")
                    .IsUnique();

                entity.Property(e => e.CatId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("cat_id");

                entity.Property(e => e.CatName)
                    .HasMaxLength(100)
                    .HasColumnName("cat_name");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("class_id");

                entity.Property(e => e.Weight)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("weight");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.AssignmentCategories)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AssignmentCategories_ibfk_1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => new { e.CourseId, e.SemesterYear, e.SemesterSeason }, "course_id")
                    .IsUnique();

                entity.HasIndex(e => e.ProfessorUid, "professor_uid");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("class_id");

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("course_id");

                entity.Property(e => e.EndTime)
                    .HasColumnType("time")
                    .HasColumnName("end_time");

                entity.Property(e => e.Location)
                    .HasMaxLength(100)
                    .HasColumnName("location");

                entity.Property(e => e.ProfessorUid)
                    .HasMaxLength(8)
                    .HasColumnName("professor_uid")
                    .IsFixedLength();

                entity.Property(e => e.SemesterSeason)
                    .HasColumnType("enum('Spring','Summer','Fall')")
                    .HasColumnName("semester_season");

                entity.Property(e => e.SemesterYear)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("semester_year");

                entity.Property(e => e.StartTime)
                    .HasColumnType("time")
                    .HasColumnName("start_time");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("Classes_ibfk_1");

                entity.HasOne(d => d.ProfessorU)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.ProfessorUid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_2");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => e.DeptId, "dept_id");

                entity.HasIndex(e => new { e.Number, e.DeptId }, "number")
                    .IsUnique();

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("course_id");

                entity.Property(e => e.CourseName)
                    .HasMaxLength(100)
                    .HasColumnName("course_name");

                entity.Property(e => e.DeptId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("dept_id");

                entity.Property(e => e.Number)
                    .HasColumnType("int(11)")
                    .HasColumnName("number");

                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.DeptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Courses_ibfk_1");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DeptId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Subject, "subject")
                    .IsUnique();

                entity.Property(e => e.DeptId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("dept_id");

                entity.Property(e => e.DeptName)
                    .HasMaxLength(100)
                    .HasColumnName("dept_name");

                entity.Property(e => e.Subject)
                    .HasMaxLength(4)
                    .HasColumnName("subject");
            });

            modelBuilder.Entity<EnrollmentGrade>(entity =>
            {
                entity.HasKey(e => new { e.ClassId, e.Uid })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasIndex(e => e.Uid, "uid");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("class_id");

                entity.Property(e => e.Uid)
                    .HasMaxLength(8)
                    .HasColumnName("uid")
                    .IsFixedLength();

                entity.Property(e => e.Grade)
                    .HasMaxLength(2)
                    .HasColumnName("grade");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.EnrollmentGrades)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EnrollmentGrades_ibfk_1");

                entity.HasOne(d => d.UidNavigation)
                    .WithMany(p => p.EnrollmentGrades)
                    .HasForeignKey(d => d.Uid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EnrollmentGrades_ibfk_2");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.Uid)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.DeptId, "dept_id");

                entity.Property(e => e.Uid)
                    .HasMaxLength(8)
                    .HasColumnName("uid")
                    .IsFixedLength();

                entity.Property(e => e.DeptId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("dept_id");

                entity.Property(e => e.Dob).HasColumnName("dob");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .HasColumnName("last_name");

                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.DeptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Professors_ibfk_1");
            });

            modelBuilder.Entity<Sshkey>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("sshkey");

                entity.Property(e => e.Sshkey1)
                    .HasColumnType("text")
                    .HasColumnName("sshkey");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Uid)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Major, "major");

                entity.Property(e => e.Uid)
                    .HasMaxLength(8)
                    .HasColumnName("uid")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("dob");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .HasColumnName("last_name");

                entity.Property(e => e.Major)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("major");

                entity.HasOne(d => d.MajorNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.Major)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Students_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasIndex(e => e.AssignmentId, "assignment_id");

                entity.HasIndex(e => e.Uid, "uid");

                entity.Property(e => e.SubmissionId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("submission_id");

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("assignment_id");

                entity.Property(e => e.Contents)
                    .HasColumnType("text")
                    .HasColumnName("contents");

                entity.Property(e => e.Score)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("score");

                entity.Property(e => e.SubmissionTime)
                    .HasColumnType("datetime")
                    .HasColumnName("submission_time");

                entity.Property(e => e.Uid)
                    .HasMaxLength(8)
                    .HasColumnName("uid")
                    .IsFixedLength();

                entity.HasOne(d => d.Assignment)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.AssignmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submissions_ibfk_2");

                entity.HasOne(d => d.UidNavigation)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.Uid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submissions_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
