
using Domain.MetaData;

namespace Domain
{
    public interface ICreateLogFileFromSpecifiedPeriodGateway
    {
        string CreateLogFile(CreateLogFileFromSpecifiedPeriodData createLogFileRequest);
        bool IsRepositoryUpToDate(string clonedRepositoryPath);
        bool FetchChanges(string repositoryClonedPath);
        bool LogFileAlreadyExists(string logFilePath);
    }
}
