namespace GymProject.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string? UserName { get; set; }

    public string? UserPassword { get; set; }

    public int? CustomerBool { get; set; }

    public int? AdministratorBool { get; set; }

    public string? CustomerId { get; set; }

    public string? AdminastorId { get; set; }
}
