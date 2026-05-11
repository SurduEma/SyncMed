using Microsoft.EntityFrameworkCore;
using SyncMed.Models;

namespace SyncMed.Data;

public class SyncMedDbContext : DbContext
{
    public SyncMedDbContext(DbContextOptions<SyncMedDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Nurse> Nurses => Set<Nurse>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<TestResult> TestResults => Set<TestResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Doctor — shared PK with User (1:1)
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasOne(d => d.User)
                  .WithOne(u => u.Doctor)
                  .HasForeignKey<Doctor>(d => d.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(d => d.DoctorLicenseId).IsUnique();
        });

        // Nurse — shared PK with User (1:1)
        modelBuilder.Entity<Nurse>(entity =>
        {
            entity.HasOne(n => n.User)
                  .WithOne(u => u.Nurse)
                  .HasForeignKey<Nurse>(n => n.NurseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(n => n.NurseLicenseId).IsUnique();
        });

        // Patient — shared PK with User (1:1)
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Patient)
                  .HasForeignKey<Patient>(p => p.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasOne(a => a.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Consultation — 1:1 with Appointment
        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.HasIndex(c => c.AppointmentId).IsUnique();

            entity.HasOne(c => c.Appointment)
                  .WithOne(a => a.Consultation)
                  .HasForeignKey<Consultation>(c => c.AppointmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Nurse)
                  .WithMany(n => n.Consultations)
                  .HasForeignKey(c => c.NurseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Prescription
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasOne(p => p.Patient)
                  .WithMany(pt => pt.Prescriptions)
                  .HasForeignKey(p => p.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Doctor)
                  .WithMany(d => d.ApprovedPrescriptions)
                  .HasForeignKey(p => p.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.DraftedByNurse)
                  .WithMany(n => n.DraftedPrescriptions)
                  .HasForeignKey(p => p.DraftedByNurseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TestResult
        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasOne(t => t.Patient)
                  .WithMany(p => p.TestResults)
                  .HasForeignKey(t => t.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.UploadedByDoctor)
                  .WithMany(d => d.UploadedTestResults)
                  .HasForeignKey(t => t.UploadedByDoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
