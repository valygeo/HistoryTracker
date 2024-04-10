
namespace Domain
{
    public interface ICloneRepositoryGateway
    {
         bool CloneRepository(string githubUrl, string directoryPathWhereRepositoryWillBeCloned);
    }
}
