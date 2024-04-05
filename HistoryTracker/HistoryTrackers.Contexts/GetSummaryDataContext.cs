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

                foreach (string line in result)
                {
                   
                    string pattern = @"\[([^\]]+)\]\s+(\S+)\s+(\d{4}-\d{2}-\d{2})\s+(.*?)\s*(?=\[|$)";

                   
                    Match match = Regex.Match(line, pattern);

                    if (match.Success)
                    {
                        string commitId = match.Groups[1].Value;
                        string commitAuthor = match.Groups[2].Value;
                        string commitDate = match.Groups[3].Value;
                        string commitMessage = match.Groups[4].Value;

                        Commit commit = new Commit(commitId, commitAuthor, commitDate);
                        commits.Add(commit);
                    }
                }

                return new GetSummaryDataResponse { Commits = commits };
            }

            return new GetSummaryDataResponse { IsSuccess = false, Error = "Creation of the log file failed!" };
        }

    }
    public class GetSummaryDataResponse : BaseResponse
    {
        public ICollection<string> FileContent { get; set; }
        public ICollection<SummaryData> SummaryData { get; set; }
        public ICollection<Commit> Commits { get; set; }
    }
}
