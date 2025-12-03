using System.Security.Claims;
using System.Transactions;
using GymProject.Helpers;
using GymProject.Models;
using GymProject.Models.Requests;
using GymProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymProject.Validators.CustomersValidators;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private AlperyurtdasGymProjectContext _dbContext;
        private readonly Services.PushTokenService _pushTokenService;

        public CustomerController(AlperyurtdasGymProjectContext dbContext, Services.PushTokenService pushTokenService)
        {
            _dbContext = dbContext;
            _pushTokenService = pushTokenService;
        }

        [HttpPost]
        [Route("AddCustomer")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> AddCustomer([FromBody] AddCustomerRequest model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var validator = new CustomerAddValidator();

                var result = await validator.ValidateAsync(model);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = Bir Hata Oluştu", result.Errors, null);
                }

                var userId = NulidGenarator.Id();
                // Otomatik şifre oluştur - Ad.Soyad formatında (admin'in hatırlayabileceği)
                var temporaryPassword = CustomerService.GenerateCustomerPassword(model.CustomerName ?? "", model.CustomerSurname ?? "");
                var hashedPassword = NulidGenarator.GenerateSHA512String(temporaryPassword);
                
                var customer = new Customer()
                {
                    UserId = userId,
                    UserName = model.CustomerEmail,
                    UserPassword = hashedPassword,
                    CustomerEmail = model.CustomerEmail,
                    CustomerIdentityNumber = model.CustomerIdentityNumber,
                    CustomerName = model.CustomerName,
                    CustomerPhoneNumber = model.CustomerPhoneNumber,
                    CustomerSurname = model.CustomerSurname,
                    IsPasswordChanged = false // İlk girişte şifre değiştirmesi gerekecek
                };

                await _dbContext.Customers.AddAsync(customer);
                await _dbContext.SaveChangesAsync();

                var startDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                var finishDate = DateTime.SpecifyKind(DateTime.Now.AddMonths(int.Parse(model.CustomerRegistryDateLong!)), DateTimeKind.Unspecified);
                
                var customerRegistration = new CustomersRegistration()
                {
                   CustomerRegistrationId = NulidGenarator.Id(),
                    CustomerId = customer.UserId, // Artık UserId kullanıyoruz
                    CustomerRegistrationStartDate = startDate,
                    CustomerRegistrationFinishDate = finishDate,
               
                };
                await _dbContext.CustomersRegistrations.AddAsync(customerRegistration);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                // Response'a geçici şifreyi ekle (sadece bu seferlik gösterilecek)
                var responseData = new
                {
                    customer,
                    temporaryPassword = temporaryPassword // Geçici şifre - sadece bu response'da gösterilir
                };

                return new ApiResponse("Success", $"Başarıyla Eklendi. Geçici şifre: {temporaryPassword}", responseData);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);

            }

        }

        [HttpPost]
        [Route("GetCustomerById")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetCustomer([FromBody]CustomerRequest model)
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                // Customer rolü ise sadece kendi bilgilerini görebilir
                if (userRole == "Customer" && userId != model.UserId)
                {
                    return new ApiResponse("Error", $"Yetkiniz yok. Sadece kendi bilgilerinizi görüntüleyebilirsiniz.", null);
                }

                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.UserId == model.UserId);

                return customer != null ? new ApiResponse("Success", $"Başarıyla Getirildi", customer) 
                    : new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }

        }

        [HttpGet]
        [Route("GetAllCustomers")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetAllCustomer()
        {
            try
            {
                var customerList = await _dbContext.Customers.ToListAsync();

                if (customerList != null && customerList.Count > 0)
                {
                    foreach (var item in customerList)
                    {
                        var registery =
                            await _dbContext.CustomersRegistrations.FirstOrDefaultAsync(
                                x => x.CustomerId == item.UserId);

                        if (registery != null)
                        {
                            item.CustomerRegistryStartDate = registery.CustomerRegistrationStartDate.Value;
                            item.CustomerRegistryEndDate = registery.CustomerRegistrationFinishDate.Value;

                            // Kaç gün kaldığını hesapla
                            var today = DateTime.Now.Date;
                            var finishDate = registery.CustomerRegistrationFinishDate.Value.Date;
                            var remainDate = (finishDate - today).Days;
                            if (remainDate < 0) remainDate = 0; // Eğer süresi dolmuşsa 0

                            item.CustomerRegistryDateLong = remainDate.ToString();
                        } 
                    }
                }
                
                return new ApiResponse("Success", $"Başarıyla Getirildi", customerList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        [HttpPost]
        [Route("UpdateCustomer")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> UpdateCustomer([FromBody] UpdateCustomerRequest model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                // Customer rolü ise sadece kendi bilgilerini güncelleyebilir
                if (userRole == "Customer" && userId != model.UserId)
                {
                    return new ApiResponse("Error", $"Yetkiniz yok. Sadece kendi bilgilerinizi güncelleyebilirsiniz.", null);
                }

                // Trainer rolü müşteri güncelleyemez
                if (userRole == "Trainer")
                {
                    return new ApiResponse("Error", $"Yetkiniz yok. Müşteri bilgilerini güncelleyemezsiniz.", null);
                }

                var validator = new CustomerUpdateValidator();

                var result = await validator.ValidateAsync(model);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = Bir Hata Oluştu", result.Errors, null);
                }

                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.UserId == model.UserId);

                if (customer == null)
                {
                    return new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);
                }

                // Email değiştiyse UserName'i de güncelle
                if (!string.IsNullOrWhiteSpace(model.CustomerEmail) && customer.CustomerEmail != model.CustomerEmail)
                {
                    // Yeni email'in başka bir kullanıcı tarafından kullanılıp kullanılmadığını kontrol et
                    var existingUser = await _dbContext.Users
                        .FirstOrDefaultAsync(x => x.UserName == model.CustomerEmail && x.UserId != model.UserId);
                    
                    if (existingUser != null)
                    {
                        return new ApiResponse("Error", "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.", null);
                    }
                    
                    customer.UserName = model.CustomerEmail;
                }

                // Sadece gönderilen alanları güncelle
                if (!string.IsNullOrWhiteSpace(model.CustomerName))
                    customer.CustomerName = model.CustomerName;
                
                if (!string.IsNullOrWhiteSpace(model.CustomerEmail))
                    customer.CustomerEmail = model.CustomerEmail;
                
                if (!string.IsNullOrWhiteSpace(model.CustomerPhoneNumber))
                    customer.CustomerPhoneNumber = model.CustomerPhoneNumber;
                
                if (!string.IsNullOrWhiteSpace(model.CustomerSurname))
                    customer.CustomerSurname = model.CustomerSurname;
                
                if (!string.IsNullOrWhiteSpace(model.CustomerIdentityNumber))
                    customer.CustomerIdentityNumber = model.CustomerIdentityNumber;
                
                if (!string.IsNullOrWhiteSpace(model.CustomerRegistryDateLong))
                    customer.CustomerRegistryDateLong = model.CustomerRegistryDateLong;

                _dbContext.Customers.Update(customer);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse("Success", $"Başarıyla Güncellendi", customer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);

            }

        }

        [HttpPost]
        [Route("DeleteCustomer")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> DeleteCustomer(CustomerRequest req)
        {
        
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var validator = new CustomerDeleteValidator();

                var result = await validator.ValidateAsync(req.UserId);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = Bir Hata Oluştu", result.Errors, null);
                }

                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.UserId == req.UserId);

                if (customer == null)
                {
                    return new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);
                }

                _dbContext.Customers.Remove(customer);
                await _dbContext.SaveChangesAsync();

                //var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.CustomerId == id);

                //_dbContext.Users.Remove(user);
                //await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse("Success", $"Başarıyla Silindi", customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }

        }


        /// <summary>
        /// Üye kendi profil bilgilerini görüntüleyebilir
        /// </summary>
        [HttpGet]
        [Route("GetMyProfile")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetMyProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                // Müşteri bilgilerini getir
                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.UserId == userId);

                if (customer == null)
                {
                    return new ApiResponse("Error", "Müşteri bulunamadı.", null);
                }

                // Üyelik kaydını getir
                var registration = await _dbContext.CustomersRegistrations
                    .Where(x => x.CustomerId == userId)
                    .OrderByDescending(x => x.CustomerRegistrationStartDate)
                    .FirstOrDefaultAsync();

                // Kalan gün hesapla
                int remainingDays = 0;
                string membershipStatus = "Aktif";
                
                if (registration?.CustomerRegistrationFinishDate.HasValue == true)
                {
                    var finishDate = registration.CustomerRegistrationFinishDate.Value.Date;
                    var today = DateTime.Now.Date;
                    var daysDifference = (finishDate - today).Days;

                    if (daysDifference > 0)
                    {
                        remainingDays = daysDifference;
                        membershipStatus = "Aktif";
                    }
                    else if (daysDifference == 0)
                    {
                        remainingDays = 0;
                        membershipStatus = "Bugün Bitiyor";
                    }
                    else
                    {
                        remainingDays = 0;
                        membershipStatus = "Süresi Dolmuş";
                    }
                }

                // Üyelik süresini hesapla (ay cinsinden)
                int membershipDurationMonths = 0;
                if (registration?.CustomerRegistrationStartDate.HasValue == true && 
                    registration?.CustomerRegistrationFinishDate.HasValue == true)
                {
                    var startDate = registration.CustomerRegistrationStartDate.Value;
                    var finishDate = registration.CustomerRegistrationFinishDate.Value;
                    membershipDurationMonths = ((finishDate.Year - startDate.Year) * 12) + 
                                              (finishDate.Month - startDate.Month);
                }

                // Profil bilgilerini hazırla (şifre hariç)
                var profile = new
                {
                    UserId = customer.UserId,
                    UserName = customer.UserName,
                    CustomerName = customer.CustomerName,
                    CustomerSurname = customer.CustomerSurname,
                    CustomerEmail = customer.CustomerEmail,
                    CustomerPhoneNumber = customer.CustomerPhoneNumber,
                    CustomerIdentityNumber = customer.CustomerIdentityNumber,
                    IsPasswordChanged = customer.IsPasswordChanged,
                    Membership = new
                    {
                        StartDate = registration?.CustomerRegistrationStartDate?.ToString("yyyy-MM-dd"),
                        FinishDate = registration?.CustomerRegistrationFinishDate?.ToString("yyyy-MM-dd"),
                        RemainingDays = remainingDays,
                        Status = membershipStatus,
                        DurationMonths = membershipDurationMonths,
                        IsActive = remainingDays > 0 || membershipStatus == "Bugün Bitiyor"
                    }
                };

                return new ApiResponse("Success", "Profil bilgileri başarıyla getirildi.", profile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi üyelik bilgilerini ve kalan gün sayısını görüntüleyebilir
        /// </summary>
        [HttpGet]
        [Route("GetMyMembershipInfo")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetMyMembershipInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                // Üyelik kaydını getir
                var registration = await _dbContext.CustomersRegistrations
                    .Where(x => x.CustomerId == userId)
                    .OrderByDescending(x => x.CustomerRegistrationStartDate)
                    .FirstOrDefaultAsync();

                if (registration == null)
                {
                    return new ApiResponse("Error", "Üyelik kaydı bulunamadı.", null);
                }

                // Kalan gün hesapla
                int remainingDays = 0;
                string membershipStatus = "Aktif";
                
                if (registration.CustomerRegistrationFinishDate.HasValue)
                {
                    var finishDate = registration.CustomerRegistrationFinishDate.Value.Date;
                    var today = DateTime.Now.Date;
                    var daysDifference = (finishDate - today).Days;

                    if (daysDifference > 0)
                    {
                        remainingDays = daysDifference;
                        membershipStatus = "Aktif";
                    }
                    else if (daysDifference == 0)
                    {
                        remainingDays = 0;
                        membershipStatus = "Bugün Bitiyor";
                    }
                    else
                    {
                        remainingDays = 0;
                        membershipStatus = "Süresi Dolmuş";
                    }
                }

                // Üyelik süresini hesapla (ay cinsinden)
                int membershipDurationMonths = 0;
                if (registration.CustomerRegistrationStartDate.HasValue && 
                    registration.CustomerRegistrationFinishDate.HasValue)
                {
                    var startDate = registration.CustomerRegistrationStartDate.Value;
                    var finishDate = registration.CustomerRegistrationFinishDate.Value;
                    membershipDurationMonths = ((finishDate.Year - startDate.Year) * 12) + 
                                              (finishDate.Month - startDate.Month);
                }

                var membershipInfo = new
                {
                    CustomerId = userId,
                    StartDate = registration.CustomerRegistrationStartDate?.ToString("yyyy-MM-dd"),
                    FinishDate = registration.CustomerRegistrationFinishDate?.ToString("yyyy-MM-dd"),
                    RemainingDays = remainingDays,
                    MembershipStatus = membershipStatus,
                    MembershipDurationMonths = membershipDurationMonths,
                    IsActive = remainingDays > 0 || membershipStatus == "Bugün Bitiyor"
                };

                return new ApiResponse("Success", "Üyelik bilgileri başarıyla getirildi.", membershipInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        [HttpPost]
        [Route("ChangePassword")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> ChangePassword(ChangePasswordRequest model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", $"Kullanıcı bilgisi bulunamadı.", null);
                }

                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.UserId == userId);

                if (customer == null)
                {
                    return new ApiResponse("Error", $"Müşteri bulunamadı.", null);
                }

                // Mevcut şifre kontrolü
                var currentPasswordHash = NulidGenarator.GenerateSHA512String(model.CurrentPassword);
                if (customer.UserPassword != currentPasswordHash)
                {
                    return new ApiResponse("Error", $"Mevcut şifre hatalı.", null);
                }

                // Yeni şifre ve onay şifresi eşleşiyor mu?
                if (model.NewPassword != model.ConfirmPassword)
                {
                    return new ApiResponse("Error", $"Yeni şifre ve onay şifresi eşleşmiyor.", null);
                }

                // Şifre validasyonu
                var (isValid, errorMessage) = CustomerService.ValidatePassword(model.NewPassword);
                if (!isValid)
                {
                    return new ApiResponse("Error", errorMessage ?? "Şifre geçersiz.", null);
                }

                // Yeni şifreyi hash'le ve kaydet
                var newPasswordHash = NulidGenarator.GenerateSHA512String(model.NewPassword);
                customer.UserPassword = newPasswordHash;
                customer.IsPasswordChanged = true; // Şifre değiştirildi olarak işaretle

                _dbContext.Customers.Update(customer);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", $"Şifre başarıyla değiştirildi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Push notification token kaydet (mobil uygulama için)
        /// </summary>
        [HttpPost]
        [Route("RegisterPushToken")]
        public async Task<ApiResponse> RegisterPushToken([FromBody] RegisterPushTokenRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bulunamadı.", null);
                }

                if (string.IsNullOrWhiteSpace(request.PushToken))
                {
                    return new ApiResponse("Error", "Push token boş olamaz.", null);
                }

                var result = await _pushTokenService.RegisterPushTokenAsync(
                    userId,
                    request.PushToken,
                    request.Platform ?? "expo"
                );

                if (result)
                {
                    return new ApiResponse("Success", "Push token başarıyla kaydedildi.", null);
                }

                return new ApiResponse("Error", "Push token kaydedilemedi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegisterPushToken hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Push notification token sil (logout veya token geçersiz olduğunda)
        /// </summary>
        [HttpPost]
        [Route("UnregisterPushToken")]
        public async Task<ApiResponse> UnregisterPushToken([FromBody] UnregisterPushTokenRequest? request = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bulunamadı.", null);
                }

                var result = await _pushTokenService.UnregisterPushTokenAsync(
                    userId,
                    request?.PushToken
                );

                if (result)
                {
                    return new ApiResponse("Success", "Push token başarıyla silindi.", null);
                }

                return new ApiResponse("Error", "Push token silinemedi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnregisterPushToken hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Kullanıcının kayıtlı push token'larını kontrol et (test için)
        /// </summary>
        [HttpGet]
        [Route("GetMyPushTokens")]
        public async Task<ApiResponse> GetMyPushTokens()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bulunamadı.", null);
                }

                var tokens = await _pushTokenService.GetActivePushTokensAsync(userId);

                return new ApiResponse("Success", $"Bulunan aktif token sayısı: {tokens.Count}", new
                {
                    TokenCount = tokens.Count,
                    Tokens = tokens.Select(t => new
                    {
                        Token = t.Substring(0, Math.Min(20, t.Length)) + "...", // İlk 20 karakteri göster
                        FullToken = t
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMyPushTokens hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }
    }
}
