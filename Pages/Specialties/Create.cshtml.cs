using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Specialties;

[Authorize(Roles = AppRoles.AdminOnly)]
public class CreateModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly IUserAccountService _userAccountService;

    public CreateModel(IDoctorService doctorService, IUserAccountService userAccountService)
    {
        _doctorService = doctorService;
        _userAccountService = userAccountService;
    }

    [BindProperty]
    public Doctor Doctor { get; set; } = default!;

    [BindProperty]
    [Required, DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        Doctor = new Doctor { User = new User() };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Doctor.User.PasswordHash");
        ModelState.Remove("Doctor.User.Role");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var user = Doctor.User;
            user.PasswordHash = _userAccountService.HashPassword(user, Password);
            user.Role = AppRoles.Doctor;
            user.CreatedAt = DateTime.UtcNow;

            await _doctorService.AddDoctorAsync(Doctor);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
