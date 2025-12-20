using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FitnessCentersController : Controller
    {
        private readonly FitnessDbContext _context;

        public FitnessCentersController(FitnessDbContext context)
        {
            _context = context;
        }

        // GET: FitnessCenters
        public async Task<IActionResult> Index()
        {
            return View(await _context.FitnessCenters.ToListAsync());
        }

        // GET: FitnessCenters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCenter = await _context.FitnessCenters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fitnessCenter == null)
            {
                return NotFound();
            }

            return View(fitnessCenter);
        }

        // GET: FitnessCenters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FitnessCenters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,WorkingHours")] FitnessCenter fitnessCenter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fitnessCenter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fitnessCenter);
        }

        // GET: FitnessCenters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCenter = await _context.FitnessCenters.FindAsync(id);
            if (fitnessCenter == null)
            {
                return NotFound();
            }
            return View(fitnessCenter);
        }

        // POST: FitnessCenters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,WorkingHours")] FitnessCenter fitnessCenter)
        {
            if (id != fitnessCenter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fitnessCenter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fitnessCenter);
        }

        // GET: FitnessCenters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCenter = await _context.FitnessCenters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fitnessCenter == null)
            {
                return NotFound();
            }

            return View(fitnessCenter);
        }

        // POST: FitnessCenters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fitnessCenter = await _context.FitnessCenters
                .Include(f => f.Appointments)
                .Include(f => f.Services)
                    .ThenInclude(s => s.Trainers)
                .Include(f => f.Trainers)
                    .ThenInclude(t => t.Services)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fitnessCenter == null)
                return NotFound();

            // 1️⃣ RANDEVULAR
            _context.Appointments.RemoveRange(fitnessCenter.Appointments);

            // 2️⃣ SERVICE ↔ TRAINER BAĞLANTILARI
            foreach (var service in fitnessCenter.Services)
            {
                service.Trainers.Clear();
            }

            foreach (var trainer in fitnessCenter.Trainers)
            {
                trainer.Services.Clear();
            }

            // 3️⃣ SERVICES
            _context.Services.RemoveRange(fitnessCenter.Services);

            // 4️⃣ TRAINERS
            _context.Trainers.RemoveRange(fitnessCenter.Trainers);

            // 5️⃣ SALON
            _context.FitnessCenters.Remove(fitnessCenter);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
