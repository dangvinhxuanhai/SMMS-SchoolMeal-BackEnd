using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Application.Features.school.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace SMMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SchoolsController : ControllerBase
    {
        private readonly ISchoolRepository _schoolRepository;

        public SchoolsController(ISchoolRepository schoolRepository)
        {
            _schoolRepository = schoolRepository;
        }

        /// <summary>
        /// ✅ Lấy danh sách trường học (có hỗ trợ filter, sort, search qua OData)
        /// </summary>
        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            var schools = _schoolRepository.GetAllSchools()
                .Select(s => new SchoolDTO
                {
                    SchoolId = s.SchoolId,
                    SchoolName = s.SchoolName,
                    ContactEmail = s.ContactEmail,
                    Hotline = s.Hotline,
                    SchoolContract = null,
                    SchoolAddress = s.SchoolAddress,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    StudentCount = s.Students.Count()
                });

            return Ok(schools);
        }

        /// <summary>
        /// ✅ Lấy chi tiết một trường
        /// </summary>
        [EnableQuery]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var school = await _schoolRepository.GetByIdAsync(id);
            if (school == null)
                return NotFound();

            return Ok(school);
        }

        /// <summary>
        /// ✅ Thêm mới trường học
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSchoolDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var school = new School
            {
                SchoolId = Guid.NewGuid(),
                SchoolName = dto.SchoolName,
                ContactEmail = dto.ContactEmail,
                Hotline = dto.Hotline,
                SchoolAddress = dto.SchoolAddress,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _schoolRepository.AddAsync(school);
            return CreatedAtAction(nameof(GetById), new { id = school.SchoolId }, school);
        }

        /// <summary>
        /// ✅ Cập nhật thông tin trường học
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchoolDto dto)
        {
            var existing = await _schoolRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.SchoolName = dto.SchoolName;
            existing.ContactEmail = dto.ContactEmail;
            existing.Hotline = dto.Hotline;
            existing.SchoolAddress = dto.SchoolAddress;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _schoolRepository.UpdateAsync(existing);
            return NoContent();
        }

        /// <summary>
        /// ✅ Xóa trường học
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var school = await _schoolRepository.GetByIdAsync(id);
            if (school == null)
                return NotFound();

            await _schoolRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
