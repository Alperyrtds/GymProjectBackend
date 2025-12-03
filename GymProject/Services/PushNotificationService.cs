using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace GymProject.Services;

public class PushNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _expoPushApiUrl = "https://exp.host/--/api/v2/push/send";

    public PushNotificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    // Tek bir push notification gönder
    public async Task<bool> SendPushNotificationAsync(string pushToken, string title, string body, object? data = null)
    {
        try
        {
            var payload = new
            {
                to = pushToken,
                sound = "default",
                title = title,
                body = body,
                data = data,
                priority = "default",
                channelId = "default"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_expoPushApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ExpoPushResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                var isSuccess = result?.Data?.Status == "ok";
                
                if (!isSuccess)
                {
                    Console.WriteLine($"[PUSH] Expo API Response: Status={result?.Data?.Status}, Id={result?.Data?.Id}, FullResponse={responseContent}");
                }
                
                return isSuccess;
            }
            else
            {
                Console.WriteLine($"[PUSH ERROR] Expo API HTTP Error: StatusCode={response.StatusCode}, Response={responseContent}");
            }

            return false;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Push notification gönderilirken hata: {ex.Message}");
            return false;
        }
    }

    // Birden fazla push notification gönder (batch)
    public async Task<Dictionary<string, bool>> SendMultiplePushNotificationsAsync(
        Dictionary<string, (string title, string body, object? data)> notifications)
    {
        var results = new Dictionary<string, bool>();

        var messages = new List<object>();
        foreach (var notification in notifications)
        {
            messages.Add(new
            {
                to = notification.Key,
                sound = "default",
                title = notification.Value.title,
                body = notification.Value.body,
                data = notification.Value.data,
                priority = "default",
                channelId = "default"
            });
        }

        try
        {
            var payload = new { messages };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_expoPushApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ExpoPushBatchResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Her token için sonucu kontrol et
                if (result?.Data != null)
                {
                    int index = 0;
                    foreach (var token in notifications.Keys)
                    {
                        if (index < result.Data.Count)
                        {
                            results[token] = result.Data[index].Status == "ok";
                        }
                        index++;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Batch push notification gönderilirken hata: {ex.Message}");
        }

        return results;
    }

    public async Task SendProgramNotificationAsync(
          string customerId,
          string programName,
          string programId,
          Services.PushTokenService pushTokenService,
          Services.PushNotificationService pushNotificationService)
    {
        try
        {
            Console.WriteLine($"[PUSH] SendProgramNotificationAsync başladı. CustomerId: {customerId}, ProgramName: {programName}");

            // Kullanıcının aktif push token'larını al
            var pushTokens = await pushTokenService.GetActivePushTokensAsync(customerId);
            Console.WriteLine($"[PUSH] Bulunan aktif token sayısı: {pushTokens?.Count ?? 0}");

            if (pushTokens == null || pushTokens.Count == 0)
            {
                Console.WriteLine($"[PUSH WARNING] Kullanıcı {customerId} için aktif push token bulunamadı. Kullanıcı mobil uygulamada login olup token kaydetmemiş olabilir.");
                return;
            }

            // Notification içeriği
            var title = "Yeni Antrenman Program Atandı! 🎯";
            var body = $"Size özel {programName} hazır. Programı görmek için tıklayın.";
            var data = new
            {
                screen = "ProgramDetail",
                programId = programId,
                type = "program_assigned"
            };

            Console.WriteLine($"[PUSH] {pushTokens.Count} token için notification gönderiliyor...");

            // Her token için notification gönder
            int successCount = 0;
            int failCount = 0;
            foreach (var token in pushTokens)
            {
                try
                {
                    Console.WriteLine($"[PUSH] Token'a notification gönderiliyor: {token.Substring(0, Math.Min(20, token.Length))}...");
                    var result = await SendPushNotificationAsync(
                        token,
                        title,
                        body,
                        data
                    );

                    if (result)
                    {
                        successCount++;
                        Console.WriteLine($"[PUSH SUCCESS] Notification başarıyla gönderildi.");
                    }
                    else
                    {
                        failCount++;
                        Console.WriteLine($"[PUSH FAIL] Notification gönderilemedi (Expo API false döndü).");
                    }
                }
                catch (Exception tokenEx)
                {
                    failCount++;
                    Console.WriteLine($"[PUSH ERROR] Token için notification gönderilirken hata: {tokenEx.Message}");
                }
            }

            Console.WriteLine($"[PUSH] Notification gönderme tamamlandı. Başarılı: {successCount}, Başarısız: {failCount}");
        }
        catch (Exception ex)
        {
            // Log error ama program oluşturma işlemini bozma
            Console.WriteLine($"[PUSH ERROR] SendProgramNotificationAsync içinde hata: {ex.Message}");
            Console.WriteLine($"[PUSH ERROR] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[PUSH ERROR] InnerException: {ex.InnerException.Message}");
            }
        }
    }
}

// Response modelleri
public class ExpoPushResponse
{
    public ExpoPushData? Data { get; set; }
}

public class ExpoPushData
{
    public string? Status { get; set; }
    public string? Id { get; set; }
}

public class ExpoPushBatchResponse
{
    public List<ExpoPushData>? Data { get; set; }
}

