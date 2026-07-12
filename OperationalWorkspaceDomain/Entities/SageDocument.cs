using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceDomain.Entities;

public class SageDocument
{
    public string DocumentNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., Invoice, Order, Quote
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}
