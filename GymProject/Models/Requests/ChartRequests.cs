namespace GymProject.Models.Requests;

public class WorkoutFrequencyRequest
{
    public string? Period { get; set; } // "weekly" veya "monthly"
}

public class PerformanceChartRequest
{
    public string? MovementId { get; set; }
    public string? MovementName { get; set; }
}

