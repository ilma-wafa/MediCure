using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCure.Data;
using MediCure.Models;

namespace MediCure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PrescriptionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prescription>>> GetAll()
        {
            return await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Prescription>> GetById(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (prescription == null) return NotFound();
            return prescription;
        }

        [HttpPost]
        public async Task<ActionResult<Prescription>> Create(Prescription prescription)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == prescription.PatientId);
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == prescription.DoctorId);
            if (!patientExists) return BadRequest("Patient not found.");
            if (!doctorExists) return BadRequest("Doctor not found.");

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null) return NotFound();
            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}