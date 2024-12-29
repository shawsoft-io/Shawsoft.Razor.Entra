namespace Shawsoft.Razor.Entra.Models
{
    public class UserPhoto
    {
        public Uri? Uri { get; set; }

        public string? ContentType { get; set; }
        public string? Base64EncodedImage { get; set; }

        public override string ToString()
        {
            return $"data:{ContentType};base64,{Base64EncodedImage}";
        }
    }
}
