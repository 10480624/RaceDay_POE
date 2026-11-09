using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public class ProfileViewModel
{
    public int UserId { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required, StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";
    public string Role { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(100)]
    [Display(Name = "Emergency contact name")]
    public string? EmergencyContactName { get; set; }

    [StringLength(20)]
    [Display(Name = "Emergency contact phone")]
    public string? EmergencyContactPhone { get; set; }

    [StringLength(500)]
    [Display(Name = "Medical conditions")]
    public string? MedicalConditions { get; set; }

    [StringLength(500), Url]
    [Display(Name = "Profile picture URL")]
    public string? ProfilePictureUrl { get; set; }
}
