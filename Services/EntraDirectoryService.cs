using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Shawsoft.Razor.Entra.Models;

namespace Shawsoft.Razor.Entra.Services
{
    public class EntraDirectoryService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<EntraDirectoryService> _logger;

        public EntraDirectoryService(GraphServiceClient graphServiceClient, ILogger<EntraDirectoryService> logger)
        {
            _graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<EntraUser>> GetAppUserByName(string query, int results = 10, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(query, nameof(query));

            try
            {
                var users = await _graphServiceClient.Users.GetAsync(req =>
                {
                    req.QueryParameters.Select =
                    [
                        "id", "userPrincipalName", "givenName", "surname", "jobTitle", "department",
                        "country", "mail", "mobilePhone", "accountEnabled"
                    ];
                    req.QueryParameters.Top = results;
                    req.QueryParameters.Filter = $"startsWith(displayName, '{query}') or startsWith(givenName, '{query}') or startsWith(surname, '{query}')";
                }, cancellationToken);

                if (users?.Value == null || users.Value.Count == 0)
                    return [];

                var result =  await Task.WhenAll(users.Value
                    .Where(user => user != null)
                    .Select(async user =>
                    {
                        var appUser = ConvertToAppUser(user);
                        if (!string.IsNullOrEmpty(appUser.Identity?.Id))
                        {
                            appUser.Photo = await GetPhoto(appUser.Identity.Id, cancellationToken);
                        }
                        return appUser;
                    }));

                return [.. result];
            }
            catch (ServiceException ex)
            {
                _logger.LogError($"Error fetching users by query: {ex.Message}");
                throw;
            }
        }

        public async Task<EntraUser?> GetAppUserById(string id, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            try
            {
                var user = await _graphServiceClient.Users[id].GetAsync(req =>
                {
                    req.QueryParameters.Select =
                    [
                        "id", "userPrincipalName", "givenName", "surname", "jobTitle", "department",
                        "country", "mail", "mobilePhone", "accountEnabled", "manager"
                    ];
                    req.QueryParameters.Expand = ["manager"];
                }, cancellationToken);

                if (user == null)
                    return null;

                var appUser = ConvertToAppUser(user);
                appUser.Photo = await GetPhoto(appUser.Identity?.Id, cancellationToken);
                return appUser;
            }
            catch (ServiceException ex)
            {
                _logger.LogError($"Error fetching user by ID: {ex.Message}");
                throw;
            }
        }

        public async Task<EntraUser?> GetAppUserByUpn(string upn, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(upn, nameof(upn));

            try
            {
                var users = await _graphServiceClient.Users.GetAsync(req =>
                {
                    req.QueryParameters.Filter = $"userPrincipalName eq '{upn}'";
                    req.QueryParameters.Select = new[]
                    {
                        "id", "userPrincipalName", "givenName", "surname", "jobTitle", "department",
                        "country", "mail", "mobilePhone", "accountEnabled"
                    };
                }, cancellationToken);

                var user = users?.Value?.FirstOrDefault();

                if (user == null)
                    return null;

                var appUser = ConvertToAppUser(user);
                appUser.Photo = await GetPhoto(appUser.Identity?.Id, cancellationToken);

                return appUser;
            }
            catch (ServiceException ex)
            {
                _logger.LogError($"Error fetching user by UPN: {ex.Message}");
                throw;
            }
        }

        public async Task<List<EntraUser>> GetGroupMembersAsync(string groupName, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(groupName, nameof(groupName));

            try
            {
                var groupResponse = await _graphServiceClient.Groups.GetAsync(req =>
                {
                    req.QueryParameters.Filter = $"displayName eq '{groupName}'";
                }, cancellationToken);

                var group = groupResponse?.Value?.FirstOrDefault();

                if (group == null)
                    throw new InvalidOperationException($"Group '{groupName}' not found.");

                var membersResponse = await _graphServiceClient.Groups[group.Id].Members.GetAsync(req => { }, cancellationToken);

                return membersResponse?.Value?
                    .OfType<User>()
                    .Select(ConvertToAppUser)
                    .ToList() ?? [];
            }
            catch (ServiceException ex)
            {
                _logger.LogError($"Error fetching group members for '{groupName}': {ex.Message}");
                throw;
            }
        }

        public async Task<UserPhoto?> GetPhoto(string? userId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId, nameof(userId));

            try
            {
                var stream = await _graphServiceClient.Users[userId].Photo.Content.GetAsync(req => { }, cancellationToken);

                if (stream == null)
                    return null;

                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream, cancellationToken);

                return new UserPhoto
                {
                    ContentType = "data:image/jpeg",
                    Base64EncodedImage = Convert.ToBase64String(memoryStream.ToArray())
                };
            }
            catch (ServiceException ex) when (ex.ResponseStatusCode == 404)
            {
                _logger.LogWarning($"User with ID {userId} does not have a photo.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error fetching photo for user ID {userId}: {ex.Message}");
                return null;
            }
        }

        private EntraUser ConvertToAppUser(User user)
        {
            if (string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.UserPrincipalName))
                throw new Exception("Cannot parse user to EntraUser object");

            return new EntraUser(user.Id, user.UserPrincipalName, user.EmployeeId)
            {
                GivenName = user.GivenName,
                Surname = user.Surname,
                ContactDetails = new()
                {
                    EMail = user.Mail,
                    MobilePhone = user.MobilePhone
                },
                JobDetails = new()
                {
                    JobTitle = user.JobTitle,
                    Department = user.Department,
                    Country = user.Country,
                    Office = user.OfficeLocation,
                    Company = user.CompanyName,
                    ManagerId = user.Manager?.Id
                },
                AccountEnabled = user.AccountEnabled ?? false
            };
        }
    }
}