namespace GymProject.Models;

public partial class WorkoutLog
{
    public string WorkoutLogId { get; set; } = null!;

    public string? WorkoutSessionId { get; set; } // Antrenman oturumu ID'si

    public string? CustomerId { get; set; } // Backward compatibility için

    public string? MovementId { get; set; }

    public string? MovementName { get; set; }

    public decimal? Weight { get; set; }

    public int? SetCount { get; set; }

    public int? Reps { get; set; }

    public DateTime? WorkoutDate { get; set; } // Backward compatibility için

    public int? WorkoutDuration { get; set; } // Backward compatibility için (artık kullanılmayacak)

    public string? Notes { get; set; } // Bu hareket için özel notlar

    public string? TrainerId { get; set; } // Backward compatibility için
}

