namespace RaceDay.Web.Models;

public class CategoryViewModel
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? AgeRange { get; set; }
    public decimal? Distance { get; set; }
}

public class EnrolmentViewModel
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "";
}

public class CreateEnrolmentViewModel
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
}