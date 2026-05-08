using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCure.Data;
using MediCure.Models;
using MediCure.Services;

namespace MediCure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NoShowPredictionService _predictionService;

        public AppointmentsController(AppDbContext context, NoShowPredictionService predictionService)
        {
            _context = context;
            _predictionService = predictionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAll()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetById(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();
            return appointment;
        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> Create(Appointment appointment)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == appointment.PatientId);
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == appointment.DoctorId);

            if (!patientExists) return BadRequest("Patient not found.");
            if (!doctorExists) return BadRequest("Doctor not found.");

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var validStatuses = new[] { "Scheduled", "Completed", "Cancelled", "NoShow" };
            if (!validStatuses.Contains(status))
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");

            appointment.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("predict-noshow")]
        public ActionResult PredictNoShow([FromBody] NoShowInputData input)
        {
            if (input.Age < 0 || input.Age > 120)
                return BadRequest("Invalid age value.");

            if (input.DaysInAdvance < 0)
                return BadRequest("Days in advance cannot be negative.");

            var result = _predictionService.Predict(input);

            return Ok(new
            {
                willNoShow = result.WillNoShow,
                noShowProbability = $"{result.NoShowProbability}%",
                riskLevel = result.RiskLevel,
                recommendation = result.Recommendation
            });
        }
    }
}