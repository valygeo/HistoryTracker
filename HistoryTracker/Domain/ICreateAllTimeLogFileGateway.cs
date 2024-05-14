
namespace Domain
{
    public interface ICreateAllTimeLogFileGateway
    {
        string CreateLogFile(string repositoryName, string clonedRepositoryPath);
        bool LogFileAlreadyExists(string logFilePath);
        bool IsRepositoryUpToDate(string repositoryClonedPath);
        bool FetchChanges(string repositoryClonedPath);
    }
}
