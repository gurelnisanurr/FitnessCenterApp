using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AppointmentController : Controller
{
    private readonly FitnessDbContext _context;

    public AppointmentController(FitnessDbContext context)
    {
        _context = context;
    }

    // GET: Tüm Randevuları Listele (Admin İçin)
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

    // GET: Detay Sayfası
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _context.Appointments
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .Include(a => a.FitnessCenter)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (appointment == null) return NotFound();

        return View(appointment);
    }

    // GET: Yeni Randevu Oluşturma Sayfası
    public IActionResult Create()
    {
        LoadDropdowns();
        return View(new Appointment());
    }

    // POST: Yeni Randevu Kaydı
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        if (ModelState.IsValid)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        LoadDropdowns();
        return View(appointment);
    }

    // GET: Randevu Düzenleme Sayfası
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        LoadDropdowns();
        return View(appointment);
    }

    // POST: Randevu Güncelleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.Id) return NotFound();

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

    // GET: Randevu Silme Onay Sayfası
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _context.Appointments
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .Include(a => a.FitnessCenter)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (appointment == null) return NotFound();

        return View(appointment);
    }

    // POST: Randevu Silme İşlemi
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

    // Dropdownları Yükleme Yardımcı Metodu
    private void LoadDropdowns()
    {
        ViewBag.Members = _context.Members.ToList();
        ViewBag.Trainers = _context.Trainers.ToList();
        ViewBag.Services = _context.Services.ToList();
        ViewBag.FitnessCenters = _context.FitnessCenters.ToList();
    }

    // POST: Randevu Onaylama (Admin)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var app = await _context.Appointments.FindAsync(id);
        if (app != null)
        {
            app.IsApproved = true;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Onayı Geri Çekme (Admin)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int id)
    {
        var app = await _context.Appointments.FindAsync(id);
        if (app != null)
        {
            app.IsApproved = false;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}