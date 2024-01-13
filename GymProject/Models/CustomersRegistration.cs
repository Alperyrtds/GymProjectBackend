using System;
using System.Collections.Generic;

namespace GymProject.Models;

public partial class CustomersRegistration
{
    public string CustomerRegistrationId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public DateTime? CustomerRegistrationStartDate { get; set; }

    public DateTime? CustomerRegistrationFinishDate { get; set; }
}
