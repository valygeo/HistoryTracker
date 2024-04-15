
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
        private readonly CloneRepositoryContext _cloneRepositoryContext;

        public GetSummaryDataContext(IGetSummaryDataGateway gateway, CreateLogFileContext createLogFileContext,
            CloneRepositoryContext cloneRepositoryContext)
        {
            _gateway = gateway;
            _createLogFileContext = createLogFileContext;
            _cloneRepositoryContext = cloneRepositoryContext;
        }

        public GetSummaryDataResponse Execute(string githubUrl)
        {
            if (String.IsNullOrWhiteSpace(githubUrl))
                return new GetSummaryDataResponse { IsSuccess = false, Error = "Github url is empty!" };

            githubUrl = HttpUtility.UrlDecode(githubUrl);
            var cloneRepositoryResponse = _cloneRepositoryContext.Execute(githubUrl);

            if (cloneRepositoryResponse.IsSuccess)
            {
                var createLogFileResponse = _createLogFileContext.Execute(githubUrl, cloneRepositoryResponse.ClonedRepositoryPath);

                if (createLogFileResponse.IsSuccess)
                {
                    var result = _gateway.ReadFile(createLogFileResponse.LogFilePath); 
                    var commits =  ExtractCommits(result);
                    var statistics = GetStatistics(commits);
                    return new GetSummaryDataResponse { IsSuccess = true, Statistics = statistics, Commits = commits};
                }
                return new GetSummaryDataResponse { IsSuccess = false, Error = "Creation of the log file failed!" };
            }
         
            {
                var isRepositoryUpToDate = _gateway.IsRepositoryUpToDate(cloneRepositoryResponse.ClonedRepositoryPath);

                if (!isRepositoryUpToDate)
                {
                    var fetchChangesResponse = _gateway.FetchChanges(cloneRepositoryResponse.ClonedRepositoryPath);

                    if (!fetchChangesResponse)
                        return new GetSummaryDataResponse
                            { IsSuccess = false, Error = "Error trying to fetch changes!" };
                    {
                        var createLogFileResponse = _createLogFileContext.Execute(githubUrl, cloneRepositoryResponse.ClonedRepositoryPath);
                        if (createLogFileResponse.IsSuccess)
                        {
                            var result = _gateway.ReadFile(createLogFileResponse.LogFilePath);
                            var commits = ExtractCommits(result);
                            var statistics = GetStatistics(commits);
                            return new GetSummaryDataResponse {IsSuccess = true, Statistics = statistics, Commits = commits};
                        }
                    }
                }

                if (isRepositoryUpToDate)
                {
                    var logFilePath = GetLogFilePath(githubUrl, cloneRepositoryResponse.ClonedRepositoryPath);
                    if (!File.Exists(logFilePath))
                    {
                        var createLogFileResponse = _createLogFileContext.Execute(githubUrl, cloneRepositoryResponse.ClonedRepositoryPath);
                        if (createLogFileResponse.IsSuccess)
                        {
                            var result = _gateway.ReadFile(createLogFileResponse.LogFilePath);
                            var commits = ExtractCommits(result);
                            var statistics = GetStatistics(commits);
                            return new GetSummaryDataResponse { IsSuccess = true, Statistics = statistics, Commits = commits };
                        }
                    }

                    {
                        var result = _gateway.ReadFile(logFilePath);
                        var commits = ExtractCommits(result);
                        var statistics = GetStatistics(commits);
                        return new GetSummaryDataResponse
                            { IsSuccess = true, Statistics = statistics, Commits = commits };
                    }
                }
            }
            return new GetSummaryDataResponse { IsSuccess = false, Error = "Error trying to retrieve data!" };
        }
        private string GetLogFilePath(string githubUrl, string clonedRepositoryPath)
        {
            var repositoryName = Path.GetFileNameWithoutExtension(new Uri(githubUrl).AbsolutePath.TrimStart('/'));
            var logFilePath = Path.Combine(clonedRepositoryPath, $"{repositoryName}.log");
            return logFilePath;
        }

        private Statistics GetStatistics(ICollection<Commit> commits)
        {
            var response = new Statistics();
            var authors = new List<string>();
            var uniqueEntities = new List<string>();
            var entitiesChangedCount = new Dictionary<string, int>();
            foreach (var commit in commits)
            {
                if(!String.IsNullOrWhiteSpace(commit.Author) && !authors.Contains(commit.Author))
                    authors.Add(commit.Author);

                foreach (var commitDetail in commit.CommitDetails)
                {
                    if (!String.IsNullOrWhiteSpace(commitDetail.EntityChangedName) &&
                        !uniqueEntities.Contains(commitDetail.EntityChangedName))
                    {
                        uniqueEntities.Add(commitDetail.EntityChangedName);
                        entitiesChangedCount[commitDetail.EntityChangedName] = 0; 
                    }
                    
                    if (!String.IsNullOrWhiteSpace(commitDetail.EntityChangedName) && uniqueEntities.Contains(commitDetail.EntityChangedName))

                    {
                        entitiesChangedCount[commitDetail.EntityChangedName]++;
                    }
                        
                }
            }
            response.NumberOfCommits = commits.Count;
            response.NumberOfAuthors = authors.Count;
            response.NumberOfEntities = uniqueEntities.Count;
            response.NumberOfEntitiesChanged = entitiesChangedCount.Values.Sum();
            return response;
        }

        private ICollection<Commit> ExtractCommits(ICollection<string> result)
        {
            List<Commit> commits = new List<Commit>();
            var commitToAdd = new Commit();

            for (int i = 0; i < result.Count; i++)
            {
                var line = result.ElementAt(i);
                if (line.StartsWith("["))
                {
                    commitToAdd = new Commit();
                    string[] parts = line.Split(' ', 4);
                    if (parts.Length >= 4)
                    {
                        commitToAdd.Id = parts[0];
                        commitToAdd.Author = parts[1];
                        commitToAdd.CommitDate = parts[2];
                        commitToAdd.Message = parts[3];
                    }

                    if (i + 1 < result.Count)
                    {
                        var nextLine = result.ElementAt(i + 1);
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

            return commits;
        }
    }


    public class GetSummaryDataResponse : BaseResponse
    {
        public Statistics Statistics { get; set; } = new Statistics();
        public ICollection<Commit> Commits { get; set; }
    }
}
