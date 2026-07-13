using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Requests.CustomerRequest;


public record GetCustomerContextQuery(string SenderEmail, string SenderName, string ActiveUserEmail, string NetworkIp);
