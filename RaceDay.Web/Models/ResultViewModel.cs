namespace RaceDay.Web.Models;

public class ResultViewModel
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public int CategoryId { get; set; }
    public string ParticipantName { get; set; } = "";
}

public class CreateResultViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }
}

public class EditResultViewModel
{
    public int ResultId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }
}