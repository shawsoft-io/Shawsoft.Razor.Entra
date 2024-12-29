using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Shawsoft.Razor.Entra.Models;
using Shawsoft.Razor.Entra.Services;
using Shawsoft.Razor.Entra.Utilities;

namespace Shawsoft.Razor.Entra
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntraServices(this IServiceCollection services, Action<EntraOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddSingleton<IValidateOptions<EntraOptions>, EntraOptionsValidator>();
            services.AddSingleton<GraphServiceClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<EntraOptions>>().Value;

                var clientSecretCredential = new ClientSecretCredential(
                    options.TenantId,
                    options.ClientId,
                    options.ClientSecret
                );

                var scopes = new[] { "https://graph.microsoft.com/.default"};

                return new GraphServiceClient(clientSecretCredential, scopes);
            });

            services.AddScoped<EntraDirectoryService>();


            return services;
        }
    }
}