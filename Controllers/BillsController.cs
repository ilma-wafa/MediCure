using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCure.Data;
using MediCure.Models;

namespace MediCure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BillsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bill>>> GetAll()
        {
            return await _context.Bills
                .Include(b => b.Patient)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bill>> GetById(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bill == null) return NotFound();
            return bill;
        }

        [HttpPost]
        public async Task<ActionResult<Bill>> Create(Bill bill)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == bill.PatientId);
            if (!patientExists) return BadRequest("Patient not found.");

            if (bill.PaidAmount > bill.TotalAmount)
                return BadRequest("Paid amount cannot exceed total amount.");

            bill.Status = bill.PaidAmount == 0 ? "Unpaid" :
                          bill.PaidAmount < bill.TotalAmount ? "Partial" : "Paid";

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = bill.Id }, bill);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return NotFound();
            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
