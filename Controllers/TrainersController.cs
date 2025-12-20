using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using FitnessCenterApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly FitnessDbContext _context;

        public TrainersController(FitnessDbContext context)
        {
            _context = context;
        }

        // GET: Trainers
        public async Task<IActionResult> Index()
        {
            var trainers = _context.Trainers
                .Include(t => t.FitnessCenter);

            return View(await trainers.ToListAsync());
        }

        // GET: Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .Include(t => t.Services)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            return View(trainer);
        }

        // GET: Trainers/Create
        public IActionResult Create()
        {
            var model = new TrainerViewModel
            {
                Trainer = new Trainer(),
                Services = _context.Services
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.ServiceName
                    })
                    .ToList()
            };

            ViewData["FitnessCenterId"] =
                new SelectList(_context.FitnessCenters, "Id", "Name");

            return View(model);
        }

        // POST: Trainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Services = _context.Services
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.ServiceName
                    }).ToList();

                ViewData["FitnessCenterId"] =
                    new SelectList(_context.FitnessCenters, "Id", "Name", model.Trainer.FitnessCenterId);

                return View(model);
            }

            _context.Trainers.Add(model.Trainer);
            await _context.SaveChangesAsync();

            if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
            {
                var services = await _context.Services
                    .Where(s => model.SelectedServiceIds.Contains(s.Id))
                    .ToListAsync();

                model.Trainer.Services = services;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Eğitmeni buluyoruz
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) return NotFound();

            // Salon listesini dropdown için hazırlıyoruz
            ViewData["FitnessCenterId"] = new SelectList(_context.FitnessCenters, "Id", "Name", trainer.FitnessCenterId);

            // DİKKAT: Burası 'trainer' olmalı, 'TrainerViewModel' değil!
            return View(trainer);
        }

        // POST: Trainers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,SpecialtyText,FitnessCenterId,AvailableFrom,AvailableTo")] Trainer trainer)
        {
            if (id != trainer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Trainers.Any(e => e.Id == trainer.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FitnessCenterId"] = new SelectList(_context.FitnessCenters, "Id", "Name", trainer.FitnessCenterId);
            return View(trainer);
        }

        // GET: Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            return View(trainer);
        }

        // POST: Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Services)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            trainer.Services.Clear(); // FK HATASINI ENGELLE
            _context.Trainers.Remove(trainer);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}
