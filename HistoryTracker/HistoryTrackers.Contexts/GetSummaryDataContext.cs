
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
                List<EntityRows> entityRows = new List<EntityRows>();

                foreach (string line in result)
                {
                    string pattern = @"\[([^\]]+)\]\s+(\S+)\s+(\d{4}-\d{2}-\d{2})\s+(.*?)\s*(?=\[|$)";
                    // Regular expression explication : [([^\]]+)\] - any string array into the [] , in our case this is the commit id which is a sha 256 encrypted
                    //  \s - one or many white spaces or tabs newlines etc
                    //  (\S+) - one or many characters which aren't white spaces or tabs
                    // d{4}-\d{2}-\d{2} - string char for a date in format yyyy-mm-dd
                    // 

                    Match match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        commitToAdd.Id = match.Groups[1].Value;
                        commitToAdd.Author = match.Groups[2].Value;
                        commitToAdd.CommitDate = match.Groups[3].Value;
                        commitToAdd.Message = match.Groups[4].Value;
                    }

                    string changePattern = @"\b(\d+)\t(\d+)\t(.+?)\b";
                    MatchCollection matchesForEntityChanged = Regex.Matches(line, changePattern);

                    foreach (Match matchForEntityChanged in matchesForEntityChanged)
                    {
                        entityRows.Add(ParseEntityRows(matchForEntityChanged));
                    }

                    if (String.IsNullOrWhiteSpace(line))
                    {
                        var commitToBeAdded = new Commit
                        {
                            Id = commitToAdd.Id,
                            Author = commitToAdd.Author,
                            CommitDate = commitToAdd.CommitDate,
                            Message = commitToAdd.Message,
                            EntityChanged = entityRows
                        };

                        commits.Add(commitToBeAdded);
                        entityRows = new List<EntityRows>();
                    }
                }
                
                return new GetSummaryDataResponse { Commits = commits };
            }

            return new GetSummaryDataResponse { IsSuccess = false, Error = "Creation of the log file failed!" };
        }

        private EntityRows ParseEntityRows(Match match)
        {
            EntityRows entityRow = new EntityRows(0,0,"");
                int addedLine, removedLine;
                if (int.TryParse(match.Groups[1].Value, out addedLine) &&
                    int.TryParse(match.Groups[2].Value, out removedLine))
                {
                    string entityChangedPath = match.Groups[3].Value;
                     entityRow = new EntityRows(addedLine, removedLine, entityChangedPath);
                     entityRow.EntityName = entityChangedPath;
                     entityRow.RowsAdded = addedLine;
                     entityRow.RowsDeleted = removedLine;
                }
           
            return entityRow;
        }
    }
    public class GetSummaryDataResponse : BaseResponse
    {
        public ICollection<string> FileContent { get; set; }
        public ICollection<SummaryData> SummaryData { get; set; }
        public ICollection<Commit> Commits { get; set; }
    }
}
