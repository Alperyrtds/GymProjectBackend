namespace GymProject.Models;

public partial class CustomersProgram
{
    public string CustomerProgramId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? MovementName { get; set; }

    public int? SetCount { get; set; }

    public int? Reps { get; set; }

    public DateTime? ProgramStartDate { get; set; }

    public DateTime? ProgramEndDate { get; set; }

    public int LeftValidity { get; set; }
}
