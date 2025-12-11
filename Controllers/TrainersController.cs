using System;
using System.Collections.Generic;
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
            var fitnessDbContext = _context.Trainers.Include(t => t.FitnessCenter);
            return View(await fitnessDbContext.ToListAsync());
        }

        // GET: Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

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

            ViewData["FitnessCenterId"] = new SelectList(_context.FitnessCenters, "Id", "Name");

            return View(model);
        }

        // POST: Trainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Eğitmeni kaydet
                _context.Add(model.Trainer);
                await _context.SaveChangesAsync();

                // Seçilen hizmetleri ilişkilendir
                if (model.SelectedServiceIds != null)
                {
                    var selectedServices = await _context.Services
                        .Where(s => model.SelectedServiceIds.Contains(s.Id))
                        .ToListAsync();

                    model.Trainer.Services = selectedServices;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // VALIDATION HATASI OLURSA SERVİSLERİ YENİDEN DOLDURMALISIN!
            model.Services = _context.Services
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.ServiceName
                }).ToList();

            ViewData["FitnessCenterId"] = new SelectList(_context.FitnessCenters, "Id", "Name", model.Trainer.FitnessCenterId);

            return View(model);
        }

        // GET: Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound();
            }
            ViewData["FitnessCenterId"] = new SelectList(_context.FitnessCenters, "Id", "Address", trainer.FitnessCenterId);
            return View(trainer);
        }

        // POST: Trainers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,SpecialtyText,FitnessCenterId")] Trainer trainer)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FitnessCenterId"] = new SelectList(_context.FitnessCenters, "Id", "Address", trainer.FitnessCenterId);
            return View(trainer);
        }

        // GET: Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // POST: Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}
