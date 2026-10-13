namespace RaceDay.Api.Models;

public class Profile
{
    public int ProfileId { get; set; }
    public int UserId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MedicalConditions { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public User User { get; set; } = null!;
}
