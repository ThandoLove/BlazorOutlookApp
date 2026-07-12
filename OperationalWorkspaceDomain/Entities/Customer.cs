using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceDomain.Entities;

public class Customer
{
    public string Id { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty; // e.g., Active, Blocked
    public decimal CreditLimit { get; set; }
    public decimal BalanceDue { get; set; }
    public string SalesRepCode { get; set; } = string.Empty;
}

