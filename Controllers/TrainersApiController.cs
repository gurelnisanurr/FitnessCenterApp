using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly FitnessDbContext _context;

        public TrainersApiController(FitnessDbContext context)
        {
            _context = context;
        }

        // Örnek çağrı:
        // GET /api/trainersapi/available?date=2025-12-10T10:00:00&serviceId=1
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableTrainers(DateTime date, int serviceId)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                return BadRequest("Geçersiz serviceId");

            var endTime = date.AddMinutes(service.DurationInMinutes);

            var trainers = await _context.Trainers
                .Include(t => t.Services)
                .Include(t => t.Appointments)
                    .ThenInclude(a => a.Service)
                .Where(t =>
                    // Bu hizmeti verebilen eğitmenler
                    t.Services.Any(s => s.Id == serviceId) &&

                    // Verilen saat aralığında çakışan randevusu olmayanlar
                    !t.Appointments.Any(a =>
                        a.AppointmentDate < endTime &&
                        a.AppointmentDate.AddMinutes(a.Service.DurationInMinutes) > date
                    )
                )
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.SpecialtyText
                })
                .ToListAsync();

            return Ok(trainers);
        }
    }
}

