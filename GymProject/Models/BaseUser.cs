namespace GymProject.Models;

public abstract partial class BaseUser
{
    public string UserId { get; set; } = null!;

    public string? UserName { get; set; }

    public string? UserPassword { get; set; }
}

