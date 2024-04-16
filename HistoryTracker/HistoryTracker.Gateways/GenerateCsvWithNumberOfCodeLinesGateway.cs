
using System.Diagnostics;
using Domain;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithNumberOfCodeLinesGateway : IGenerateCsvWithNumberOfCodeLinesGateway
    {
        public bool GenerateCsvWithNumberOfCodeLines(string repositoryPath)
        {
            var generateCsvProcessStartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = repositoryPath
            };
            var repositoryName = Path.GetFileNameWithoutExtension(repositoryPath);

            using (var process = new Process())
            {
                process.StartInfo = generateCsvProcessStartInfo;
                process.Start();
                process.StandardInput.WriteLine($"cloc {repositoryPath} --by-file --csv --quiet --report-file=countingLines.csv");
                process.StandardInput.Flush();
                process.StandardInput.Close();
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
