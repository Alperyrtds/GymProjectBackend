namespace GymProject.Models.Requests;

public class WorkoutLogRequest
{
    public string? WorkoutLogId { get; set; }

    public string? MovementId { get; set; }
}

public class MovementItem
{
    public string? MovementId { get; set; }
    public string? MovementName { get; set; }
    public decimal? Weight { get; set; }
    public int? SetCount { get; set; }
    public int? Reps { get; set; }
    public string? Notes { get; set; } // Bu hareket için özel notlar
}

public class AddWorkoutSessionRequest
{
    public DateTime? WorkoutDate { get; set; }
    public int? TotalDuration { get; set; } // Dakika cinsinden
    public string? Notes { get; set; } // Antrenman genel notları
    public List<MovementItem> Movements { get; set; } = new List<MovementItem>();
}

public class GetWorkoutSessionDetailRequest
{
    public string WorkoutSessionId { get; set; } = null!;
}

