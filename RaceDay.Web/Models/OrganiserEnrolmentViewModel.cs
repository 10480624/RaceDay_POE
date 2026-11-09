namespace RaceDay.Web.Models;

public class OrganiserEnrolmentViewModel
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "";
    public int? ResultId { get; set; }
}

public class ManageEnrolmentsViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public List<OrganiserEnrolmentViewModel> Enrolments { get; set; } = new();
}