using static FoodSafety.Domain.Enums;

namespace FoodSafety.Domain.Entities
{
    public class FollowUp
    {
        public int Id { get; set; }
        public int InspectionId { get; set; }
        public virtual Inspection? Inspection { get; set; }

        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Open"; // Default to Open when created
        public DateTime? ClosedDate { get; set; }
    }
}
