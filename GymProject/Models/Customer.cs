using System.ComponentModel.DataAnnotations.Schema;

namespace GymProject.Models;

public partial class Customer
{
    public string CustomerId { get; set; } = null!;

    public string? CustomerName { get; set; }

    public string? CustomerSurname { get; set; }

    public string? CustomerIdentityNumber { get; set; }

    public string? CustomerPhoneNumber { get; set; }

    public string? CustomerEmail { get; set; }
    [NotMapped]
    public string? CustomerRegistryDateLong { get; set; }
}
