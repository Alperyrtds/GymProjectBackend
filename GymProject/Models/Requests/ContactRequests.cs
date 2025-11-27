namespace GymProject.Models.Requests;

public class AddContactMessageRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Message { get; set; }
}

public class UpdateContactMessageRequest
{
    public string ContactMessageId { get; set; } = null!;
    public bool? IsRead { get; set; }
}

public class DeleteContactMessageRequest
{
    public string ContactMessageId { get; set; } = null!;
}

public class GetContactMessageRequest
{
    public string ContactMessageId { get; set; } = null!;
}

