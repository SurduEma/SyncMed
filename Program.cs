using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncMed.Authorization;
using SyncMed.Data;
using SyncMed.Data.Repositories;
using SyncMed.Models;
using SyncMed.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin));
    options.AddPolicy(AppPolicies.PatientOrAdmin, policy => policy.RequireRole(AppRoles.Patient, AppRoles.Admin));
    options.AddPolicy(AppPolicies.DoctorOrAdmin, policy => policy.RequireRole(AppRoles.Doctor, AppRoles.Admin));
    options.AddPolicy(AppPolicies.NurseOrAdmin, policy => policy.RequireRole(AppRoles.Nurse, AppRoles.Admin));
    options.AddPolicy(AppPolicies.StaffOrAdmin, policy => policy.RequireRole(AppRoles.Doctor, AppRoles.Nurse, AppRoles.Admin));
    options.AddPolicy(AppPolicies.AnyAuthenticatedRole, policy => policy.RequireRole(AppRoles.Patient, AppRoles.Doctor, AppRoles.Nurse, AppRoles.Admin));
});

builder.Services.AddDbContext<SyncMedDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
builder.Services.AddScoped<IMedicalServiceRepository, MedicalServiceRepository>();

// Register Services
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
builder.Services.AddScoped<IMedicalServiceModelService, MedicalServiceModelService>();
builder.Services.AddScoped<IAppointmentNotificationService, AppointmentNotificationService>();
builder.Services.AddScoped<IClinicalRecordService, ClinicalRecordService>();

WebApplication app;
try
{
    app = builder.Build();
}
catch (Exception ex)
{
    // Log the exception to console to help debugging during development
    Console.Error.WriteLine("Application build failed: " + ex);
    // Rethrow so the host will surface the original error to the debugger
    throw;
}

// Seed demo accounts and baseline catalog data.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SyncMedDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    db.Database.Migrate();

    var admin = EnsureUser(db, passwordHasher, "System", "Admin", "admin@syncmed.com", AppRoles.Admin, "Admin123!");
    var patient = EnsureUser(db, passwordHasher, "John", "Doe", "john.doe@example.com", AppRoles.Patient, "Patient123!");
    EnsurePatient(db, patient, new DateOnly(1990, 5, 15), "555-0100");

    var nurse = EnsureUser(db, passwordHasher, "Nora", "Williams", "nora.williams@syncmed.com", AppRoles.Nurse, "Nurse123!");
    EnsureNurse(db, nurse, "NUR-001");

    var alice = EnsureUser(db, passwordHasher, "Alice", "Smith", "alice.smith@syncmed.com", AppRoles.Doctor, "Doctor123!");
    EnsureDoctor(db, alice, "Cardiology", "LIC-001", "Mon-Fri 09:00-17:00");

    var robert = EnsureUser(db, passwordHasher, "Robert", "Johnson", "robert.johnson@syncmed.com", AppRoles.Doctor, "Doctor123!");
    EnsureDoctor(db, robert, "Neurology", "LIC-002", "Mon-Fri 08:00-16:00");

    var maria = EnsureUser(db, passwordHasher, "Maria", "Garcia", "maria.garcia@syncmed.com", AppRoles.Doctor, "Doctor123!");
    EnsureDoctor(db, maria, "General Practice", "LIC-003", "Mon-Fri 10:00-18:00");

    EnsureMedicalServices(db);

    _ = admin;
    db.SaveChanges();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

static User EnsureUser(
    SyncMedDbContext db,
    IPasswordHasher<User> passwordHasher,
    string firstName,
    string lastName,
    string email,
    string role,
    string password)
{
    var user = db.Users.FirstOrDefault(u => u.Email == email);
    if (user == null)
    {
        user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    user.FirstName = firstName;
    user.LastName = lastName;
    user.Role = role;
    user.PasswordHash = passwordHasher.HashPassword(user, password);
    db.SaveChanges();
    return user;
}

static void EnsurePatient(SyncMedDbContext db, User user, DateOnly dateOfBirth, string phoneNumber)
{
    var patient = db.Patients.FirstOrDefault(p => p.PatientId == user.UserId);
    if (patient == null)
    {
        db.Patients.Add(new Patient
        {
            PatientId = user.UserId,
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber
        });
        return;
    }

    patient.DateOfBirth = dateOfBirth;
    patient.PhoneNumber = phoneNumber;
}

static void EnsureDoctor(SyncMedDbContext db, User user, string specialty, string licenseId, string workingHours)
{
    var doctor = db.Doctors.FirstOrDefault(d => d.DoctorId == user.UserId);
    if (doctor == null)
    {
        db.Doctors.Add(new Doctor
        {
            DoctorId = user.UserId,
            Specialty = specialty,
            DoctorLicenseId = licenseId,
            WorkingHours = workingHours
        });
        return;
    }

    doctor.Specialty = specialty;
    doctor.DoctorLicenseId = licenseId;
    doctor.WorkingHours = workingHours;
}

static void EnsureNurse(SyncMedDbContext db, User user, string licenseId)
{
    var nurse = db.Nurses.FirstOrDefault(n => n.NurseId == user.UserId);
    if (nurse == null)
    {
        db.Nurses.Add(new Nurse
        {
            NurseId = user.UserId,
            NurseLicenseId = licenseId
        });
        return;
    }

    nurse.NurseLicenseId = licenseId;
}

static void EnsureMedicalServices(SyncMedDbContext db)
{
    if (db.MedicalServices.Any())
        return;

    db.MedicalServices.AddRange(
        new MedicalService { Specialty = "General Practice", Name = "General Consultation", Description = "Primary care consultation and care plan.", Price = 80 },
        new MedicalService { Specialty = "Cardiology", Name = "Cardiac Evaluation", Description = "Heart health review and cardiovascular assessment.", Price = 160 },
        new MedicalService { Specialty = "Neurology", Name = "Neurology Consultation", Description = "Assessment for nervous system symptoms and conditions.", Price = 170 },
        new MedicalService { Specialty = "Pediatrics", Name = "Pediatric Checkup", Description = "Preventive visit for children and adolescents.", Price = 95 }
    );
}
