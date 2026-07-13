using OperationalWorkspaceApplication.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Services;

public interface IPremiumWorkspaceEngine
{
    string CompileLocalScenarioBriefText(CustomerDto customer, List<SageDocumentDto> docs);
    string CreateAutomatedDepartmentReplyDraft(CustomerDto? customer, string queryBody, string departmentRole);
}

public class PremiumWorkspaceEngine : IPremiumWorkspaceEngine
{
    public string CompileLocalScenarioBriefText(CustomerDto customer, List<SageDocumentDto> docs)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"• ACCOUNT BRIEF: Linked matching business profile for {customer.CompanyName}.");

        int overdueItems = docs.Count(d => d.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase));
        if (overdueItems > 0)
        {
            stringBuilder.AppendLine($"• CREDIT CRITICAL WARNING: System detected {overdueItems} critically overdue invoice tracking rows matching Ledger records.");
        }
        else
        {
            stringBuilder.AppendLine("• BUSINESS HEALTH: Account payment tracking histories are registering normal operational clear bands.");
        }

        stringBuilder.AppendLine($"• ACTION DIRECTION: Leverage integrated departmental tool assets below to process mail attachments or file followups.");
        return stringBuilder.ToString();
    }

    public string CreateAutomatedDepartmentReplyDraft(CustomerDto? customer, string queryBody, string departmentRole)
    {
        // Enforce localized structural semantic token generation optimization loops
        if (customer == null)
        {
            return "Dear Client,\n\nThank you for reaching out. We have logged your enquiry into our CRM system, and an operations specialist will contact you shortly to confirm business details.\n\nBest regards,\nThe Operations Hub";
        }

        return departmentRole.ToUpperInvariant() switch
        {
            "FINANCE" => $"Dear Team,\n\nRegarding your invoice query balance parameters, our Sage X3 ledger statement indicates an open balance of {customer.BalanceDue:C} against your account. Please confirm processing timelines before release authorizations clear.\n\nBest regards,\nFinance Discrepancy Control",
            "SALES" => $"Dear {customer.ContactName},\n\nThank you for checking in with our project team! We have successfully tracked your message notes and are preparing updated matching pricing catalogs bound to your account values.\n\nBest regards,\nEnterprise Account Management",
            _ => $"Dear Client,\n\nWe have successfully received your mail inquiry context and logged an operational tracking ticket under your partner account code {customer.Id}.\n\nBest regards,\nTechnical Operations Team"
        };
    }
}
