
using Domain;
using System.Diagnostics;

namespace HistoryTracker.Gateways
{
    public class CloneRepositoryGateway : ICloneRepositoryGateway
    {
        public bool CloneRepository(string githubUrl, string directoryPathWhereRepositoryWillBeCloned)
        {
            if (!Directory.Exists(directoryPathWhereRepositoryWillBeCloned))
                Directory.CreateDirectory(directoryPathWhereRepositoryWillBeCloned);
            {
                var cloneProcess = new Process();
                cloneProcess.StartInfo.FileName = "git";
                cloneProcess.StartInfo.Arguments = $"clone {githubUrl}";
                cloneProcess.StartInfo.WorkingDirectory = directoryPathWhereRepositoryWillBeCloned;
                cloneProcess.Start();
                cloneProcess.WaitForExit();

                if (cloneProcess.ExitCode != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsRepositoryUpToDate(string repositoryClonedPath)
        {
            var verifyRepositoryProcessStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "fetch",
                WorkingDirectory = repositoryClonedPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true
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
                WorkingDirectory = repositoryClonedPath
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
    }
}
