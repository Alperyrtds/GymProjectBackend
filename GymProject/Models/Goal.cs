namespace GymProject.Models;

public partial class Goal
{
    public string GoalId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? GoalType { get; set; } // "Weight", "Measurement", "Exercise" vb.

    public string? GoalName { get; set; } // "Kilo verme", "Göğüs ölçüsü artırma" vb.

    public decimal? TargetValue { get; set; } // Hedef değer (kilo, ölçü vb.)

    public decimal? CurrentValue { get; set; } // Mevcut değer

    public DateTime? TargetDate { get; set; } // Hedef tarih

    public DateTime? StartDate { get; set; } // Başlangıç tarihi

    public bool IsCompleted { get; set; } = false; // Tamamlandı mı?

    public DateTime? CompletedDate { get; set; } // Tamamlanma tarihi

    public string? Notes { get; set; } // Notlar

    public string? TrainerId { get; set; } // Eğer antrenör belirlediyse
}

