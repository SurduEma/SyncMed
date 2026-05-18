using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SyncMed.Authorization;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Services;

[Authorize(Roles = AppRoles.AdminOnly)]
public class EditModel : PageModel
{
    private readonly IMedicalServiceModelService _service;

    public EditModel(IMedicalServiceModelService service)
    {
        _service = service;
    }

    [BindProperty]
    public MedicalService MedicalService { get; set; } = default!;

    public SelectList Specialties { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        MedicalService = await _service.GetServiceByIdAsync(id);

        if (MedicalService == null)
            return NotFound();

        LoadSpecialties();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadSpecialties();
            return Page();
        }

        try
        {
            await _service.UpdateServiceAsync(MedicalService);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            LoadSpecialties();
            return Page();
        }
    }

    private void LoadSpecialties()
    {
        Specialties = new SelectList(IndexModel.AllSpecialties);
    }
}
