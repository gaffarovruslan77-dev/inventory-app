namespace InventoryApp.Web.Options;

public class SalesforceOptions
{
    public const string SectionName = "Salesforce";

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    /// <summary>Full OAuth token URL, e.g. https://login.salesforce.com/services/oauth2/token or https://test.salesforce.com/services/oauth2/token</summary>
    public string AuthUrl { get; set; } = "https://login.salesforce.com/services/oauth2/token";
    public string ApiVersion { get; set; } = "v59.0";
}
