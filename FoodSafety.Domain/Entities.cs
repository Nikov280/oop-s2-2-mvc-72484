using System.ComponentModel.DataAnnotations;
using static FoodSafety.Domain.Enums;

namespace FoodSafety.Domain.Entities;

public class Premises
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public RiskRating RiskRating { get; set; }

    // Navigation Property: One Premises has many Inspections
    public List<Inspection> Inspections { get; set; } = new();
}


