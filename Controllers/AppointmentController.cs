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
    [Authorize] // role yok, sadece giriş yeterli
    public class AppointmentController : Controller
    {
        private readonly FitnessDbContext _context;

        public AppointmentController(FitnessDbContext context)
        {
            _context = context;
        }

        // GET: Appointment
        public async Task<IActionResult> Index()
        {
            var query = _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service);

            return View(await query.ToListAsync());
        }

        // GET: Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // GET: Appointment/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] =
                new SelectList(_context.Members, "Id", "FullName");
            ViewData["TrainerId"] =
                new SelectList(_context.Trainers, "Id", "FullName");
            ViewData["ServiceId"] =
                new SelectList(_context.Services, "Id", "ServiceName");

            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentDate,MemberId,TrainerId,ServiceId")] Appointment appointment)
        {
            // Seçilen servis süresini al
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError("", "Geçersiz hizmet seçimi.");
            }

            if (ModelState.IsValid)
            {
                var startTime = appointment.AppointmentDate;
                var endTime = service != null
                    ? startTime.AddMinutes(service.DurationInMinutes)
                    : startTime;

                // Aynı antrenörün çakışan randevusu var mı?
                var hasConflict = await _context.Appointments
                    .Include(a => a.Service)
                    .AnyAsync(a =>
                        a.TrainerId == appointment.TrainerId &&
                        // tarih çakışma kontrolü
                        a.AppointmentDate < endTime &&
                        a.AppointmentDate.AddMinutes(a.Service.DurationInMinutes) > startTime
                    );

                if (hasConflict)
                {
                    ModelState.AddModelError("AppointmentDate",
                        "Bu eğitmenin bu saat aralığında başka bir randevusu var.");
                }
            }

            if (ModelState.IsValid)
            {
                // Varsayılan olarak onaysız
                appointment.IsApproved = false;

                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // ViewData'ları tekrar doldur
            ViewData["MemberId"] =
                new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);
            ViewData["TrainerId"] =
                new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] =
                new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

            return View(appointment);
        }

        // GET: Appointment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            ViewData["MemberId"] =
                new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);
            ViewData["TrainerId"] =
                new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] =
                new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

            return View(appointment);
        }

        // POST: Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppointmentDate,IsApproved,MemberId,TrainerId,ServiceId")] Appointment appointment)
        {
            if (id != appointment.Id)
                return NotFound();

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError("", "Geçersiz hizmet seçimi.");
            }

            if (ModelState.IsValid && service != null)
            {
                var startTime = appointment.AppointmentDate;
                var endTime = startTime.AddMinutes(service.DurationInMinutes);

                var hasConflict = await _context.Appointments
                    .Include(a => a.Service)
                    .AnyAsync(a =>
                        a.TrainerId == appointment.TrainerId &&
                        a.Id != appointment.Id && // kendisi hariç
                        a.AppointmentDate < endTime &&
                        a.AppointmentDate.AddMinutes(a.Service.DurationInMinutes) > startTime
                    );

                if (hasConflict)
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
                    if (!AppointmentExists(appointment.Id))
                        return NotFound();

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MemberId"] =
                new SelectList(_context.Members, "Id", "FullName", appointment.MemberId);
            ViewData["TrainerId"] =
                new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] =
                new SelectList(_context.Services, "Id", "ServiceName", appointment.ServiceId);

            return View(appointment);
        }

        // GET: Appointment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // POST: Appointment/Delete/5
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

        // Admin onay/reddet aksiyonları
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            appointment.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            appointment.IsApproved = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(a => a.Id == id);
        }
    }
}
