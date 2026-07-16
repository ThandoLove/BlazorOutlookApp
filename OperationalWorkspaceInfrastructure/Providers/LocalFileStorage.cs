using OperationalWorkspaceApplication.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace OperationalWorkspaceInfrastructure.Providers;

public class LocalFileStorage : IFileStorage
{
    public void EnsureDirectoryExists(string path) => Directory.CreateDirectory(path);
    public bool FileExists(string path) => File.Exists(path);
    public string ReadAllText(string path) => File.ReadAllText(path);
    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
}