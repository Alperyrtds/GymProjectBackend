namespace GymProject.Models.Requests;

public class AdministratorRequest
{
    public string? UserId { get; set; } // Update ve Get işlemleri için

    public string? UserName { get; set; } // Update işlemi için

    // UserPassword artık otomatik oluşturulacak, request'te göndermeye gerek yok
    // Update işleminde şifre değiştirmek için kullanılabilir
    public string? UserPassword { get; set; } // Update işlemi için

    public string? AdministratorName { get; set; }

    public string? AdministratorSurname { get; set; }

    public string? AdministratorEmail { get; set; } // Add işlemi için email
}

