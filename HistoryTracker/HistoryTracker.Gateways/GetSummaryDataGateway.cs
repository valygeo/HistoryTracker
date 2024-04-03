
using System.Diagnostics;
using Domain;
using Domain.Entities;

namespace HistoryTracker.Gateways
{
    public class GetSummaryDataGateway : IGetSummaryDataGateway
    {
        public void GetSummaryData(string githubUrl)
        {
            githubUrl = "https://github.com/spescarus/app-stagiatura-2023.git";

            string currentDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FolderNou");
            // daca nu exista deja clonat sa il cloneze daca nu il lasam asa TODO
   
            var cloneProcess = new Process();
            cloneProcess.StartInfo.FileName = "git";
            cloneProcess.StartInfo.Arguments = $"clone {githubUrl}";
            cloneProcess.StartInfo.WorkingDirectory = currentDirectory ;
            cloneProcess.Start();
            cloneProcess.WaitForExit();

            if (cloneProcess.ExitCode != 0)
            {
                Console.WriteLine("Error");
                return;
            }

            {
                var command = "git log --pretty=format:\"[%h] %an %ad %s\" --date=short --numstat";
                var process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c{command} > file.log"; 
                string directoryCreated = Path.Combine(currentDirectory, "app-stagiatura-2023");
                process.StartInfo.WorkingDirectory = directoryCreated ;
                process.Start();
                process.WaitForExit();
            }

        }
    }
}
