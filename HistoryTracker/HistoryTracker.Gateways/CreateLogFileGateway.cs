
using Domain;
using System.Diagnostics;

namespace HistoryTracker.Gateways
{
    public class CreateLogFileGateway : ICreateLogFileGateway
    {
        public string CreateLogFile(string githubUrl, string clonedRepositoryPath)
        {
            var repositoryName = Path.GetFileNameWithoutExtension(new Uri(githubUrl).AbsolutePath.TrimStart('/'));
            var command = "git log --pretty=format:\"[%h] %an %ad %s\" --date=short --numstat";
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {command} > {repositoryName}.log";
            process.StartInfo.WorkingDirectory = clonedRepositoryPath;
            process.Start();
            process.WaitForExit();
            
            var logFilePath = Path.Combine(clonedRepositoryPath, $"{repositoryName}.log");
            if (process.ExitCode != 0 || !File.Exists(logFilePath))
                return ""; 
            return logFilePath; 
        }
    }
}
