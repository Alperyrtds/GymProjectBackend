namespace GymProject.Models.Requests;

public class CustomerProgramRequest
{
    public string CustomerId { get; set; } = null!;
}

public class DeleteProgramRequest
{
    public string ProgramId { get; set; } = null!;
}

public class ProgramMovementItem
{
    public string? MovementId { get; set; }
    public string? MovementName { get; set; }
    public int? SetCount { get; set; }
    public int? Reps { get; set; }
}

public class AddProgramRequest
{
    public string? CustomerId { get; set; }
    public string? ProgramName { get; set; }
    public DateTime? ProgramStartDate { get; set; }
    public DateTime? ProgramEndDate { get; set; }
    public int LeftValidity { get; set; }
    public List<ProgramMovementItem> Movements { get; set; } = new List<ProgramMovementItem>();
    
    // Backward compatibility için (eski yöntem)
    public string? MovementId { get; set; }
    public string? MovementName { get; set; }
    public int? SetCount { get; set; }
    public int? Reps { get; set; }
}

public class GetProgramDetailRequest
{
    public string ProgramId { get; set; } = null!;
}

public class UpdateProgramRequest
{
    public string ProgramId { get; set; } = null!;
    public string? ProgramName { get; set; }
    public DateTime? ProgramStartDate { get; set; }
    public DateTime? ProgramEndDate { get; set; }
    public int LeftValidity { get; set; }
    public List<ProgramMovementItem>? Movements { get; set; } // null ise güncellenmez
}

