namespace Shawsoft.Razor.Entra.Models
{
    public class EntraUser
    {
        public EntraUser(string id, string upn, string? empId = null) 
        { 
            Identity = new(id, upn, empId);
        }

        public UserIdentity Identity { get; set; }

        public string? GivenName { get; set; }

        public string? Surname { get; set; }

        public UserJobDetails? JobDetails { get; set; }

        public UserContactDetails? ContactDetails { get; set; }


        public UserPhoto? Photo { get; set; }

        public bool AccountEnabled { get; set; }

        public string? DisplayName
        {
            get => $"{GivenName} {Surname}";
        }

        public string? Initials
        {
            get => $"{GivenName?[0]}{Surname?[0]}";
        }
    }
}
