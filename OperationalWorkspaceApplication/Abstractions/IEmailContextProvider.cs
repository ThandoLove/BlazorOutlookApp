using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Abstractions;
public interface IEmailContextProvider
{
    string ExtractMessageIdFromHeader(IDictionary<string, string> headers);
    string SanitizeEmailBodyPayload(string rawHtmlBody);
}