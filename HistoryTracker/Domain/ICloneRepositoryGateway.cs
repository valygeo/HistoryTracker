
namespace Domain
{
    public interface ICloneRepositoryGateway
    {
         bool CloneRepository(string githubUrl, string directoryPathWhereRepositoryWillBeCloned);
         bool IsRepositoryUpToDate(string repositoryClonedPath);
         bool FetchChanges(string repositoryClonedPath);
    }
}
