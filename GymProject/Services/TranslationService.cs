using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using XLocalizer.Translate;

namespace GymProject.Services;

public class TranslationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ITranslator _translator;

    public TranslationService(HttpClient httpClient, IConfiguration configuration, ITranslator translator)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _translator = translator;
    }

    /// <summary>
    /// Metni belirtilen dile çevirir (Google Translate API veya başka bir servis kullanılabilir)
    /// Şimdilik basit bir placeholder - gerçek çeviri API'si entegre edilebilir
    /// </summary>
    public async Task<string> TranslateTextAsync(string text, string fromLanguage, string toLanguage)
    {
        try
        {
            // Eğer aynı dilse çeviri yapma
            if (fromLanguage.ToLower() == toLanguage.ToLower())
            {
                return text;
            }

            var translation = await _translator.TranslateAsync(fromLanguage, toLanguage, text,  "string");

            if (translation == null)
            {
                Console.WriteLine("hata");
            }

            return translation.Text;

            // TODO: Gerçek çeviri API'si entegrasyonu (Google Translate, DeepL, vb.)
            // Şimdilik basit bir placeholder döndürüyoruz
            // Gerçek implementasyon için:
            // - Google Cloud Translation API
            // - Azure Translator
            // - DeepL API
            // vb. kullanılabilir

            // Örnek: Google Translate API kullanımı (API key gerekli)
            /*
            var apiKey = _configuration["Translation:GoogleApiKey"];
            var url = $"https://translation.googleapis.com/language/translate/v2?key={apiKey}";
            
            var payload = new
            {
                q = text,
                source = fromLanguage,
                target = toLanguage
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<GoogleTranslateResponse>(responseContent);
                return result?.Data?.Translations?.FirstOrDefault()?.TranslatedText ?? text;
            }
            */

            // Şimdilik: Çeviri yapılamadığında orijinal metni döndür
            Console.WriteLine($"[TRANSLATION] Çeviri yapılamadı: {text} ({fromLanguage} -> {toLanguage})");
            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TRANSLATION ERROR] Çeviri hatası: {ex.Message}");
            return text; // Hata durumunda orijinal metni döndür
        }
    }

    /// <summary>
    /// Birden fazla metni aynı anda çevirir
    /// </summary>
    public async Task<List<string>> TranslateTextsAsync(List<string> texts, string fromLanguage, string toLanguage)
    {
        var translatedTexts = new List<string>();
        
        foreach (var text in texts)
        {
            var translated = await TranslateTextAsync(text, fromLanguage, toLanguage);
            translatedTexts.Add(translated);
        }

        return translatedTexts;
    }
}

