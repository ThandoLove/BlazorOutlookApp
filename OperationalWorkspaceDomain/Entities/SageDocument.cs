using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceDomain.Entities;


public class SageDocument
{
    public string DocumentNumber { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty; // e.g., Invoice, Order, Quote, DeliverySlip
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Status { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = "application/pdf"; // Support universal viewing mapping
    public string InternalSageBlobId { get; private set; } = string.Empty;

    private SageDocument() { }

    public static SageDocument MapExternalRecord(
        string docNo, string type, DateTime date, decimal amount, string currency, string status, string blobId)
    {
        if (string.IsNullOrWhiteSpace(docNo)) throw new ArgumentException("Document identifier key structure must not be blank.");

        string autoMime = type?.ToUpperInvariant() switch
        {
            "DELIVERYSLIP" or "PACKINGSLIP" => "image/tiff",
            "EXCELREPORT" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/pdf"
        };

        return new SageDocument
        {
            DocumentNumber = docNo.Trim().ToUpperInvariant(),
            Type = string.IsNullOrWhiteSpace(type) ? "Invoice" : type.Trim(),
            Date = date == default ? DateTime.UtcNow : date,
            Amount = amount,
            Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant(),
            Status = string.IsNullOrWhiteSpace(status) ? "Unpaid" : status.Trim(),
            MimeType = autoMime,
            InternalSageBlobId = blobId?.Trim() ?? string.Empty
        };
    }

    public bool IsActionableFinancialDocument => Type == "Invoice" && Status != "Paid" && Amount > 0;

    public string CustomerId { get; set; }
}
