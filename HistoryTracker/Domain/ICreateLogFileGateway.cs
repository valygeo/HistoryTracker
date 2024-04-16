
namespace Domain
{
    public interface ICreateLogFileGateway
    {
        string CreateLogFile(string githubUrl, string clonedRepositoryPath);
        bool IsRepositoryUpToDate(string repositoryClonedPath);
        bool FetchChanges(string repositoryClonedPath);
    }
}
