using GymProject.Helpers;

namespace GymProject.Services
{
    public class CustomerService
    {
        /// <summary>
        /// Müşteri için hatırlanabilir şifre oluşturur (Ad.Soyad formatında)
        /// </summary>
        public static string GenerateCustomerPassword(string firstName, string lastName)
        {
            // Türkçe karakterleri İngilizce karşılıklarına çevir
            firstName = RemoveTurkishCharacters(firstName.ToLower());
            lastName = RemoveTurkishCharacters(lastName.ToLower());
            
            // Boşlukları ve özel karakterleri temizle
            firstName = new string(firstName.Where(c => char.IsLetterOrDigit(c)).ToArray());
            lastName = new string(lastName.Where(c => char.IsLetterOrDigit(c)).ToArray());
            
            // Eğer ad veya soyad boşsa, rastgele şifre oluştur
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                return NulidGenarator.GenerateRandomPassword(10);
            }
            
            // İlk harfleri büyük yap
            if (firstName.Length > 0) firstName = char.ToUpper(firstName[0]) + (firstName.Length > 1 ? firstName.Substring(1) : "");
            if (lastName.Length > 0) lastName = char.ToUpper(lastName[0]) + (lastName.Length > 1 ? lastName.Substring(1) : "");
            
            // Ad.Soyad formatında oluştur
            return $"{firstName}.{lastName}";
        }

        /// <summary>
        /// Türkçe karakterleri İngilizce karşılıklarına çevirir
        /// </summary>
        private static string RemoveTurkishCharacters(string text)
        {
            var turkishChars = new Dictionary<char, char>
            {
                {'ç', 'c'}, {'Ç', 'C'},
                {'ğ', 'g'}, {'Ğ', 'G'},
                {'ı', 'i'}, {'İ', 'I'},
                {'ö', 'o'}, {'Ö', 'O'},
                {'ş', 's'}, {'Ş', 'S'},
                {'ü', 'u'}, {'Ü', 'U'}
            };

            return new string(text.Select(c => turkishChars.ContainsKey(c) ? turkishChars[c] : c).ToArray());
        }

        /// <summary>
        /// Şifre validasyonu yapar
        /// </summary>
        public static (bool isValid, string? errorMessage) ValidatePassword(string password)
        {
            if (password.Length < 8)
            {
                return (false, "Şifre en az 8 karakter olmalıdır.");
            }

            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            {
                return (false, "Şifre en az bir büyük harf, bir küçük harf ve bir rakam içermelidir.");
            }

            return (true, null);
        }
    }
}

