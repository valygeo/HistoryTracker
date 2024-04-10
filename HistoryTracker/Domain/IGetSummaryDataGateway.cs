

namespace Domain
{
    public interface IGetSummaryDataGateway
    {
        bool IsRepositoryUpToDate(string repositoryClonedPath);
        bool FetchChanges(string repositoryClonedPath);
        public ICollection<string> ReadFile(string logFilePath);
    }
}
