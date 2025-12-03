namespace GymProject.Models;

public class PushToken
{
    public string Id { get; set; } = null!;
    public string CustomerId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string? Platform { get; set; } // 'ios', 'android', 'expo'
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool IsActive { get; set; }
}

