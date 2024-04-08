
namespace Domain
{
    public interface ICreateLogFileGateway
    {
        string CreateLogFile(string githubUrl, string clonedRepositoryPath);
    }
}
