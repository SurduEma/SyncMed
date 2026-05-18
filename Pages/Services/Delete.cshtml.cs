using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Services;

[Authorize(Roles = AppRoles.AdminOnly)]
public class DeleteModel : PageModel
{
    private readonly IMedicalServiceModelService _service;

    public DeleteModel(IMedicalServiceModelService service)
    {
        _service = service;
    }

    [BindProperty]
    public MedicalService MedicalService { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        MedicalService = await _service.GetServiceByIdAsync(id);

        if (MedicalService == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _service.DeleteServiceAsync(id);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error deleting service: {ex.Message}");
            MedicalService = await _service.GetServiceByIdAsync(id);
            return Page();
        }
    }
}
