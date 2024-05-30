
using Domain.MetaData;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class GetSummaryStatisticsContext
    {
        private readonly CloneRepositoryContext _cloneRepositoryContext;
        private readonly CreateAllTimeLogFileContext _createLogFileContext;
        private readonly ExtractAllCommitsContext _extractAllCommitsContext;

        public GetSummaryStatisticsContext(CloneRepositoryContext cloneRepositoryContext, CreateAllTimeLogFileContext createLogFileContext,
           ExtractAllCommitsContext extractAllCommitsContext)
        {
            _cloneRepositoryContext = cloneRepositoryContext;
            _createLogFileContext = createLogFileContext;
            _extractAllCommitsContext = extractAllCommitsContext;
        }

        public GetSummaryDataStatistics Execute(string githubUrl)
        {
            if (String.IsNullOrWhiteSpace(githubUrl))
                return new GetSummaryDataStatistics { IsSuccess = false, Error = "Github url is empty!" };

            var cloneRepositoryResponse = _cloneRepositoryContext.Execute(githubUrl);

            if (cloneRepositoryResponse.IsSuccess)
            {
                var createLogFileResponse = _createLogFileContext.Execute(cloneRepositoryResponse.ClonedRepositoryPath);

                if (createLogFileResponse.IsSuccess)
                {
                    var extractAllCommitsResponse = _extractAllCommitsContext.Execute(createLogFileResponse.LogFilePath);
                    var statistics = GetStatistics(extractAllCommitsResponse.Commits);
                    return new GetSummaryDataStatistics { IsSuccess = true, Statistics = statistics};
                }
                return new GetSummaryDataStatistics { IsSuccess = false, Error = createLogFileResponse.Error };
            }
            return new GetSummaryDataStatistics { IsSuccess = false, Error = cloneRepositoryResponse.Error };
        }

        private Statistics GetStatistics(Dictionary<string, Commit> commits)
        {
            var response = new Statistics();
            var authors = new List<string>();
            var uniqueEntities = new List<string>();
            var entitiesChangedCount = new Dictionary<string, int>();
            foreach (var commit in commits)
            {
                if(!String.IsNullOrWhiteSpace(commit.Value.Author) && !authors.Contains(commit.Value.Author))
                    authors.Add(commit.Value.Author);

                foreach (var commitDetail in commit.Value.CommitDetails)
                {
                    if (!String.IsNullOrWhiteSpace(commitDetail.Key) &&
                        !uniqueEntities.Contains(commitDetail.Key))
                    {
                        uniqueEntities.Add(commitDetail.Key);
                        entitiesChangedCount[commitDetail.Key] = 0; 
                    }
                    
                    if (!String.IsNullOrWhiteSpace(commitDetail.Key) && uniqueEntities.Contains(commitDetail.Key))

                    {
                        entitiesChangedCount[commitDetail.Key]++;
                    }
                        
                }
            }
            response.NumberOfCommits = commits.Count;
            response.NumberOfAuthors = authors.Count;
            response.NumberOfEntities = uniqueEntities.Count;
            response.NumberOfEntitiesChanged = entitiesChangedCount.Values.Sum();
            return response;
        }
    }


    public class GetSummaryDataStatistics : BaseResponse
    {
        public Statistics Statistics { get; set; } = new Statistics();
    }
}
