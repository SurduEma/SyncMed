using Microsoft.AspNetCore.Mvc;
using SyncMed.Services;

namespace SyncMed.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentService _service;

    public AppointmentsController(IAppointmentService service)
    {
        _service = service;
    }

    // GET: Appointments
    public async Task<IActionResult> Index()
    {
        var appointments = await _service.GetAllAppointmentsAsync();
        return View(appointments);
    }

    // GET: Appointments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var appointment = await _service.GetAppointmentByIdAsync(id.Value);
        if (appointment == null)
            return NotFound();

        return View(appointment);
    }
}
