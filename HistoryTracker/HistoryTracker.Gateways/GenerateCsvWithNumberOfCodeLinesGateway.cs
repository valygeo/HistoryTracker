
using System.Diagnostics;
using Domain;


namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithNumberOfCodeLinesGateway : IGenerateCsvWithNumberOfCodeLinesGateway
    {
        public string GenerateCsvWithNumberOfCodeLines(string repositoryPath, string clocPath)
        {
            clocPath = Path.GetFullPath(clocPath);
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
            var csvFileName = $"{repositoryName}_counting_lines.csv";
            var csvFilePath = Path.Combine(repositoryPath, csvFileName);

            using (var process = new Process())
            {
                process.StartInfo = generateCsvProcessStartInfo;
                process.Start();
                process.StandardInput.WriteLine($"{clocPath} ./ --by-file --csv --quiet --exclude-dir=docs,internals,node_modules,server --report-file={csvFileName}");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    return "";
                }
                return csvFilePath;
            }
        }

    }
}
