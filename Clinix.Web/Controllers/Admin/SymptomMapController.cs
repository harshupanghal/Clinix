using Clinix.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinix.Domain.Entities.Appointments;

namespace Clinix.Web.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/symptom-maps")]
[Authorize(Roles = "Admin")]
public class SymptomMapController : ControllerBase
    {
    private readonly ClinixDbContext _db;
    public SymptomMapController(ClinixDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await _db.SymptomSpecialtyMaps.OrderBy(s => s.Keyword).ToListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SymptomSpecialtyMap dto, CancellationToken ct)
        {
        _db.SymptomSpecialtyMaps.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
        var map = await _db.SymptomSpecialtyMaps.FindAsync(new object[] { id }, ct);
        if (map == null) return NotFound();
        return Ok(map);
        }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SymptomSpecialtyMap dto, CancellationToken ct)
        {
        if (id != dto.Id) return BadRequest();
        _db.Entry(dto).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
        return NoContent();
        }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
        var map = await _db.SymptomSpecialtyMaps.FindAsync(new object[] { id }, ct);
        if (map == null) return NotFound();
        _db.SymptomSpecialtyMaps.Remove(map);
        await _db.SaveChangesAsync(ct);
        return NoContent();
        }
    }
