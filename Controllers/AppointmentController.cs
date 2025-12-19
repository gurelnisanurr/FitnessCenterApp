using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly FitnessDbContext _context;

        public AppointmentController(FitnessDbContext context)
        {
            _context = context;
        }

        // =======================
        // ADMIN TÜM RANDEVULAR
        // =======================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(list);
        }

        // =======================
        // KULLANICI KENDİ RANDEVULARI
        // =======================
        public async Task<IActionResult> MyAppointments()
        {
            var email = User.Identity?.Name;
            if (email == null) return Challenge();

            var list = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserEmail == email)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(list);
        }

        // =======================
        // CREATE GET
        // =======================
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
                ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName");

            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName");
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName");

            return View();
        }

        // =======================
        // CREATE POST
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            var email = User.Identity?.Name;
            if (email == null) return Challenge();

            // 🔑 SADECE EMAIL
            appointment.UserEmail = email;

            // Eski FK’lerden kalan validationları iptal et
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            // 👤 Normal kullanıcı → Member otomatik
            if (!User.IsInRole("Admin"))
            {
                var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
                if (member == null)
                {
                    ModelState.AddModelError("", "Üyelik kaydı bulunamadı.");
                }
                else
                {
                    appointment.MemberId = member.Id;
                }
            }

            // Service
            var service = await _context.Services.FindAsync(appointment.ServiceId);
            if (service == null)
                ModelState.AddModelError("ServiceId", "Geçersiz hizmet.");

            // Trainer
            var trainer = await _context.Trainers.FindAsync(appointment.TrainerId);
            if (trainer == null)
                ModelState.AddModelError("TrainerId", "Geçersiz eğitmen.");

            // Saat + Çakışma
            if (ModelState.IsValid)
            {
                var start = appointment.AppointmentDate;
                var end = start.AddMinutes(service.DurationInMinutes);

                if (start.TimeOfDay < trainer.AvailableFrom ||
                    end.TimeOfDay > trainer.AvailableTo)
                {
                    ModelState.AddModelError("AppointmentDate",
                        $"Eğitmen müsait değil ({trainer.AvailableFrom:hh\\:mm}-{trainer.AvailableTo:hh\\:mm})");
                }

                var conflict = await _context.Appointments
                    .Include(a => a.Service)
                    .AnyAsync(a =>
                        a.TrainerId == appointment.TrainerId &&
                        a.AppointmentDate < end &&
                        a.AppointmentDate.AddMinutes(a.Service.DurationInMinutes) > start);

                if (conflict)
                    ModelState.AddModelError("AppointmentDate", "Bu saat için randevu var.");
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                    ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);

                ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

                return View(appointment);
            }

            appointment.IsApproved = false;
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAppointments));
        }

        // =======================
        // DELETE
        // =======================
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            if (!User.IsInRole("Admin") && appointment.UserEmail != User.Identity!.Name)
                return Forbid();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            if (!User.IsInRole("Admin") && appointment.UserEmail != User.Identity!.Name)
                return Forbid();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAppointments));
        }
    }
}
