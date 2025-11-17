using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi;

[Route("fastapi/Schools")]
[ApiController]
public class SchoolsController : ControllerBase
{
    private readonly EduMealContext _context;

    public SchoolsController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/Schools
    [HttpGet]
    public async Task<ActionResult<IEnumerable<School>>> GetSchools()
    {
        return await _context.Schools.ToListAsync();
    }

    // GET: api/Schools/5
    [HttpGet("{id}")]
    public async Task<ActionResult<School>> GetSchool(Guid id)
    {
        var school = await _context.Schools.FindAsync(id);

        if (school == null)
        {
            return NotFound();
        }

        return school;
    }

    // PUT: api/Schools/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSchool(Guid id, School school)
    {
        if (id != school.SchoolId)
        {
            return BadRequest();
        }

        _context.Entry(school).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SchoolExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Schools
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<School>> PostSchool([FromBody] CreateSchoolDto dto)
    {
        var school = new School
        {
            SchoolId = Guid.NewGuid(),
            SchoolName = dto.SchoolName,
            ContactEmail = dto.ContactEmail,
            Hotline = dto.Hotline,
            SchoolContract = dto.SchoolContract,
            SchoolAddress = dto.SchoolAddress,
            IsActive = dto.IsActive,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Schools.Add(school);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSchool), new { id = school.SchoolId }, school);
    }


    // DELETE: api/Schools/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchool(Guid id)
    {
        var school = await _context.Schools.FindAsync(id);
        if (school == null)
        {
            return NotFound();
        }

        _context.Schools.Remove(school);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool SchoolExists(Guid id)
    {
        return _context.Schools.Any(e => e.SchoolId == id);
    }
}
