namespace GymProject.Models;

public partial class CustomersProgram
{
    public string CustomerProgramId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? MovementId { get; set; } // Backward compatibility için

    public string? MovementName { get; set; } // Backward compatibility için

    public string? ProgramName { get; set; }

    public string? CreatedByUserId { get; set; }

    public string? CreatedByName { get; set; }

    public int? SetCount { get; set; } // Backward compatibility için

    public int? Reps { get; set; } // Backward compatibility için

    public DateTime? ProgramStartDate { get; set; }

    public DateTime? ProgramEndDate { get; set; }

    public int LeftValidity { get; set; }
}
