using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.AppSecurity;


public interface ITokenEncryptionProvider
{
    string ExtractSecureMachineCredential(string environmentalKey);
    string ProtectSensitivePayload(string rawText);
    string UnprotectSensitivePayload(string cipherText);
   
}
