
using System;
using OperationalWorkspace.Infrastructure.SageX3.X3Dtos;
using OperationalWorkspaceDomain.Entities;


namespace OperationalWorkspaceInfrastructure.SageX3.X3Mappers;

public static class SageMapper
{
    public static Customer MapToDomain(SageX3BusinessPartnerDto externalDto, string email)
    {
        ArgumentNullException.ThrowIfNull(externalDto);

        // Maps fragile, cryptic ERP keys (BPCNUM, BPCNAM) to clean internal Domain parameters
        return Customer.CreateHydratedRecord(
            externalDto.BpCode,
            externalDto.CompanyName,
            externalDto.ContactName,
            email,
            externalDto.StatusFlag,
            externalDto.CreditLimitAmount,
            externalDto.CurrentBalanceDue,
            externalDto.SalesRepIdentifier
        );
    }

    public static SageDocument MapToDomain(SageX3DocumentDto externalDto)
    {
        ArgumentNullException.ThrowIfNull(externalDto);

        return SageDocument.MapExternalRecord(
            externalDto.DocumentId,
            externalDto.DocumentType,
            externalDto.CreationDate,
            externalDto.TotalGrossAmount,
            externalDto.CurrencyCode,
            externalDto.LifecycleStatus,
            externalDto.AssociatedBlobKey
        );
    }
}
