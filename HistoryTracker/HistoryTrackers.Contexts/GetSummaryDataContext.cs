
using System.Text.RegularExpressions;
using System.Web;
using Domain;
using Domain.Entities;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class GetSummaryDataContext
    {
        private readonly IGetSummaryDataGateway _gateway;
        private readonly CreateLogFileContext _createLogFileContext;

        public GetSummaryDataContext(IGetSummaryDataGateway gateway, CreateLogFileContext createLogFileContext)
        {
            _gateway = gateway;
            _createLogFileContext = createLogFileContext;
        }

        public GetSummaryDataResponse Execute(string githubUrl)
        {
            if (String.IsNullOrWhiteSpace(githubUrl))
                return new GetSummaryDataResponse { IsSuccess = false, Error = "Github url is empty!" };

            githubUrl = HttpUtility.UrlDecode(githubUrl);
            var createLogFileResponse = _createLogFileContext.Execute(githubUrl);

            if (createLogFileResponse.IsSuccess)
            {
                var result = _gateway.GetSummaryData(createLogFileResponse.LogFilePath);

                List<Commit> commits = new List<Commit>();
                var commitToAdd = new Commit();
                List<CommitDetails> commitDetails = new List<CommitDetails>();

                foreach (string line in result)
                {
                    string pattern = @"\[([^\]]+)\]\s+(\S+)\s+(\d{4}-\d{2}-\d{2})\s+(.*?)\s*(?=\[|$)";
                    // Regular expression explanation : [([^\]]+)\] - any string array into the [] , in our case this is the commit id which is a sha 256 encrypted
                    //  \s - one or many white spaces or tabs newlines etc
                    //  (\S+) - one or many characters which aren't white spaces or tabs
                    // d{4}-\d{2}-\d{2} - string char for a date in format yyyy-mm-dd
                    // (.?) - any text or special characters * (our commit message) - meaning 0 or more and ? - non-greedy experssion - to stop at the first match of the next component
                    // (?=[|$) - used to check if the next character is either "[" or the end of the string
                    Match match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        commitToAdd.Id = match.Groups[1].Value;
                        commitToAdd.Author = match.Groups[2].Value;
                        commitToAdd.CommitDate = match.Groups[3].Value;
                        commitToAdd.Message = match.Groups[4].Value;
                        if (IsMergeCommit(commitToAdd.Message))
                            AddMergeCommit(commitToAdd, commits);
                    }
                    
                    string changesPattern = @"\b(\d+)\t(\d+)\t(.+)\b";
                    Match matchForCommitDetails = Regex.Match(line, changesPattern);
                    if (matchForCommitDetails.Success)
                    {
                        commitDetails.Add(ParseCommitDetails(matchForCommitDetails));
                    }

                    if (String.IsNullOrWhiteSpace(line))
                    {
                        AddCommitWithDetailsOfEntitiesChanged(commitToAdd, commitDetails, commits);
                        commitDetails = new List<CommitDetails>();
                    }
                   
                }
                return new GetSummaryDataResponse { Commits = commits };
            }
            return new GetSummaryDataResponse { IsSuccess = false, Error = "Creation of the log file failed!" };
        }

        private static void AddCommitWithDetailsOfEntitiesChanged(Commit commitToAdd, List<CommitDetails> commitDetails, List<Commit> commits)
        {
            var commitToBeAdded = new Commit
            {
                Id = commitToAdd.Id,
                Author = commitToAdd.Author,
                CommitDate = commitToAdd.CommitDate,
                Message = commitToAdd.Message,
                CommitDetails = commitDetails
            };
            commits.Add(commitToBeAdded);
        }
        private static void AddMergeCommit(Commit commitToAdd, List<Commit> commits)
        {
            var commitToBeAdded = new Commit
            {
                Id = commitToAdd.Id,
                Author = commitToAdd.Author,
                CommitDate = commitToAdd.CommitDate,
                Message = commitToAdd.Message,
            };
            commits.Add(commitToBeAdded);
        }

        private bool IsMergeCommit(string commitMessage)
        {
            if (commitMessage.StartsWith("Merge"))
                return true;
            return false;
        }
        private CommitDetails ParseCommitDetails(Match match)
        { 
            CommitDetails commitDetails = new CommitDetails();

            if (int.TryParse(match.Groups[1].Value, out int rowsAdded) &&
                int.TryParse(match.Groups[2].Value, out int rowsRemoved))
            {
                commitDetails.EntityChangedName = match.Groups[3].Value;
                commitDetails.RowsAdded = rowsAdded;
                commitDetails.RowsDeleted = rowsRemoved;
            }
            return commitDetails;
        }
    }
    public class GetSummaryDataResponse : BaseResponse
    {
        public ICollection<string> FileContent { get; set; }
        public ICollection<Commit> Commits { get; set; }
    }
}
