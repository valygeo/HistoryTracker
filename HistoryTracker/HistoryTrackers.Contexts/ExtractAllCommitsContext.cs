
using System.Text.RegularExpressions;
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class ExtractAllCommitsContext
    {
        private readonly IExtractAllCommitsGateway _extractAllCommitsGateway;

        public ExtractAllCommitsContext(IExtractAllCommitsGateway extractAllCommitsGateway)
        {
            _extractAllCommitsGateway = extractAllCommitsGateway;
        }
        public ExtractAllCommitsResponse Execute(string logFilePath)
        {
            var logFileContent = _extractAllCommitsGateway.ReadLogFile(logFilePath);
            List<Commit> commits = new List<Commit>();
            var commitToAdd = new Commit();

            for (int i = 0; i < logFileContent.Count; i++)
            {
                var line = logFileContent.ElementAt(i);
                if (line.StartsWith("["))
                {
                    commitToAdd = new Commit();
                    string[] parts = line.Split(' ', 4);
                    if (parts.Length >= 4)
                    {
                        if (IsDate(parts[2]))
                        {
                            commitToAdd.Id = parts[0];
                            commitToAdd.Author = parts[1];
                            commitToAdd.CommitDate = parts[2];
                            commitToAdd.Message = parts[3];
                        }
                        else
                        {
                            string[] partsForMultipleNameOfAuthor = line.Split(' ', 5);
                            commitToAdd.Id = partsForMultipleNameOfAuthor[0];
                            commitToAdd.Author = partsForMultipleNameOfAuthor[1] + partsForMultipleNameOfAuthor[2];
                            commitToAdd.CommitDate = partsForMultipleNameOfAuthor[3];
                            commitToAdd.Message = partsForMultipleNameOfAuthor[4];
                        }
                    }

                    if (i + 1 < logFileContent.Count)
                    {
                        var nextLine = logFileContent.ElementAt(i + 1);
                        if (nextLine.StartsWith("["))
                        {
                            commits.Add(commitToAdd);
                            commitToAdd = new Commit();
                        }
                    }
                }

                else if (!String.IsNullOrWhiteSpace(line) && char.IsDigit(line[0]))
                {
                    var commitDetails = new CommitDetails();
                    string[] parts = line.Split('\t', 3);

                    if (parts.Length >= 3)
                    {
                        commitDetails.RowsAdded = int.Parse(parts[0]);
                        commitDetails.RowsDeleted = int.Parse(parts[1]);
                        commitDetails.EntityChangedName = parts[2];
                        commitToAdd.CommitDetails.Add(commitDetails);
                    }
                }

                else if (String.IsNullOrWhiteSpace(line))
                {
                    commits.Add(commitToAdd);
                }
            }

            if (!commits.Contains(commitToAdd))
                commits.Add(commitToAdd);

            return new ExtractAllCommitsResponse {IsSuccess = true, Commits = commits};
        }

        private static bool IsDate(string date)
        {
            var regex = "^\\d{4}-\\d{2}-\\d{2}$";
            return Regex.IsMatch(date, regex);
        }
    }

    public class ExtractAllCommitsResponse : BaseResponse
    {
        public ICollection<Commit> Commits { get; set; }
    }
}
