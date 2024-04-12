

using Domain.Entities;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class GetChangeFrequenciesOfModulesContext
    {
        private readonly GetSummaryDataContext _summaryDataContext;

        public GetChangeFrequenciesOfModulesContext(GetSummaryDataContext summaryDataContext)
        {
            _summaryDataContext = summaryDataContext;
        }
        public GetChangeFrequenciesOfModulesResponse Execute(string repositoryUrl)
        {
            var commits = _summaryDataContext.Execute(repositoryUrl);
            var revisionsOfModules = GetChangeFrequencies(commits);
            return new GetChangeFrequenciesOfModulesResponse { IsSuccess = true, Revisions = revisionsOfModules};
        }

        private Dictionary<string, int> GetChangeFrequencies(GetSummaryDataResponse summaryData)
        {
            var entitiesChangedCount = new Dictionary<string,int>();
            var uniqueEntities = new List<string>();
            foreach (var commit in summaryData.Commits)
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
                sortedDictionary.Add(kv.Key, kv.Value);
            }
            return sortedDictionary;
        }

        public class GetChangeFrequenciesOfModulesResponse : BaseResponse
        {
            public Dictionary<string, int> Revisions;
        }
    }
}
