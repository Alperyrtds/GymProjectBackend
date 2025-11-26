namespace GymProject.Models;

public partial class Trainer : BaseUser
{
    public string? TrainerName { get; set; }

    public string? TrainerSurname { get; set; }

    public string? TrainerPhoneNumber { get; set; }

    public string? TrainerEmail { get; set; }

    public bool IsPasswordChanged { get; set; } = false;
}

