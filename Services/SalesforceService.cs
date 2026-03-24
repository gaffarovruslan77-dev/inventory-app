using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using InventoryApp.Web.Options;

namespace InventoryApp.Web.Services;

public class SalesforceService : ISalesforceService
{
    private readonly HttpClient _http;
    private readonly SalesforceOptions _options;
    private readonly ILogger<SalesforceService> _logger;

    private string? _cachedToken;
    private string? _cachedInstanceUrl;
    private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;

    public SalesforceService(HttpClient http, IOptions<SalesforceOptions> options, ILogger<SalesforceService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ClientId) && !string.IsNullOrWhiteSpace(_options.ClientSecret);

    public async Task<SalesforceAddToCrmResult> CreateAccountAndContactAsync(
        string companyName,
        string contactEmail,
        string? contactDisplayName,
        string? phone,
        string? jobTitle,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return new SalesforceAddToCrmResult(false, "Salesforce is not configured.");

        if (string.IsNullOrWhiteSpace(companyName))
            return new SalesforceAddToCrmResult(false, "Company name is required.");

        if (string.IsNullOrWhiteSpace(contactEmail))
            return new SalesforceAddToCrmResult(false, "Contact email is required.");

        try
        {
            var (token, instanceUrl) = await GetAccessTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(instanceUrl))
                return new SalesforceAddToCrmResult(false, "Could not obtain Salesforce access token.");

            var apiRoot = $"{instanceUrl.TrimEnd('/')}/services/data/{_options.ApiVersion.TrimStart('/')}";
            var accountId = await CreateAccountAsync(apiRoot, token, companyName.Trim(), cancellationToken);
            if (string.IsNullOrEmpty(accountId))
                return new SalesforceAddToCrmResult(false, "Failed to create Account in Salesforce.");

            var (firstName, lastName) = SplitDisplayName(contactDisplayName, contactEmail);
            var contactOk = await CreateContactAsync(
                apiRoot,
                token,
                accountId,
                lastName,
                firstName,
                contactEmail.Trim(),
                phone,
                jobTitle,
                cancellationToken);

            if (!contactOk)
                return new SalesforceAddToCrmResult(false, "Failed to create Contact in Salesforce.");

            return new SalesforceAddToCrmResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Salesforce CreateAccountAndContact failed");
            return new SalesforceAddToCrmResult(false, "Unexpected error while calling Salesforce.");
        }
    }

    private async Task<(string? token, string? instanceUrl)> GetAccessTokenAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        if (_cachedToken != null && _cachedInstanceUrl != null && now < _tokenExpiresAt.AddMinutes(-2))
            return (_cachedToken, _cachedInstanceUrl);

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        };

        using var content = new FormUrlEncodedContent(form);
        using var response = await _http.PostAsync(_options.AuthUrl, content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Salesforce token error {Status}: {Body}", (int)response.StatusCode, body);
            return (null, null);
        }

        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var accessToken = root.GetProperty("access_token").GetString();
        var instanceUrl = root.GetProperty("instance_url").GetString();
        var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

        _cachedToken = accessToken;
        _cachedInstanceUrl = instanceUrl;
        _tokenExpiresAt = now.AddSeconds(Math.Max(60, expiresIn));

        return (accessToken, instanceUrl);
    }

    private async Task<string?> CreateAccountAsync(string apiRoot, string token, string name, CancellationToken ct)
    {
        var url = $"{apiRoot}/sobjects/Account/";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = new StringContent(
            JsonSerializer.Serialize(new { Name = name }),
            Encoding.UTF8,
            "application/json");

        using var response = await _http.SendAsync(req, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Salesforce Account create {Status}: {Body}", (int)response.StatusCode, body);
            return null;
        }

        var result = JsonSerializer.Deserialize<SalesforceCreateResponse>(body);
        return result?.Id;
    }

    private async Task<bool> CreateContactAsync(
        string apiRoot,
        string token,
        string accountId,
        string lastName,
        string? firstName,
        string email,
        string? phone,
        string? title,
        CancellationToken ct)
    {
        var url = $"{apiRoot}/sobjects/Contact/";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.TryAddWithoutValidation("Sforce-Duplicate-Rule-Header", "allowSave=true");

        var payload = new ContactCreatePayload
        {
            LastName = lastName,
            Email = email,
            AccountId = accountId,
            FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim()
        };

        var jsonOpts = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        req.Content = new StringContent(
            JsonSerializer.Serialize(payload, jsonOpts),
            Encoding.UTF8,
            "application/json");

        using var response = await _http.SendAsync(req, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Salesforce Contact create {Status}: {Body}", (int)response.StatusCode, body);
            return false;
        }

        return true;
    }

    private static (string? firstName, string lastName) SplitDisplayName(string? displayName, string email)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            var trimmed = displayName.Trim();
            var space = trimmed.IndexOf(' ');
            if (space > 0 && space < trimmed.Length - 1)
                return (trimmed[..space], trimmed[(space + 1)..].Trim());
            return (null, trimmed);
        }

        var at = email.IndexOf('@');
        var local = at > 0 ? email[..at] : email;
        return (null, string.IsNullOrWhiteSpace(local) ? "Unknown" : local);
    }

    private sealed class ContactCreatePayload
    {
        public string LastName { get; set; } = "";
        public string? FirstName { get; set; }
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? Title { get; set; }
        public string AccountId { get; set; } = "";
    }

    private sealed class SalesforceCreateResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
