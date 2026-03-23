namespace FoodSafety.MVC.ViewModels
{
    // ViewModel to flatten the data for the Follow-up index table
    public class FollowUpIndexViewModel
    {
        public int Id { get; set; }
        public string PremisesName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Open"; // Open or Closed
        public DateTime? ClosedDate { get; set; }
        public bool IsOverdue => Status == "Open" && DueDate < DateTime.Today;
        public int InspectionId { get; set; }
    }
}