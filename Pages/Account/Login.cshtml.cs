using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Services;

namespace SyncMed.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IUserAccountService _userAccountService;

    public LoginModel(IUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

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

        var result = await _userAccountService.ValidateCredentialsAsync(Input.Email, Input.Password);
        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        var principal = _userAccountService.CreatePrincipal(result.User);
        var properties = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("/Appointments/Index");
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
