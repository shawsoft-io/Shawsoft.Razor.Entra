using Microsoft.Extensions.Options;
using Shawsoft.Razor.Entra.Models;

namespace Shawsoft.Razor.Entra.Utilities
{
    public class EntraOptionsValidator : IValidateOptions<EntraOptions>
    {
        public ValidateOptionsResult Validate(string? name, EntraOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ClientId)) 
                return ValidateOptionsResult.Fail("ClientId is required.");

            if (string.IsNullOrWhiteSpace(options.ClientSecret)) 
                return ValidateOptionsResult.Fail("ClientSecret is required.");

            if (string.IsNullOrWhiteSpace(options.TenantId)) 
                return ValidateOptionsResult.Fail("TenantId is required.");

            return ValidateOptionsResult.Success;
        }
    }
}