namespace GymProject.Models;

public partial class WorkoutSession
{
    public string WorkoutSessionId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public DateTime? WorkoutDate { get; set; }

    public int? TotalDuration { get; set; } // Dakika cinsinden toplam antrenman süresi

    public string? Notes { get; set; } // Antrenman genel notları

    public string? TrainerId { get; set; } // Eğer antrenör yazdıysa
}

