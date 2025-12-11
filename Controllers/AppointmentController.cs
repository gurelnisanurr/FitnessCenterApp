using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterApp.Controllers
{
    [Authorize] // giriş yapmış herkes erişir
    public class AppointmentController : Controller
    {
        private readonly FitnessDbContext _context;

        public AppointmentController(FitnessDbContext context)
        {
            _context = context;
        }

        // ADMIN tüm randevuları görür
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var query = _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service);

            return View(await query.ToListAsync());
        }

        // Kullanıcının kendi randevuları
        [Authorize]
        public async Task<IActionResult> MyAppointments()
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
                return Challenge();

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email == userEmail);

            if (member == null)
                return View(new List<Appointment>());

            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.MemberId == member.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName");
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName");
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName");

            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentDate,MemberId,TrainerId,ServiceId")] Appointment appointment)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError("", "Geçersiz hizmet seçimi.");
            }

            if (ModelState.IsValid)
            {
                var start = appointment.AppointmentDate;
                var end = start.AddMinutes(service.DurationInMinutes);

                // Eğitmen başka randevuya denk geliyor mu?
                var conflict = await _context.Appointments
                    .Include(a => a.Service)
                    .AnyAsync(a =>
                        a.TrainerId == appointment.TrainerId &&
                        a.AppointmentDate < end &&
                        a.AppointmentDate.AddMinutes(a.Service.DurationInMinutes) > start
                    );

                if (conflict)
                {
                    ModelState.AddModelError("AppointmentDate",
                        "Bu eğitmenin bu saat aralığında başka bir randevusu var.");
                }
            }

            if (ModelState.IsValid)
            {
                appointment.IsApproved = false;

                _context.Add(appointment);
                await _context.SaveChangesAsync();

                // Admin → Index / Normal kullanıcı → MyAppointments
                if (User.IsInRole("Admin"))
                    return RedirectToAction(nameof(Index));
                else
                    return RedirectToAction(nameof(MyAppointments));
            }

            // dropdown'ları tekrar doldur
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

            return View(appointment);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

            return View(appointment);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppointmentDate,IsApproved,MemberId,TrainerId,ServiceId")] Appointment appointment)
        {
            if (id != appointment.Id)
                return NotFound();

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service != null && ModelState.IsValid)
            {
                var start = appointment.AppointmentDate;
                var end = start.AddMinutes(service.DurationInMinutes);

                var conflict = await _context.Appointments
                    .Include(a => a.Service)
                    .AnyAsync(a =>
                        a.TrainerId == appointment.TrainerId &&
                        a.Id != appointment.Id &&
                        a.AppointmentDate < end &&
                        a.AppointmentDate.AddMinutes(a.Service.DurationInMinutes) > start
                    );

                if (conflict)
                {
                    ModelState.AddModelError("AppointmentDate",
                        "Bu eğitmenin bu saat aralığında başka bir randevusu var.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(a => a.Id == appointment.Id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

            return View(appointment);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // RANDEVU ONAY
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // RANDEVU REDDETME
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.IsApproved = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
