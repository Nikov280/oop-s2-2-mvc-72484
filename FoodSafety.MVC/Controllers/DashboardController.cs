using FoodSafety.Domain.Entities;
using FoodSafety.MVC.Data;
using FoodSafety.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static FoodSafety.Domain.Enums;

namespace FoodSafety.MVC.Controllers
{
        
    /// Restricts access to Admin, Inspector, and Viewer (Manager) roles.    
    [Authorize(Roles = "Admin,Inspector,Viewer")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        /// Calculates summary statistics filtered by Town and Risk Rating.        
        public async Task<IActionResult> Index(string town, RiskRating? risk)
        {
            try
            {
                
                _logger.LogInformation("Dashboard accessed by {User}. Filters applied - Town: {Town}, Risk: {Risk}",
                    User.Identity?.Name ?? "Anonymous", town ?? "All", risk?.ToString() ?? "Any");

                var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var now = DateTime.Now;

                // --- STEP 1: Filter Inspections ---
                var inspectionsQuery = _context.Inspections
                    .Include(i => i.Premises)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(town))
                {
                    inspectionsQuery = inspectionsQuery.Where(i => i.Premises != null && i.Premises.Town == town);
                }

                if (risk.HasValue)
                {
                    inspectionsQuery = inspectionsQuery.Where(i => i.Premises != null && i.Premises.RiskRating == risk.Value);
                }

                // --- STEP 2: Filter Follow-ups (It respects Town and Risk filters) ---
                var followUpsQuery = _context.FollowUps
                    .Include(f => f.Inspection)
                        .ThenInclude(i => i.Premises)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(town))
                {
                    followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.Town == town);
                }

                if (risk.HasValue)
                {
                    followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.RiskRating == risk.Value);
                }

                // --- STEP 3: Populate ViewModel with Aggregated Data ---
                var model = new DashboardViewModel
                {
                    // Statistics filtered by the active selection
                    MonthlyInspections = await inspectionsQuery
                        .CountAsync(i => i.InspectionDate >= firstDayOfMonth),

                    MonthlyFailures = await inspectionsQuery
                        .CountAsync(i => i.InspectionDate >= firstDayOfMonth && i.Outcome == Outcome.Fail),

                    OverdueFollowUps = await followUpsQuery
                        .CountAsync(f => f.Status == "Open" && f.DueDate < now),

                    // Filter State
                    SelectedTown = town,
                    SelectedRisk = risk,

                    // Populate Town dropdown list
                    Towns = await _context.Premises
                        .Select(p => p.Town)
                        .Distinct()
                        .OrderBy(t => t)
                        .ToListAsync()
                };

                // Warning log for significant backlog
                if (model.OverdueFollowUps > 5)
                {
                    _logger.LogWarning("Backlog Alert: {Count} overdue follow-ups detected for the current filter.",
                        model.OverdueFollowUps);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // Critical failure logging
                _logger.LogError(ex, "Critical failure generating Dashboard for user {User}",
                    User.Identity?.Name ?? "Anonymous");

                return RedirectToAction("Error", "Home");
            }
        }
    }
}