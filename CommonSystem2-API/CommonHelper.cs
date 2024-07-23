using CommonSystem2_API.Models;
using System.Security.Cryptography;
using System.Text;

namespace CommonSystem2_API
{
    public static class CommonHelper
    {
        public static string GenerateHmacSignature(string data,IConfiguration _configuration)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_configuration["Link:Secret"])))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyHmacSignature(string data, string signature, IConfiguration _configuration)
        {
            var expectedSignature = GenerateHmacSignature(data, _configuration);
            return signature == expectedSignature;
        }

        public static string GetInviteEmailBody(User user, IConfiguration _configuration)
        {
            var expiryTime = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Link:ExpirationTime"]));
            var dataToSign = $"{user.Id}|{expiryTime:o}";
            var signature = CommonHelper.GenerateHmacSignature(dataToSign, _configuration);
            return $"{_configuration["Link:BaseUrl"]}/{_configuration["Link:RedirectPath"]}?guid={user.Id}&expiryTime={expiryTime:o}&signature={signature}";
        }
    }
}
