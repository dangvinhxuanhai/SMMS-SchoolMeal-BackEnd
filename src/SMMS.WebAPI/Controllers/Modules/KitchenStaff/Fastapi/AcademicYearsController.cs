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

[Route("fastapi/Years")]
[ApiController]
public class AcademicYearsController : ControllerBase
{
    private readonly EduMealContext _context;

    public AcademicYearsController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/AcademicYears
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AcademicYear>>> GetAcademicYears()
    {
        return await _context.AcademicYears.ToListAsync();
    }

    // GET: api/AcademicYears/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AcademicYear>> GetAcademicYear(int id)
    {
        var academicYear = await _context.AcademicYears.FindAsync(id);

        if (academicYear == null)
        {
            return NotFound();
        }

        return academicYear;
    }

    // PUT: api/AcademicYears/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAcademicYear(int id, AcademicYear academicYear)
    {
        if (id != academicYear.YearId)
        {
            return BadRequest();
        }

        _context.Entry(academicYear).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AcademicYearExists(id))
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

    // POST: api/AcademicYears
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<AcademicYear>> PostAcademicYear([FromBody] CreateAcademicYearDto dto)
    {
        var year = new AcademicYear
        {
            YearName = dto.YearName,
            BoardingStartDate = dto.BoardingStartDate,
            BoardingEndDate = dto.BoardingEndDate,
            SchoolId = dto.SchoolId
        };

        _context.AcademicYears.Add(year);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAcademicYear), new { id = year.YearId }, year);
    }


    // DELETE: api/AcademicYears/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAcademicYear(int id)
    {
        var academicYear = await _context.AcademicYears.FindAsync(id);
        if (academicYear == null)
        {
            return NotFound();
        }

        _context.AcademicYears.Remove(academicYear);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool AcademicYearExists(int id)
    {
        return _context.AcademicYears.Any(e => e.YearId == id);
    }
}
