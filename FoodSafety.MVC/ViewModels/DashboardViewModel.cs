using static FoodSafety.Domain.Enums;

namespace FoodSafety.MVC.ViewModels
{    
    /// ViewModel used to transport aggregated data to the Dashboard View.
    /// It encapsulates statistics for inspections and follow-ups.    
    public class DashboardViewModel
    {
        // Total number of inspections performed in the current month
        public int MonthlyInspections { get; set; }

        // Total number of failed inspections in the current month
        public int MonthlyFailures { get; set; }

        // Number of open follow-ups where the due date has passed
        public int OverdueFollowUps { get; set; }

        // Holds the currently selected town for filtering purposes
        public string? SelectedTown { get; set; }

        
        public RiskRating? SelectedRisk { get; set; }

        public List<string> Towns { get; set; } = new List<string>();
    }
}