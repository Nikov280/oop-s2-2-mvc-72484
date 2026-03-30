using FoodSafety.Domain;
using FoodSafety.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodSafety.Domain.Entities;


namespace FoodSafety.MVC.Controllers
{
    [Authorize(Roles = "Admin,Inspector")]
    public class InspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InspectionsController> _logger;

        public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Inspections
        [Authorize(Roles = "Inspector,Admin,Viewer")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Inspections.Include(i => i.Premises);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Inspections/Details/5
        [Authorize(Roles = "Inspector,Admin,Viewer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspection == null)
            {
                return NotFound();
            }

            return View(inspection);
        }

        // GET: Inspections/Create 
        public IActionResult Create()
        {
            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name");
            return View();
        }

        // POST: Inspections/Create 
        [HttpPost]
        [Authorize(Roles = "Inspector,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            using (Serilog.Context.LogContext.PushProperty("UserName", User.Identity?.Name))
            {

                if (inspection.InspectionDate > DateTime.Now)
                {
                    _logger.LogWarning("Validation failure: Attempted to create inspection with future date {Date} for Premises {PremisesId}",
                inspection.InspectionDate, inspection.PremisesId);

                    ModelState.AddModelError("InspectionDate", "The inspection date cannot be in the future..");
                }

                if (ModelState.IsValid)
                {
                    _context.Add(inspection);
                    await _context.SaveChangesAsync();


                    _logger.LogInformation("Inspection {InspId} created for Premises {PremId} by {User}",
            inspection.Id, inspection.PremisesId, User.Identity.Name);

                    return RedirectToAction(nameof(Index));
                }

                ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
                return View(inspection);
            }
        }

        // GET: Inspections/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null)
            {
                return NotFound();
            }
            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Address", inspection.PremisesId);
            return View(inspection);
        }

        // POST: Inspections/Edit/5              
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            if (id != inspection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspection);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Inspection {Id} updated by {User}", id, User.Identity?.Name);

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error updating Inspection {Id}", id);
                    throw;
                }
                
            }
            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Address", inspection.PremisesId);
            return View(inspection);
        }

        // GET: Inspections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspection == null)
            {
                return NotFound();
            }

            return View(inspection);
        }

        // POST: Inspections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
            {
                _context.Inspections.Remove(inspection);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.Id == id);
        }
    }
}
