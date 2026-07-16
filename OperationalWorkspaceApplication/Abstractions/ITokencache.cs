using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Abstractions;
public interface ITokenCache
{
    bool TryGet(string key, out string? value);
    void Set(string key, string value, TimeSpan expiration);
    void Remove(string key);
}