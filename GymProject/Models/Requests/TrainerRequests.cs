namespace GymProject.Models.Requests;

public class TrainerRequest
{
    public string? UserId { get; set; } // Update ve Get işlemleri için

    public string? UserName { get; set; } // Update işlemi için

    // UserPassword artık otomatik oluşturulacak, request'te göndermeye gerek yok
    // Update işleminde şifre değiştirmek için kullanılabilir
    public string? UserPassword { get; set; } // Update işlemi için

    public string? TrainerName { get; set; }

    public string? TrainerSurname { get; set; }

    public string? TrainerPhoneNumber { get; set; }

    public string? TrainerEmail { get; set; } // Add işlemi için email (userName olarak kullanılacak)
}

public class GetTrainerByIdRequest
{
    public string UserId { get; set; } = null!;
}

