namespace Shawsoft.Razor.Entra.Models
{
    public class EntraOptions
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TenantId { get; set; }
        public string? Authority { get; set; }
        public string? GraphApiBaseUrl { get; set; }
    }
}
