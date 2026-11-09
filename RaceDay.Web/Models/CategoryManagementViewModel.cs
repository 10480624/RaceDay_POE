using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public class CategoryManagementViewModel
{
    public int EventId { get; set; }

    public string EventName { get; set; } = "";

    public List<CategoryViewModel> Categories { get; set; } = new();

    public CreateCategoryViewModel NewCategory { get; set; } = new();
}

public class CreateCategoryViewModel
{
    public int EventId { get; set; }

    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = "";

    [StringLength(100)]
    public string? AgeRange { get; set; }

    [Range(0.1, 9999)]
    public decimal? Distance { get; set; }
}