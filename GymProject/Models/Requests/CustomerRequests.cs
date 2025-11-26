namespace GymProject.Models.Requests;

public class CustomerRequest
{
    public string UserId { get; set; } = null!;
}

public class AddCustomerRequest
{
    public string? CustomerName { get; set; }
    public string? CustomerSurname { get; set; }
    public string? CustomerIdentityNumber { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerRegistryDateLong { get; set; } // Ay cinsinden üyelik süresi
}

public class UpdateCustomerRequest
{
    public string UserId { get; set; } = null!; // Güncellenecek müşterinin ID'si
    public string? CustomerName { get; set; }
    public string? CustomerSurname { get; set; }
    public string? CustomerIdentityNumber { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerRegistryDateLong { get; set; } // Ay cinsinden üyelik süresi
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}

