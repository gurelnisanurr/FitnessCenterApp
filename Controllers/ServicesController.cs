using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly FitnessDbContext _context;

        public ServicesController(FitnessDbContext context)
        {
            _context = context;
        }

        // GET: Services
        public async Task<IActionResult> Index()
        {
            var services = _context.Services
                .Include(s => s.FitnessCenter);

            return View(await services.ToListAsync());
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await _context.Services
                .Include(s => s.FitnessCenter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
                return NotFound();

            return View(service);
        }

        // GET: Services/Create
        public IActionResult Create()
        {
            ViewBag.FitnessCenterId =
                new SelectList(_context.FitnessCenters, "Id", "Name");

            return View();
        }

        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ServiceName,DurationInMinutes,Price,FitnessCenterId")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FitnessCenterId =
                new SelectList(_context.FitnessCenters, "Id", "Name", service.FitnessCenterId);

            return View(service);
        }

        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            ViewBag.FitnessCenterId =
                new SelectList(_context.FitnessCenters, "Id", "Name", service.FitnessCenterId);

            return View(service);
        }

        // POST: Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,ServiceName,DurationInMinutes,Price,FitnessCenterId")] Service service)
        {
            if (id != service.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.FitnessCenterId =
                new SelectList(_context.FitnessCenters, "Id", "Name", service.FitnessCenterId);

            return View(service);
        }

        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await _context.Services
                .Include(s => s.FitnessCenter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
                return NotFound();

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services
                .Include(s => s.Trainers)        // Eğitmenlerle olan ilişki
                .Include(s => s.Appointments)    // Randevular
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
                return NotFound();

            // Eğitmen – Hizmet (ServiceTrainer) ilişkilerini temizle
            service.Trainers.Clear();

            // Bu hizmete bağlı randevuları sil
            if (service.Appointments != null && service.Appointments.Any())
            {
                _context.Appointments.RemoveRange(service.Appointments);
            }

            // Hizmeti sil
            _context.Services.Remove(service);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}

