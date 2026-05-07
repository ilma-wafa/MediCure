using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCure.Data;
using MediCure.Models;

namespace MediCure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LabReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LabReportsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LabReport>>> GetAll()
        {
            return await _context.LabReports
                .Include(l => l.Patient)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LabReport>> GetById(int id)
        {
            var report = await _context.LabReports
                .Include(l => l.Patient)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (report == null) return NotFound();
            return report;
        }

        [HttpPost]
        public async Task<ActionResult<LabReport>> Create(LabReport report)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == report.PatientId);
            if (!patientExists) return BadRequest("Patient not found.");

            _context.LabReports.Add(report);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var report = await _context.LabReports.FindAsync(id);
            if (report == null) return NotFound();

            var validStatuses = new[] { "Pending", "Processing", "Completed" };
            if (!validStatuses.Contains(status))
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");

            report.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.LabReports.FindAsync(id);
            if (report == null) return NotFound();
            _context.LabReports.Remove(report);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}