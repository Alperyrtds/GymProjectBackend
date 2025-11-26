using NUlid;
using System.Security.Cryptography;
using System.Text;

namespace GymProject.Helpers
{
    public class NulidGenarator
    {
        public static string Id()
        {
            var myUlid = Ulid.NewUlid().ToString();

            return myUlid;
        }
        public static string GenerateSHA512String(string inputString)
        {
            SHA512 sha512 = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        /// <summary>
        /// Güvenli random şifre oluşturur (8-12 karakter, büyük/küçük harf, rakam, özel karakter)
        /// </summary>
        public static string GenerateRandomPassword(int length = 10)
        {
            const string uppercase = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%&*";
            const string allChars = uppercase + lowercase + digits + special;

            var random = new Random();
            var password = new StringBuilder(length);

            // En az bir karakter her kategoriden
            password.Append(uppercase[random.Next(uppercase.Length)]);
            password.Append(lowercase[random.Next(lowercase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(special[random.Next(special.Length)]);

            // Kalan karakterleri rastgele ekle
            for (int i = password.Length; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Karakterleri karıştır
            var shuffled = password.ToString().ToCharArray();
            for (int i = shuffled.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            return new string(shuffled);
        }
    }
}
