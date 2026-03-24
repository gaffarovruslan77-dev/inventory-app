namespace InventoryApp.Web.Services;

public interface ISalesforceService
{
    bool IsConfigured { get; }

    Task<SalesforceAddToCrmResult> CreateAccountAndContactAsync(
        string companyName,
        string contactEmail,
        string? contactDisplayName,
        string? phone,
        string? jobTitle,
        CancellationToken cancellationToken = default);
}

public sealed record SalesforceAddToCrmResult(bool Success, string? ErrorMessage = null);
