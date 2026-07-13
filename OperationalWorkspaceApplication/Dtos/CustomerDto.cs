using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Dtos;


public record CustomerDto(
    string Id,
    string CompanyName,
    string ContactName,
    string Email,
    string AccountStatus,
    decimal CreditLimit,
    decimal BalanceDue,
    decimal AvailableCredit,
    bool IsRiskExposureExceeded
);
