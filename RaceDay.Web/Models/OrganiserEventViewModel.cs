using System.ComponentModel.DataAnnotations;
namespace RaceDay.Web.Models;
public class OrganiserEventViewModel
{
 public int EventId { get; set; }
 [Required, StringLength(150)] public string Name { get; set; } = "";
 public string? Description { get; set; }
 [Required] public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);
 [Required] public string Location { get; set; } = "";
 [Range(0.1, 9999)] public decimal Distance { get; set; }
 [Required] public string EventType { get; set; } = "run";
 public string? BannerImageUrl { get; set; }
}