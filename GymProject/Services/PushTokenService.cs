using GymProject.Helpers;
using GymProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GymProject.Services;

public class PushTokenService
{
    private readonly AlperyurtdasGymProjectContext _context;

    public PushTokenService(AlperyurtdasGymProjectContext context)
    {
        _context = context;
    }

    // Push token kaydet
    public async Task<bool> RegisterPushTokenAsync(string customerId, string pushToken, string platform = "expo")
    {
        try
        {
            // Mevcut token'ı kontrol et
            var existingToken = await _context.PushTokens
                .FirstOrDefaultAsync(pt => pt.CustomerId == customerId && pt.Token == pushToken);

            if (existingToken != null)
            {
                existingToken.UpdatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                existingToken.IsActive = true;
                if (!string.IsNullOrEmpty(platform))
                {
                    existingToken.Platform = platform;
                }
            }
            else
            {
                var newToken = new PushToken
                {
                    Id = NulidGenarator.Id(),
                    CustomerId = customerId,
                    Token = pushToken,
                    Platform = platform,
                    CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    UpdatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    IsActive = true
                };
                _context.PushTokens.Add(newToken);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Push token kaydedilirken hata: {ex.Message}");
            return false;
        }
    }

    // Push token sil
    public async Task<bool> UnregisterPushTokenAsync(string customerId, string? pushToken = null)
    {
        try
        {
            var tokens = _context.PushTokens
                .Where(pt => pt.CustomerId == customerId);

            if (!string.IsNullOrEmpty(pushToken))
            {
                tokens = tokens.Where(pt => pt.Token == pushToken);
            }

            var tokenList = await tokens.ToListAsync();
            foreach (var token in tokenList)
            {
                token.IsActive = false;
                token.UpdatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Push token silinirken hata: {ex.Message}");
            return false;
        }
    }

    // Kullanıcının aktif push token'larını al
    public async Task<List<string>> GetActivePushTokensAsync(string customerId)
    {
        try
        {
            Console.WriteLine($"[PUSH TOKEN] GetActivePushTokensAsync çağrıldı. CustomerId: {customerId}");
            
            var tokens = await _context.PushTokens
                .Where(pt => pt.CustomerId == customerId && pt.IsActive)
                .Select(pt => pt.Token)
                .ToListAsync();
            
            Console.WriteLine($"[PUSH TOKEN] Bulunan aktif token sayısı: {tokens.Count}");
            
            return tokens;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PUSH TOKEN ERROR] GetActivePushTokensAsync hatası: {ex.Message}");
            Console.WriteLine($"[PUSH TOKEN ERROR] StackTrace: {ex.StackTrace}");
            
            // Migration yapılmamış olabilir, boş liste döndür
            if (ex.Message.Contains("does not exist") || ex.Message.Contains("relation") || ex.Message.Contains("table"))
            {
                Console.WriteLine($"[PUSH TOKEN ERROR] PushTokens tablosu bulunamadı! Migration yapılması gerekiyor: dotnet ef migrations add AddPushTokenTable && dotnet ef database update");
            }
            
            return new List<string>();
        }
    }
}

