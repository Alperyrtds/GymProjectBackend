using GymProject.Helpers;
using GymProject.Models;
using GymProject.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MovementController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public MovementController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// YouTube URL'den video ID'yi çıkarır (normal video, shorts, youtu.be formatlarını destekler)
        /// </summary>
        private string? ExtractYouTubeVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                // Eğer zaten embed URL ise direkt video ID'yi al
                if (url.Contains("/embed/"))
                {
                    var embedIndex = url.IndexOf("/embed/");
                    var videoId = url.Substring(embedIndex + 7);
                    // Query string varsa temizle
                    var queryIndex = videoId.IndexOf('?');
                    if (queryIndex > 0)
                        videoId = videoId.Substring(0, queryIndex);
                    return videoId;
                }

                var uri = new Uri(url);

                // YouTube Shorts formatı: https://www.youtube.com/shorts/VIDEO_ID
                if (uri.AbsolutePath.StartsWith("/shorts/"))
                {
                    var videoId = uri.AbsolutePath.Substring(8); // "/shorts/" kısmını atla
                    // Query string varsa temizle
                    var queryIndex = videoId.IndexOf('?');
                    if (queryIndex > 0)
                        videoId = videoId.Substring(0, queryIndex);
                    return videoId;
                }

                // youtu.be formatı: https://youtu.be/VIDEO_ID
                if (uri.Host.Contains("youtu.be"))
                {
                    var videoId = uri.AbsolutePath.TrimStart('/');
                    // Query string varsa temizle
                    var queryIndex = videoId.IndexOf('?');
                    if (queryIndex > 0)
                        videoId = videoId.Substring(0, queryIndex);
                    return videoId;
                }

                // Normal YouTube formatı: https://www.youtube.com/watch?v=VIDEO_ID
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var vParam = query["v"];
                
                if (!string.IsNullOrEmpty(vParam))
                {
                    return vParam;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"YouTube video ID çıkarılırken hata: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Admin veya Trainer hareket ekleyebilir
        /// </summary>
        [HttpPost]
        [Route("AddMovement")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> AddMovement(Movement model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.MovementName))
                {
                    return new ApiResponse("Error", "Hareket adı boş olamaz.", null);
                }

                var originalUrl = model.MovementVideoUrl;

                // VIDEO_ID'yi yakala (normal video, shorts, youtu.be formatlarını destekler)
                var videoId = ExtractYouTubeVideoId(originalUrl);

                if (string.IsNullOrEmpty(videoId))
                {
                    return new ApiResponse("Error", "Geçersiz YouTube linki. Normal video, Shorts veya youtu.be formatında olmalıdır.", null);
                }

                // Embed URL oluştur
                var embedVideoUrl = $"https://www.youtube.com/embed/{videoId}";

                var movement = new Movement
                {
                    MovementId = NulidGenarator.Id(),
                    MovementName = model.MovementName,
                    MovementDescription = model.MovementDescription,
                    MovementVideoUrl = embedVideoUrl
                };

                await _dbContext.Movements.AddAsync(movement);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Hareket başarıyla eklendi.", movement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Tüm roller hareketleri görebilir
        /// </summary>
        [HttpGet]
        [Route("GetAllMovements")]
        public async Task<ApiResponse> GetAllMovements()
        {
            try
            {
                var movements = await _dbContext.Movements
                    .OrderBy(x => x.MovementName)
                    .ToListAsync();

                return new ApiResponse("Success", "Hareketler başarıyla getirildi.", movements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Frontend dropdown için sadece ID ve Name döndüren hafif endpoint
        /// </summary>
        [HttpGet]
        [Route("GetMovementsForDropdown")]
        [Authorize]
        public async Task<ApiResponse> GetMovementsForDropdown()
        {
            try
            {
                var movements = await _dbContext.Movements
                    .OrderBy(x => x.MovementName)
                    .Select(m => new
                    {
                        m.MovementId,
                        m.MovementName
                    })
                    .ToListAsync();

                return new ApiResponse("Success", "Hareketler başarıyla getirildi.", movements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Hareket detayını getirir
        /// </summary>
        [HttpPost]
        [Route("GetMovementById")]
        public async Task<ApiResponse> GetMovementById([FromBody] MovementRequest request)
        {
            try
            {
                var movement = await _dbContext.Movements
                    .FirstOrDefaultAsync(x => x.MovementId == request.MovementId);

                if (movement == null)
                {
                    return new ApiResponse("Error", "Hareket bulunamadı.", null);
                }

                return new ApiResponse("Success", "Hareket başarıyla getirildi.", movement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer hareketi güncelleyebilir
        /// </summary>
        [HttpPost]
        [Route("UpdateMovement")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> UpdateMovement(Movement model)
        {
            try
            {
                var movement = await _dbContext.Movements
                    .FirstOrDefaultAsync(x => x.MovementId == model.MovementId);

                if (movement == null)
                {
                    return new ApiResponse("Error", "Hareket bulunamadı.", null);
                }

                if (string.IsNullOrWhiteSpace(model.MovementName))
                {
                    return new ApiResponse("Error", "Hareket adı boş olamaz.", null);
                }

                if (string.IsNullOrWhiteSpace(model.MovementVideoUrl))
                {
                    return new ApiResponse("Error", "Video url boş olamaz.", null);
                }


                var originalUrl = model.MovementVideoUrl;

                // VIDEO_ID'yi yakala (normal video, shorts, youtu.be formatlarını destekler)
                var videoId = ExtractYouTubeVideoId(originalUrl);

                if (string.IsNullOrEmpty(videoId))
                {
                    return new ApiResponse("Error", "Geçersiz YouTube linki. Normal video, Shorts veya youtu.be formatında olmalıdır.", null);
                }

                // Embed URL oluştur
                var embedVideoUrl = $"https://www.youtube.com/embed/{videoId}";

                movement.MovementName = model.MovementName;
                movement.MovementDescription = model.MovementDescription;
                movement.MovementVideoUrl = embedVideoUrl;

                _dbContext.Movements.Update(movement);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Hareket başarıyla güncellendi.", movement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer hareketi silebilir
        /// </summary>
        [HttpPost]
        [Route("DeleteMovement")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> DeleteMovement([FromBody] MovementRequest request)
        {
            try
            {
                var movement = await _dbContext.Movements
                    .FirstOrDefaultAsync(x => x.MovementId == request.MovementId);

                if (movement == null)
                {
                    return new ApiResponse("Error", "Hareket bulunamadı.", null);
                }

                // Bu hareketi kullanan programlar var mı kontrol et
                var programsUsingMovement = await _dbContext.CustomersPrograms
                    .AnyAsync(x => x.MovementId == movement.MovementId);

                if (programsUsingMovement)
                {
                    return new ApiResponse("Error", "Bu hareket kullanılan programlarda mevcut. Önce programlardan kaldırılmalı.", null);
                }

                _dbContext.Movements.Remove(movement);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Hareket başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }
    }
}

