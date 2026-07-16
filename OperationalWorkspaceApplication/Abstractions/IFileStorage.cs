using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationalWorkspaceApplication.Abstractions;

public interface IFileStorage
{
    void EnsureDirectoryExists(string directoryPath);
    bool FileExists(string filePath);
    string ReadAllText(string filePath);
    void WriteAllText(string filePath, string content);
}