using FoodSafety.Domain;
using FoodSafety.Domain.Entities;
using FoodSafety.MVC.Data;
using FoodSafety.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafety.MVC.Controllers
{
    [Authorize(Roles = "Admin,Inspector")]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpsController> _logger;

        public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: FollowUps
        // FollowUpsController.cs - Index Method
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Accessing Follow-Up Index");

            var followUps = await _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                .Select(f => new FollowUpIndexViewModel
                {
                    Id = f.Id,
                    
                    PremisesName = f.Inspection != null && f.Inspection.Premises != null
                                   ? f.Inspection.Premises.Name
                                   : "Pending",

                    DueDate = f.DueDate,
                    Status = f.Status ?? "Open",
                    ClosedDate = f.ClosedDate,
                    InspectionId = f.InspectionId
                })
                .Take(10)
                .OrderByDescending(f => f.DueDate)
                .ToListAsync();

            return View(followUps);
        }

        // POST: FollowUps/Close/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Inspector,Admin")]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var followUp = await _context.FollowUps.FindAsync(id);
                if (followUp == null)
                {
                    // LOG EVENT: Warning - Attempt to close non-existent record
                    _logger.LogWarning("Failed to close Follow-up ID {Id}: Record not found.", id);
                    return NotFound();
                }

                if (followUp.Status == "Closed")
                {
                    _logger.LogWarning("Follow-up ID {Id} is already closed.", id);
                    return BadRequest("Already closed.");
                }

                // Update status
                followUp.Status = "Closed";
                followUp.ClosedDate = DateTime.Now;

                _context.Update(followUp);
                await _context.SaveChangesAsync();

                // LOG EVENT: Information - Audit trail of closure
                _logger.LogInformation("Follow-up ID {Id} successfully CLOSED by user {User}",
                    id, User.Identity?.Name);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // LOG EVENT: Error - Exception handling
                _logger.LogError(ex, "An error occurred while closing Follow-up ID {Id}", id);
                return View("Error");
            }
        }

        // GET: FollowUps/Create
        public IActionResult Create()
        {
            // Populate dropdown with Inspection details (Premises Name + Date)
            var inspections = _context.Inspections
                .Include(i => i.Premises)
                .Select(i => new {
                    Id = i.Id,
                    DisplayName = $"{(i.Premises != null ? i.Premises.Name : "Unknown")} - {i.InspectionDate:dd/MM/yyyy}"
                })
                .ToList();

            ViewBag.InspectionId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(inspections, "Id", "DisplayName");
            return View();
        }

        // POST: FollowUps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InspectionId,DueDate,Status")] FollowUp followUp)
        {
            var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
            if (inspection != null && followUp.DueDate < inspection.InspectionDate)
            {
                // LOG EVENT: Warning - Business Rule
                _logger.LogWarning("Validation Issue: Follow-up DueDate {Due} is before InspectionDate {Insp} for Inspection {Id}",
                    followUp.DueDate, inspection.InspectionDate, followUp.InspectionId);

                ModelState.AddModelError("DueDate", "La fecha de vencimiento no puede ser anterior a la fecha de la inspección.");
            }

            if (ModelState.IsValid)
            {
                try 
                {
                
                _context.Add(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Audit Trail: User {User} created a new Follow-up for Inspection {InspId}",
                    User.Identity?.Name, followUp.InspectionId);

                return RedirectToAction(nameof(Index));
            }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database Error: Could not create Follow-up for Inspection {Id}", followUp.InspectionId);

                    ModelState.AddModelError("", "Error saving data.");
                }
            }

            var inspections = _context.Inspections
        .Include(i => i.Premises)
        .Select(i => new {
            Id = i.Id,
            DisplayName = $"{(i.Premises != null ? i.Premises.Name : "Unknown")} - {i.InspectionDate:dd/MM/yyyy}"
        }).ToList();
            ViewBag.InspectionId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(inspections, "Id", "DisplayName");

            return View(followUp);
        }

        // GET: FollowUps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        // GET: FollowUps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        // POST: FollowUps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Audit Trail: Follow-up ID {Id} deleted by {User}", id, User.Identity?.Name);
            }

            return RedirectToAction(nameof(Index));
        }

        
    }
}
