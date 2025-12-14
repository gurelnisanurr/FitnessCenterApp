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
    [Authorize] // giriş yapmış herkes
    public class AppointmentController : Controller
    {
        private readonly FitnessDbContext _context;

        public AppointmentController(FitnessDbContext context)
        {
            _context = context;
        }

        // =========================
        // ADMIN → TÜM RANDEVULAR
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // =========================
        // KULLANICI → KENDİ RANDEVULARI
        // =========================
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

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName");
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName");
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName");

            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AppointmentDate,MemberId,TrainerId,ServiceId")] Appointment appointment)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError("", "Geçersiz hizmet seçimi.");
            }

            if (ModelState.IsValid)
            {
                var start = appointment.AppointmentDate;
                var end = start.AddMinutes(service.DurationInMinutes);

                // Eğitmen saat çakışma kontrolü
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

                return User.IsInRole("Admin")
                    ? RedirectToAction(nameof(Index))
                    : RedirectToAction(nameof(MyAppointments));
            }

            // dropdown tekrar doldur
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

            return View(appointment);
        }

        // =========================
        // EDIT (GET)
        // =========================
        [Authorize(Roles = "Admin")]
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

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,AppointmentDate,IsApproved,MemberId,TrainerId,ServiceId")] Appointment appointment)
        {
            if (id != appointment.Id)
                return NotFound();

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

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
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

            return View(appointment);
        }

        // =========================
        // DELETE
        // =========================
        [Authorize(Roles = "Admin")]
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        // =========================
        // ONAY / RED
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
