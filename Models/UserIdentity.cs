using System.Text.RegularExpressions;

namespace Shawsoft.Razor.Entra.Models
{
    public class UserIdentity
    {
        public UserIdentity(string id, string upn, string? talentsoftId = null)
        {
            _upn = upn;
            _id = id;
            _talentsoftId = talentsoftId;
        }
        private string _upn;

        private string _id;

        private string? _talentsoftId;


        public string UserPrincipalName
        {
            get => _upn.ToLower();
        }

        public string Id
        {
            get => _id;
        }

        public string TalentsoftId
        {
            get => _talentsoftId?.ToUpper() ?? "n/a";
        }

        public bool IsUser
        {
            get
            {
                return Regex.IsMatch(_upn ?? "n/a", @"^[a-zA-Z]{2,4}\@(hoist\.de|hoistfinance\.com)$");
            }
        }
    }
}
