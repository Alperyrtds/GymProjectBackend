using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymProject.Models;

public partial class Customer : BaseUser
{
    public string? CustomerName { get; set; }

    public string? CustomerSurname { get; set; }

    public string? CustomerIdentityNumber { get; set; }

    public string? CustomerPhoneNumber { get; set; }

    public string? CustomerEmail { get; set; }

    public bool IsPasswordChanged { get; set; } = false;

    [NotMapped]
    public string? CustomerRegistryDateLong { get; set; }

    [NotMapped]
    public DateTime? CustomerRegistryStartDate { get; set; }
    [NotMapped]
    public DateTime? CustomerRegistryEndDate { get; set; }
}
