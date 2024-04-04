
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
    }
}
