namespace GymProject.Models;

public partial class Movement
{
    public string MovementId { get; set; } = null!;

    public string? MovementName { get; set; }

    public string? MovementDescription { get; set; }

    public string? MovementVideoUrl { get; set; }
}

