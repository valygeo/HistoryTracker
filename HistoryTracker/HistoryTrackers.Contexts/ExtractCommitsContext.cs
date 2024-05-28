
using System.Text.RegularExpressions;
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class ExtractCommitsContext
    {
        private readonly IExtractAllCommitsGateway _extractAllCommitsGateway;

        public ExtractCommitsContext(IExtractAllCommitsGateway extractAllCommitsGateway)
        {
            _extractAllCommitsGateway = extractAllCommitsGateway;
        }
        public ExtractCommitsResponse Execute(string logFilePath)
        {
            var logFileContent = _extractAllCommitsGateway.ReadLogFile(logFilePath);
            Dictionary<string,Commit1> commits = new Dictionary<string,Commit1>();
            var commitToAdd = new Commit1();
            var commitId = "";
            for (int i = 0; i < logFileContent.Count; i++)
            {
                var line = logFileContent.ElementAt(i);
                if (line.StartsWith("["))
                {
                    commitToAdd = new Commit1();
                    string[] parts = line.Split(' ', 4);
                    if (parts.Length >= 4)
                    {
                        if (IsDate(parts[2]))
                        {
                            commitId = parts[0];
                            commitToAdd.Author = parts[1];
                            commitToAdd.CommitDate = parts[2];
                            commitToAdd.Message = parts[3];
                        }
                        else
                        {
                            string[] partsForMultipleNameOfAuthor = line.Split(' ', 5);
                            commitId = partsForMultipleNameOfAuthor[0];
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
                            commits.Add(commitId,commitToAdd);
                            commitToAdd = new Commit1();
                            commitId = "";
                        }
                    }
                }

                else if (!String.IsNullOrWhiteSpace(line) && char.IsDigit(line[0]))
                {
                    var commitDetails = new CommitDetails1();
                    string[] parts = line.Split('\t', 3);

                    if (parts.Length >= 3)
                    {
                        commitDetails.RowsAdded = int.Parse(parts[0]);
                        commitDetails.RowsDeleted = int.Parse(parts[1]);
                        var entityChangedName = parts[2];
                        commitToAdd.CommitDetails.Add(entityChangedName,commitDetails);
                    }
                }

                else if (String.IsNullOrWhiteSpace(line))
                {
                    commits.Add(commitId, commitToAdd);
                }
            }

            if (!commits.ContainsKey(commitId))
                commits.Add(commitId, commitToAdd);

            return new ExtractCommitsResponse {IsSuccess = true, Commits = commits};
        }

        private static bool IsDate(string date)
        {
            var regex = "^\\d{4}-\\d{2}-\\d{2}$";
            return Regex.IsMatch(date, regex);
        }
    }

    public class ExtractCommitsResponse : BaseResponse
    {
        //public ICollection<Commit> Commits { get; set; }
        public Dictionary<string,Commit1> Commits { get; set; }
    }
}
