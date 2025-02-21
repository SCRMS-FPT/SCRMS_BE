namespace Identity.Application.Extensions
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public int ExpiryHours { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}