using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceDomain.Entities;


public class Customer
{
    public string Id { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string ContactName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string AccountStatus { get; private set; } = "Active"; // Active, Blocked, Review
    public decimal CreditLimit { get; private set; }
    public decimal BalanceDue { get; private set; }
    public string SalesRepCode { get; private set; } = string.Empty;

    // Domain Invariant Calculations (Non-Boilerplate Business Logic)
    public decimal AvailableCredit => CreditLimit - BalanceDue;
    public bool IsCreditExceeded => BalanceDue > CreditLimit;
    public bool IsHighRiskExposure => CreditLimit > 0 && (BalanceDue / CreditLimit) >= 0.90m;

    // Prevent direct raw instantiation without schema validation rule processing
    private Customer() { }

    public static Customer CreateHydratedRecord(
        string id, string companyName, string contactName, string email,
        string accountStatus, decimal creditLimit, decimal balanceDue, string salesRepCode)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Sage X3 BP Code identification required.");
        if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("Company corporate legal name registration required.");
        if (creditLimit < 0) throw new ArgumentException("Credit boundary thresholds cannot register negative values.");

        return new Customer
        {
            Id = id.Trim().ToUpperInvariant(),
            CompanyName = companyName.Trim(),
            ContactName = contactName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            AccountStatus = string.IsNullOrWhiteSpace(accountStatus) ? "Active" : accountStatus.Trim(),
            CreditLimit = creditLimit,
            BalanceDue = balanceDue,
            SalesRepCode = salesRepCode.Trim().ToUpperInvariant()
        };
    }

    public void EnforceStatusOverride(string status)
    {
        if (status != "Active" && status != "Blocked" && status != "Review")
            throw new InvalidOperationException($"Invalid state change assignment target: {status}");

        AccountStatus = status;
    }
}
