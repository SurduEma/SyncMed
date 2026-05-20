using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Services;

namespace SyncMed.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IUserAccountService _userAccountService;

    public RegisterModel(IUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _userAccountService.RegisterPatientAsync(
            Input.FirstName,
            Input.LastName,
            Input.Email,
            Input.Password,
            Input.DateOfBirth,
            Input.PhoneNumber);

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        var principal = _userAccountService.CreatePrincipal(result.User);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToPage("/Appointments/Index");
    }

    public class InputModel
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
