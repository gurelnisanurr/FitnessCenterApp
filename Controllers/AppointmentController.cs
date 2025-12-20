using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AppointmentController : Controller
{
    private readonly FitnessDbContext _context;

    public AppointmentController(FitnessDbContext context)
    {
        _context = context;
    }

    // GET: Tüm Randevuları Listele (Admin)
    public async Task<IActionResult> Index()
    {
        var list = await _context.Appointments
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .Include(a => a.FitnessCenter)
            .ToListAsync();
        return View(list);
    }

    // GET: Kullanıcının Kendi Randevuları
    [Authorize]
    public async Task<IActionResult> MyAppointments()
    {
        var userEmail = User.Identity?.Name;
        var myAppointments = await _context.Appointments
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .Include(a => a.FitnessCenter)
            .Include(a => a.Member)
            .Where(a => a.Member.Email == userEmail)
            .ToListAsync();

        return View(myAppointments);
    }

    // GET: Yeni Randevu Sayfası
    public IActionResult Create()
    {
        LoadDropdowns();
        // Saniyeleri temizlenmiş başlangıç tarihi
        var model = new Appointment
        {
            AppointmentDate = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute)
        };
        return View(model);
    }

    // POST: Yeni Randevu Kaydı
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        // 1. ZAMAN TEMİZLEME: Saniye ve milisaniyeyi sıfırla
        appointment.AppointmentDate = new DateTime(
            appointment.AppointmentDate.Year, appointment.AppointmentDate.Month, appointment.AppointmentDate.Day,
            appointment.AppointmentDate.Hour, appointment.AppointmentDate.Minute, 0
        );

        // 2. ÇAKIŞMA KONTROLÜ (LINQ): Aynı hoca, aynı saatte dolu mu?
        bool isBusy = _context.Appointments.Any(a =>
            a.TrainerId == appointment.TrainerId &&
            a.AppointmentDate == appointment.AppointmentDate);

        if (isBusy)
        {
            // Hata mesajı ekle
            ModelState.AddModelError("AppointmentDate", "Seçilen antrenör bu saatte dolu. Lütfen başka bir zaman seçiniz.");

            // NULL HATASINI ÖNLEYEN KRİTİK ADIM: Listeleri tekrar yükle
            LoadDropdowns();
            return View(appointment);
        }

        // 3. GEÇMİŞ TARİH KONTROLÜ
        if (appointment.AppointmentDate < DateTime.Now)
        {
            ModelState.AddModelError("AppointmentDate", "Geçmiş bir tarihe randevu alınamaz.");
            LoadDropdowns();
            return View(appointment);
        }

        if (ModelState.IsValid)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Model geçerli değilse listeleri tekrar yükle
        LoadDropdowns();
        return View(appointment);
    }

    // GET: Düzenleme Sayfası
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        LoadDropdowns();
        return View(appointment);
    }

    // POST: Düzenleme İşlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.Id) return NotFound();

        // Düzenleme sırasında çakışma kontrolü (Kendisi hariç)
        bool isConflict = _context.Appointments.Any(a =>
            a.Id != id &&
            a.TrainerId == appointment.TrainerId &&
            a.AppointmentDate == appointment.AppointmentDate);

        if (isConflict)
        {
            ModelState.AddModelError("AppointmentDate", "Seçilen saatte antrenör başka bir randevusu olduğu için uygun değil.");
            LoadDropdowns();
            return View(appointment);
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
                if (!_context.Appointments.Any(e => e.Id == appointment.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        LoadDropdowns();
        return View(appointment);
    }

    // Yükleme sırasında patlamayı önleyen güvenli liste yükleyici
    private void LoadDropdowns()
    {
        ViewBag.Members = _context.Members.ToList() ?? new List<Member>();
        ViewBag.Trainers = _context.Trainers.ToList() ?? new List<Trainer>();
        ViewBag.Services = _context.Services.ToList() ?? new List<Service>();
        ViewBag.FitnessCenters = _context.FitnessCenters.ToList() ?? new List<FitnessCenter>();
    }

    // Onay ve Silme Metodları
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var app = await _context.Appointments.FindAsync(id);
        if (app != null) { app.IsApproved = true; await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null) { _context.Appointments.Remove(appointment); await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}