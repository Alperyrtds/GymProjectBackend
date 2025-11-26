namespace GymProject.Models;

public partial class ProgramMovement
{
    public string ProgramMovementId { get; set; } = null!;

    public string? CustomerProgramId { get; set; }

    public string? MovementId { get; set; }

    public string? MovementName { get; set; }

    public int? SetCount { get; set; }

    public int? Reps { get; set; }

    public int? Order { get; set; } // Hareket sırası
}

