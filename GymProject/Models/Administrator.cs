namespace GymProject.Models;

public partial class Administrator : BaseUser
{
    public string? AdministratorName { get; set; }

    public string? AdministratorSurname { get; set; }

    public bool IsPasswordChanged { get; set; } = false;
}
