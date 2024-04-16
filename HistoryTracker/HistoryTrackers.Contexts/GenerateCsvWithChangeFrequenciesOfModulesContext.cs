
using Domain;
using HistoryTracker.Contexts.Base;
using System.Web;

namespace HistoryTracker.Contexts
{
    public class GenerateCsvWithChangeFrequenciesOfModulesContext
    {
        private readonly IGenerateCsvWithChangeFrequenciesOfModulesGateway _gateway;
        private readonly CloneRepositoryContext _cloneRepositoryContext;
        private readonly CreateLogFileContext _createLogFileContext;
        private readonly ReadLogFileContext _readLogFileContext;
        private readonly ExtractAllCommitsContext _extractAllCommitsContext;

        public GenerateCsvWithChangeFrequenciesOfModulesContext(IGenerateCsvWithChangeFrequenciesOfModulesGateway gateway, CloneRepositoryContext cloneRepositoryContext,
            CreateLogFileContext createLogFileContext, ReadLogFileContext readLogFileContext,
            ExtractAllCommitsContext extractAllCommitsContext)
        {
            _gateway = gateway;
            _cloneRepositoryContext = cloneRepositoryContext;
            _createLogFileContext = createLogFileContext;
            _readLogFileContext = readLogFileContext;
            _extractAllCommitsContext = extractAllCommitsContext;
        }

        public GetChangeFrequenciesOfModulesResponse Execute(string repositoryUrl)
        {
            if (!String.IsNullOrWhiteSpace(repositoryUrl))
            {
                repositoryUrl = HttpUtility.UrlDecode(repositoryUrl);
                var cloneRepositoryResponse = _cloneRepositoryContext.Execute(repositoryUrl);

                if (cloneRepositoryResponse.IsSuccess)
                {
                    var createLogFileResponse =
                        _createLogFileContext.Execute(repositoryUrl, cloneRepositoryResponse.ClonedRepositoryPath);
                    if (createLogFileResponse.IsSuccess)
                    {
                        var readLogFileResponse = _readLogFileContext.Execute(createLogFileResponse.LogFilePath);
                        if (readLogFileResponse.IsSuccess)
                        {
                            var extractAllCommitsResponse = _extractAllCommitsContext.Execute(readLogFileResponse.LogFileContent);
                            var revisionsOfModules = GetChangeFrequencies(extractAllCommitsResponse);
                            var response = _gateway.CreateCsvFileWithChangeFrequenciesOfModules(revisionsOfModules,cloneRepositoryResponse.ClonedRepositoryPath);
                            return new GetChangeFrequenciesOfModulesResponse { IsSuccess = true, Revisions = revisionsOfModules };
                        }
                        return new GetChangeFrequenciesOfModulesResponse
                            { IsSuccess = false, Error = readLogFileResponse.Error };
                    }
                    return new GetChangeFrequenciesOfModulesResponse
                        { IsSuccess = false, Error = createLogFileResponse.Error };
                }
                return new GetChangeFrequenciesOfModulesResponse
                    { IsSuccess = false, Error = cloneRepositoryResponse.Error };
            }

            return new GetChangeFrequenciesOfModulesResponse { IsSuccess = false, Error = "Repository url is empty!" };
        }

        private Dictionary<string, int> GetChangeFrequencies(ExtractAllCommitsResponse commits)
        {
            var entitiesChangedCount = new Dictionary<string, int>();
            var uniqueEntities = new List<string>();

            foreach (var commit in commits.Commits)
            {
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
            var sortedEntitiesChangedCount = SortDictionaryInDescendingOrder(entitiesChangedCount);
            return sortedEntitiesChangedCount;
        }

        private Dictionary<string, int> SortDictionaryInDescendingOrder(Dictionary<string, int> dictionary)
        {
            var sortedDictionary = new Dictionary<string, int>();
            foreach (var kv in dictionary.OrderByDescending(kv => kv.Value))
            {
                var kvKeyWithoutComma = kv.Key.Replace(",", "");
                sortedDictionary.Add(kvKeyWithoutComma, kv.Value);
            }
            return sortedDictionary;
        }

        public class GetChangeFrequenciesOfModulesResponse : BaseResponse
        {
            public Dictionary<string, int> Revisions;
        }
    }
}
