namespace GymProject.Models;

public partial class CustomersRegistration
{
    public string CustomerRegistrationId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? PricingPlanId { get; set; } // Hangi plan seçildi

    public DateTime? CustomerRegistrationStartDate { get; set; }

    public DateTime? CustomerRegistrationFinishDate { get; set; }
}
