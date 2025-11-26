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
    public class AdministratorController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public AdministratorController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Admin yeni admin ekleyebilir
        /// </summary>
        [HttpPost]
        [Route("AddAdministrator")]
        public async Task<ApiResponse> AddAdministrator([FromBody] AdministratorRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.AdministratorEmail))
                {
                    return new ApiResponse("Error", "E-posta adresi boş olamaz.", null);
                }

                if (string.IsNullOrWhiteSpace(model.AdministratorName))
                {
                    return new ApiResponse("Error", "Admin adı boş olamaz.", null);
                }

                if (string.IsNullOrWhiteSpace(model.AdministratorSurname))
                {
                    return new ApiResponse("Error", "Admin soyadı boş olamaz.", null);
                }

                // Email kontrolü (userName olarak kullanılacak)
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(x => x.UserName == model.AdministratorEmail);

                if (existingUser != null)
                {
                    return new ApiResponse("Error", "Bu e-posta adresi zaten kullanılıyor.", null);
                }

                // Otomatik şifre oluştur - Ad.Soyad formatında (admin'in hatırlayabileceği)
                var temporaryPassword = CustomerService.GenerateCustomerPassword(
                    model.AdministratorName, 
                    model.AdministratorSurname);
                var hashedPassword = NulidGenarator.GenerateSHA512String(temporaryPassword);

                var administrator = new Administrator
                {
                    UserId = NulidGenarator.Id(),
                    UserName = model.AdministratorEmail, // Email userName olarak kullanılıyor
                    UserPassword = hashedPassword,
                    AdministratorName = model.AdministratorName,
                    AdministratorSurname = model.AdministratorSurname,
                    IsPasswordChanged = false // İlk girişte şifre değiştirmesi gerekecek
                };

                await _dbContext.Administrators.AddAsync(administrator);
                await _dbContext.SaveChangesAsync();

                // Response'a geçici şifreyi ekle (sadece bu seferlik gösterilecek)
                var responseData = new
                {
                    administrator,
                    temporaryPassword = temporaryPassword // Geçici şifre - sadece bu response'da gösterilir
                };

                return new ApiResponse("Success", $"Admin başarıyla eklendi. Geçici şifre: {temporaryPassword}", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin tüm adminleri görebilir
        /// </summary>
        [HttpGet]
        [Route("GetAllAdministrators")]
        public async Task<ApiResponse> GetAllAdministrators()
        {
            try
            {
                var administrators = await _dbContext.Administrators
                    .OrderBy(x => x.AdministratorName)
                    .ThenBy(x => x.AdministratorSurname)
                    .ToListAsync();

                return new ApiResponse("Success", "Adminler başarıyla getirildi.", administrators);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin bir adminin detayını görebilir
        /// </summary>
        [HttpPost]
        [Route("GetAdministratorById")]
        public async Task<ApiResponse> GetAdministratorById([FromBody] AdministratorRequest request)
        {
            try
            {
                var administrator = await _dbContext.Administrators
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId);

                if (administrator == null)
                {
                    return new ApiResponse("Error", "Admin bulunamadı.", null);
                }

                return new ApiResponse("Success", "Admin başarıyla getirildi.", administrator);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin bir admini güncelleyebilir
        /// </summary>
        [HttpPost]
        [Route("UpdateAdministrator")]
        public async Task<ApiResponse> UpdateAdministrator(AdministratorRequest model)
        {
            try
            {
                var administrator = await _dbContext.Administrators
                    .FirstOrDefaultAsync(x => x.UserId == model.UserId);

                if (administrator == null)
                {
                    return new ApiResponse("Error", "Admin bulunamadı.", null);
                }

                // Kullanıcı adı değişiyorsa kontrol et
                if (!string.IsNullOrWhiteSpace(model.UserName) && administrator.UserName != model.UserName)
                {
                    var existingUser = await _dbContext.Users
                        .FirstOrDefaultAsync(x => x.UserName == model.UserName && x.UserId != model.UserId);

                    if (existingUser != null)
                    {
                        return new ApiResponse("Error", "Bu kullanıcı adı zaten kullanılıyor.", null);
                    }

                    administrator.UserName = model.UserName;
                }

                if (!string.IsNullOrWhiteSpace(model.UserPassword))
                {
                    administrator.UserPassword = NulidGenarator.GenerateSHA512String(model.UserPassword);
                }

                if (!string.IsNullOrWhiteSpace(model.AdministratorName))
                    administrator.AdministratorName = model.AdministratorName;
                
                if (!string.IsNullOrWhiteSpace(model.AdministratorSurname))
                    administrator.AdministratorSurname = model.AdministratorSurname;

                // TPT inheritance'da Update() yerine direkt SaveChanges kullan
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Admin başarıyla güncellendi.", administrator);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin bir admini silebilir
        /// </summary>
        [HttpPost]
        [Route("DeleteAdministrator")]
        public async Task<ApiResponse> DeleteAdministrator([FromBody] AdministratorRequest request)
        {
            try
            {
                var administrator = await _dbContext.Administrators
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId);

                if (administrator == null)
                {
                    return new ApiResponse("Error", "Admin bulunamadı.", null);
                }

                // Kendini silmesini engelle
                var currentUserId = User.FindFirstValue(ClaimTypes.Sid);
                if (administrator.UserId == currentUserId)
                {
                    return new ApiResponse("Error", "Kendi hesabınızı silemezsiniz.", null);
                }

                _dbContext.Administrators.Remove(administrator);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Admin başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin kendi şifresini değiştirebilir
        /// </summary>
        [HttpPost]
        [Route("ChangePassword")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var administrator = await _dbContext.Administrators
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (administrator == null)
                {
                    return new ApiResponse("Error", "Admin bulunamadı.", null);
                }

                // Mevcut şifre kontrolü
                var currentPasswordHash = NulidGenarator.GenerateSHA512String(model.CurrentPassword);
                if (administrator.UserPassword != currentPasswordHash)
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
                administrator.UserPassword = NulidGenarator.GenerateSHA512String(model.NewPassword);
                administrator.IsPasswordChanged = true; // Şifre değiştirildi olarak işaretle

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

