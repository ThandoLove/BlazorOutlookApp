using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Abstractions;

public interface ISecuritySanitizer
{
    string SanitizeUntrustedHtml(string rawInput);
    string SanitizePlainString(string rawInput);
}
