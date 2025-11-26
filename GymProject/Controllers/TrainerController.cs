using System.Security.Claims;
using GymProject.Helpers;
using GymProject.Models;
using GymProject.Models.Requests;
using GymProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TrainerController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public TrainerController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Admin yeni antrenör ekleyebilir
        /// </summary>
        [HttpPost]
        [Route("AddTrainer")]
        public async Task<ApiResponse> AddTrainer([FromBody] TrainerRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TrainerEmail))
                {
                    return new ApiResponse("Error", "E-posta adresi boş olamaz.", null);
                }

                if (string.IsNullOrWhiteSpace(model.TrainerName))
                {
                    return new ApiResponse("Error", "Antrenör adı boş olamaz.", null);
                }

                if (string.IsNullOrWhiteSpace(model.TrainerSurname))
                {
                    return new ApiResponse("Error", "Antrenör soyadı boş olamaz.", null);
                }

                // Email kontrolü (userName olarak kullanılacak)
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(x => x.UserName == model.TrainerEmail);

                if (existingUser != null)
                {
                    return new ApiResponse("Error", "Bu e-posta adresi zaten kullanılıyor.", null);
                }

                // Otomatik şifre oluştur - Ad.Soyad formatında (admin'in hatırlayabileceği)
                var temporaryPassword = CustomerService.GenerateCustomerPassword(
                    model.TrainerName, 
                    model.TrainerSurname);
                var hashedPassword = NulidGenarator.GenerateSHA512String(temporaryPassword);

                var trainer = new Trainer
                {
                    UserId = NulidGenarator.Id(),
                    UserName = model.TrainerEmail, // Email userName olarak kullanılıyor
                    UserPassword = hashedPassword,
                    TrainerName = model.TrainerName,
                    TrainerSurname = model.TrainerSurname,
                    TrainerPhoneNumber = model.TrainerPhoneNumber,
                    TrainerEmail = model.TrainerEmail,
                    IsPasswordChanged = false // İlk girişte şifre değiştirmesi gerekecek
                };

                await _dbContext.Trainers.AddAsync(trainer);
                await _dbContext.SaveChangesAsync();

                // Response'a geçici şifreyi ekle (sadece bu seferlik gösterilecek)
                var responseData = new
                {
                    trainer,
                    temporaryPassword = temporaryPassword // Geçici şifre - sadece bu response'da gösterilir
                };

                return new ApiResponse("Success", $"Antrenör başarıyla eklendi. Geçici şifre: {temporaryPassword}", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin tüm antrenörleri görebilir
        /// </summary>
        [HttpGet]
        [Route("GetAllTrainers")]
        public async Task<ApiResponse> GetAllTrainers()
        {
            try
            {
                var trainers = await _dbContext.Trainers
                    .OrderBy(x => x.TrainerName)
                    .ThenBy(x => x.TrainerSurname)
                    .ToListAsync();

                return new ApiResponse("Success", "Antrenörler başarıyla getirildi.", trainers);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin bir antrenörün detayını görebilir
        /// </summary>
        [HttpPost]
        [Route("GetTrainerById")]
        public async Task<ApiResponse> GetTrainerById([FromBody] TrainerRequest request)
        {
            try
            {
                var trainer = await _dbContext.Trainers
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId);

                if (trainer == null)
                {
                    return new ApiResponse("Error", "Antrenör bulunamadı.", null);
                }

                return new ApiResponse("Success", "Antrenör başarıyla getirildi.", trainer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin bir antrenörü güncelleyebilir
        /// </summary>
        [HttpPost]
        [Route("UpdateTrainer")]
        public async Task<ApiResponse> UpdateTrainer(TrainerRequest model)
        {
            try
            {
                var trainer = await _dbContext.Trainers
                    .FirstOrDefaultAsync(x => x.UserId == model.UserId);

                if (trainer == null)
                {
                    return new ApiResponse("Error", "Antrenör bulunamadı.", null);
                }

                // Kullanıcı adı değişiyorsa kontrol et
                if (!string.IsNullOrWhiteSpace(model.UserName) && trainer.UserName != model.UserName)
                {
                    var existingUser = await _dbContext.Users
                        .FirstOrDefaultAsync(x => x.UserName == model.UserName && x.UserId != model.UserId);

                    if (existingUser != null)
                    {
                        return new ApiResponse("Error", "Bu kullanıcı adı zaten kullanılıyor.", null);
                    }

                    trainer.UserName = model.UserName;
                }

                if (!string.IsNullOrWhiteSpace(model.UserPassword))
                {
                    trainer.UserPassword = NulidGenarator.GenerateSHA512String(model.UserPassword);
                }

                if (!string.IsNullOrWhiteSpace(model.TrainerName))
                    trainer.TrainerName = model.TrainerName;
                
                if (!string.IsNullOrWhiteSpace(model.TrainerSurname))
                    trainer.TrainerSurname = model.TrainerSurname;
                
                if (!string.IsNullOrWhiteSpace(model.TrainerPhoneNumber))
                    trainer.TrainerPhoneNumber = model.TrainerPhoneNumber;
                
                if (!string.IsNullOrWhiteSpace(model.TrainerEmail))
                    trainer.TrainerEmail = model.TrainerEmail;

                // TPT inheritance'da Update() yerine direkt SaveChanges kullan
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Antrenör başarıyla güncellendi.", trainer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin bir antrenörü silebilir
        /// </summary>
        [HttpPost]
        [Route("DeleteTrainer")]
        public async Task<ApiResponse> DeleteTrainer([FromBody] TrainerRequest request)
        {
            try
            {
                var trainer = await _dbContext.Trainers
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId);

                if (trainer == null)
                {
                    return new ApiResponse("Error", "Antrenör bulunamadı.", null);
                }

                // Bu antrenörün yazdığı programlar var mı kontrol et
                var programsCount = await _dbContext.CustomersPrograms
                    .CountAsync(x => true); // Bu kontrolü daha detaylı yapabiliriz gerekirse

                // Şimdilik sadece silme işlemini yapıyoruz
                _dbContext.Trainers.Remove(trainer);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Antrenör başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Trainer kendi şifresini değiştirebilir
        /// </summary>
        [HttpPost]
        [Route("ChangePassword")]
        [Authorize(Roles = "Trainer")]
        public async Task<ApiResponse> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var trainer = await _dbContext.Trainers
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (trainer == null)
                {
                    return new ApiResponse("Error", "Antrenör bulunamadı.", null);
                }

                // Mevcut şifre kontrolü
                var currentPasswordHash = NulidGenarator.GenerateSHA512String(model.CurrentPassword);
                if (trainer.UserPassword != currentPasswordHash)
                {
                    return new ApiResponse("Error", "Mevcut şifre yanlış.", null);
                }

                // Yeni şifre ve onay şifresi kontrolü
                if (model.NewPassword != model.ConfirmPassword)
                {
                    return new ApiResponse("Error", "Yeni şifre ve onay şifresi eşleşmiyor.", null);
                }

                // Şifre validasyonu
                var (isValid, errorMessage) = CustomerService.ValidatePassword(model.NewPassword);
                if (!isValid)
                {
                    return new ApiResponse("Error", errorMessage ?? "Şifre geçersiz.", null);
                }

                // Şifreyi güncelle
                trainer.UserPassword = NulidGenarator.GenerateSHA512String(model.NewPassword);
                trainer.IsPasswordChanged = true; // Şifre değiştirildi olarak işaretle

                // TPT inheritance'da Update() yerine direkt SaveChanges kullan
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Şifre başarıyla değiştirildi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }
    }
}

