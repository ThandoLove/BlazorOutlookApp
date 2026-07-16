using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OperationalWorkspace.Infrastructure.SageX3.X3Dtos;

/// <summary>
/// Raw external Sage X3 Business Partner schema layout.
/// Captures legacy ERP fields without leaking them into internal application use cases.
/// </summary>
public class SageX3BusinessPartnerDto
{
    [JsonPropertyName("BPCNUM")] public string BpCode { get; set; } = string.Empty;
    [JsonPropertyName("BPCNAM")] public string CompanyName { get; set; } = string.Empty;
    [JsonPropertyName("CNTACT")] public string ContactName { get; set; } = string.Empty;
    [JsonPropertyName("BPCAST")] public string StatusFlag { get; set; } = "Active";
    [JsonPropertyName("CRDLMT")] public decimal CreditLimitAmount { get; set; }
    [JsonPropertyName("CURBAL")] public decimal CurrentBalanceDue { get; set; }
    [JsonPropertyName("REPIDE")] public string SalesRepIdentifier { get; set; } = string.Empty;
}

/// <summary>
/// Raw external Sage X3 Financial Ledger Document schema layout.
/// </summary>
public class SageX3DocumentDto
{
    [JsonPropertyName("NUM")] public string DocumentId { get; set; } = string.Empty;
    [JsonPropertyName("TYP")] public string DocumentType { get; set; } = string.Empty;
    [JsonPropertyName("DAT")] public DateTime CreationDate { get; set; }
    [JsonPropertyName("AMT")] public decimal TotalGrossAmount { get; set; }
    [JsonPropertyName("CUR")] public string CurrencyCode { get; set; } = "USD";
    [JsonPropertyName("STA")] public string LifecycleStatus { get; set; } = "Unpaid";
    [JsonPropertyName("BLB")] public string AssociatedBlobKey { get; set; } = string.Empty;
}
