using Clinix.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Clinix.Web.Api.Controllers;

[ApiController]
[Route("api/myappointments")]
[Authorize]
public class MyAppointmentsController : ControllerBase
    {
    private readonly ApplicationDbContext _db;
    public MyAppointmentsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetMyAppointments(CancellationToken ct)
        {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId)) return Unauthorized();

        var appts = await _db.Appointments
            .Include(a => a.AppointmentSlot)
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        var dto = appts.Select(a => new {
            a.Id,
            a.Status,
            a.Reason,
            a.CreatedAt,
            SlotStart = a.AppointmentSlot.StartUtc,
            SlotEnd = a.AppointmentSlot.EndUtc,
            Doctor = new { a.Doctor.Id, a.Doctor.Name, a.Doctor.Specialty }
            });

        return Ok(dto);
        }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId)) return Unauthorized();

        var a = await _db.Appointments
            .Include(x => x.AppointmentSlot)
            .Include(x => x.Doctor)
            .SingleOrDefaultAsync(x => x.Id == id && x.PatientId == userId, ct);

        if (a == null) return NotFound();
        return Ok(new
            {
            a.Id,
            a.Status,
            a.Reason,
            a.CreatedAt,
            SlotStart = a.AppointmentSlot.StartUtc,
            SlotEnd = a.AppointmentSlot.EndUtc,
            Doctor = new { a.Doctor.Id, a.Doctor.Name, a.Doctor.Specialty }
            });
        }
    }
