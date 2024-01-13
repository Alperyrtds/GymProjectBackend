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
            SHA512 sha512 = SHA512Managed.Create();
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
    }
}
