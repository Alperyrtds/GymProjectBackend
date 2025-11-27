using System.Security.Claims;
using GymProject.Helpers;
using GymProject.Models;
using GymProject.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public ContactController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Yeni iletişim mesajı ekle (Herkes kullanabilir - authorization gerekmez)
        /// </summary>
        [HttpPost]
        [Route("AddContactMessage")]
        public async Task<ApiResponse> AddContactMessage([FromBody] AddContactMessageRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    return new ApiResponse("Error", "Ad Soyad alanı boş olamaz.", null);
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return new ApiResponse("Error", "E-posta alanı boş olamaz.", null);
                }

                if (string.IsNullOrWhiteSpace(model.Message))
                {
                    return new ApiResponse("Error", "Mesaj alanı boş olamaz.", null);
                }

                var contactMessage = new ContactMessage
                {
                    ContactMessageId = NulidGenarator.Id(),
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Message = model.Message,
                    CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    IsRead = false
                };

                await _dbContext.ContactMessages.AddAsync(contactMessage);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Mesajınız başarıyla gönderildi.", contactMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Tüm iletişim mesajlarını getir (Admin ve Trainer için)
        /// </summary>
        [HttpGet]
        [Route("GetAllContactMessages")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetAllContactMessages()
        {
            try
            {
                var messages = await _dbContext.ContactMessages
                    .OrderByDescending(x => x.CreatedDate)
                    .Select(x => new
                    {
                        x.ContactMessageId,
                        x.FullName,
                        x.Email,
                        PhoneNumber = x.PhoneNumber!.StartsWith("5")
                            ? "0" + x.PhoneNumber
                            : x.PhoneNumber,
                        x.Message,
                        CreatedDate = x.CreatedDate.HasValue
                            ? x.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            : null,
                        x.IsRead
                    })
                    .ToListAsync();

                return new ApiResponse("Success", "İletişim mesajları başarıyla getirildi.", messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Belirli bir iletişim mesajını getir (Admin ve Trainer için)
        /// </summary>
        [HttpPost]
        [Route("GetContactMessage")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetContactMessage([FromBody] GetContactMessageRequest request)
        {
            try
            {
                var message = await _dbContext.ContactMessages
                    .FirstOrDefaultAsync(x => x.ContactMessageId == request.ContactMessageId);

                if (message == null)
                {
                    return new ApiResponse("Error", "Mesaj bulunamadı.", null);
                }

                message.PhoneNumber = $"0{message.PhoneNumber}";
                var messageData = new
                {
                    message.ContactMessageId,
                    message.FullName,
                    message.Email,
                    message.PhoneNumber,
                    message.Message,
                    CreatedDate = message.CreatedDate.HasValue ? message.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    message.IsRead
                };

                return new ApiResponse("Success", "Mesaj başarıyla getirildi.", messageData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// İletişim mesajını güncelle (Okundu işaretle) - Admin ve Trainer için
        /// </summary>
        [HttpPost]
        [Route("UpdateContactMessage")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> UpdateContactMessage([FromBody] UpdateContactMessageRequest model)
        {
            try
            {
                var message = await _dbContext.ContactMessages
                    .FirstOrDefaultAsync(x => x.ContactMessageId == model.ContactMessageId);

                if (message == null)
                {
                    return new ApiResponse("Error", "Mesaj bulunamadı.", null);
                }

                // IsRead güncelle
                if (model.IsRead.HasValue)
                {
                    message.IsRead = model.IsRead.Value;
                }

                await _dbContext.SaveChangesAsync();

                var updatedMessage = new
                {
                    message.ContactMessageId,
                    message.FullName,
                    message.Email,
                    message.PhoneNumber,
                    message.Message,
                    CreatedDate = message.CreatedDate.HasValue ? message.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    message.IsRead
                };

                return new ApiResponse("Success", "Mesaj başarıyla güncellendi.", updatedMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// İletişim mesajını sil (Admin ve Trainer için)
        /// </summary>
        [HttpPost]
        [Route("DeleteContactMessage")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> DeleteContactMessage([FromBody] DeleteContactMessageRequest request)
        {
            try
            {
                var message = await _dbContext.ContactMessages
                    .FirstOrDefaultAsync(x => x.ContactMessageId == request.ContactMessageId);

                if (message == null)
                {
                    return new ApiResponse("Error", "Mesaj bulunamadı.", null);
                }

                _dbContext.ContactMessages.Remove(message);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Mesaj başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }
    }
}

