
using Domain;
using System.Diagnostics;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class CreateLogFileFromSpecifiedPeriodGateway : ICreateLogFileFromSpecifiedPeriodGateway
    {
        public string CreateLogFile(CreateLogFileFromSpecifiedPeriodData createLogFileRequest)
        {
            var command = $"git log --pretty=format:\"[%h] %an %ad %s\" --before={createLogFileRequest.periodStartDate} --after={createLogFileRequest.periodEndDate} --date=short --numstat";
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {command} > {createLogFileRequest.repositoryName}.log";
            process.StartInfo.WorkingDirectory = createLogFileRequest.clonedRepositoryPath;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            var logFilePath = Path.Combine(createLogFileRequest.clonedRepositoryPath, $"{createLogFileRequest.repositoryName}.log");
            if (process.ExitCode != 0 || !File.Exists(logFilePath))
                return "";
            return logFilePath;
        }
        public bool IsRepositoryUpToDate(string repositoryClonedPath)
        {
            var verifyRepositoryProcessStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "fetch",
                WorkingDirectory = repositoryClonedPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(verifyRepositoryProcessStartInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    return false;
                }
            }

            verifyRepositoryProcessStartInfo.Arguments = "status";

            using (var processForStatus = Process.Start(verifyRepositoryProcessStartInfo))
            {
                string output = processForStatus.StandardOutput.ReadToEnd();
                processForStatus.WaitForExit();
                if (output.Contains("Your branch is up to date"))
                    return true;
                return false;
            }
        }

        public bool FetchChanges(string repositoryClonedPath)
        {
            var fetchChangesProcessStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "pull",
                WorkingDirectory = repositoryClonedPath,
                CreateNoWindow = true
            };
            using (var process = Process.Start(fetchChangesProcessStartInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool LogFileAlreadyExists(string logFilePath)
        {
            if (File.Exists(logFilePath))
                return true;
            return false;
        }
    }
}

