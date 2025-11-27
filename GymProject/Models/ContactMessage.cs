namespace GymProject.Models;

public partial class ContactMessage
{
    public string ContactMessageId { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool IsRead { get; set; } = false;
}

