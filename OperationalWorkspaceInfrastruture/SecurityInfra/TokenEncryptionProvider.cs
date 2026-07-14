using System;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace OperationalWorkspaceInfrastructure.SecurityInfra;

public interface ITokenEncryptionProvider
{
    string ExtractSecureMachineCredential(string environmentalKey);
    string ProtectSensitivePayload(string rawText);
    string UnprotectSensitivePayload(string cipherText);
}

public class TokenEncryptionProvider : ITokenEncryptionProvider
{
    public string ExtractSecureMachineCredential(string environmentalKey)
    {
        // Guard against repository secret leaks by forcing runtime variable ingestion
        var secureVal = Environment.GetEnvironmentVariable(environmentalKey, EnvironmentVariableTarget.Machine)
                     ?? Environment.GetEnvironmentVariable(environmentalKey, EnvironmentVariableTarget.Process);

        if (string.IsNullOrWhiteSpace(secureVal))
            throw new InvalidOperationException($"Critical system secret token parameter '{environmentalKey}' missing from host infrastructure variables.");

        return secureVal.Trim();
    }

    // 1. Explicit OS attribute guard tells the compiler compiler analyzer this method runs on Windows architectures
    [SupportedOSPlatform("windows")]
    public string ProtectSensitivePayload(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return string.Empty;

        // Enforce Windows OS-level Data Protection API (DPAPI) to protect cached memory payloads safely
        var rawBytes = Encoding.UTF8.GetBytes(rawText);
        var encryptedBytes = ProtectedData.Protect(rawBytes, null, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(encryptedBytes);
    }

    // 2. Explicit OS attribute guard silences the Unprotect platform check warning matching DPAPI scopes
    [SupportedOSPlatform("windows")]
    public string UnprotectSensitivePayload(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        var rawEncrypted = Convert.FromBase64String(cipherText);
        var decryptedBytes = ProtectedData.Unprotect(rawEncrypted, null, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
