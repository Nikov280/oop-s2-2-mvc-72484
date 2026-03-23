using FoodSafety.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using static FoodSafety.Domain.Enums;

namespace FoodSafety.Domain.Entities
{
    public class Inspection
    {
        public int Id { get; set; }
        public int PremisesId { get; set; }
        public virtual Premises? Premises { get; set; }

        [DataType(DataType.Date)]
        public DateTime InspectionDate { get; set; }

        [Range(0, 100)]
        public int Score { get; set; }
        public Outcome Outcome { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation Property: One Inspection has many Follow-ups
        public List<FollowUp> FollowUps { get; set; } = new();
    }
}
