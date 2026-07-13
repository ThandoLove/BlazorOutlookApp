using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceDomain.Entities;

using System;

namespace OperationalWorkspaceApplication.Mappers;


public static class IdentityModelMapper
{
    public static CustomerDto MapToDto(Customer entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new CustomerDto(
            entity.Id,
            entity.CompanyName,
            entity.ContactName,
            entity.Email,
            entity.AccountStatus,
            entity.CreditLimit,
            entity.BalanceDue,
            entity.AvailableCredit,
            entity.IsHighRiskExposure
        );
    }

    public static SageDocumentDto MapToDto(SageDocument entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new SageDocumentDto(
            entity.DocumentNumber,
            entity.Type,
            entity.Date,
            entity.Amount,
            entity.Currency,
            entity.Status,
            entity.MimeType,
            entity.InternalSageBlobId,
            entity.IsActionableFinancialDocument
        );
    }

    public static TicketDto MapToDto(OperationalTicket entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new TicketDto(
            entity.Id,
            entity.CustomerId,
            entity.Subject,
            entity.Description,
            entity.Priority,
            entity.Status,
            entity.CreatedAt
        );
    }
}
