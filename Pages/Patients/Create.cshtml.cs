using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Patients;

[Authorize(Roles = AppRoles.AdminOnly)]
public class CreateModel : PageModel
{
    private readonly IPatientService _service;
    private readonly IUserAccountService _userAccountService;

    public CreateModel(IPatientService service, IUserAccountService userAccountService)
    {
        _service = service;
        _userAccountService = userAccountService;
    }

    [BindProperty]
    public Patient Patient { get; set; } = default!;

    [BindProperty]
    [Required, DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        Patient = new Patient { User = new User() };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Patient.User.PasswordHash");
        ModelState.Remove("Patient.User.Role");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Create user first
            var user = Patient.User;
            user.PasswordHash = _userAccountService.HashPassword(user, Password);
            user.Role = AppRoles.Patient;
            user.CreatedAt = DateTime.UtcNow;

            await _service.AddPatientAsync(Patient);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
