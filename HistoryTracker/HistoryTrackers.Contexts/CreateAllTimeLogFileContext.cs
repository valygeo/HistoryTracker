using Domain;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class CreateAllTimeLogFileContext
    {
        private readonly ICreateAllTimeLogFileGateway _gateway;

        public CreateAllTimeLogFileContext(ICreateAllTimeLogFileGateway gateway)
        {
            _gateway = gateway;
        }

        public CreateAllTimeLogFileResponse Execute(string clonedRepositoryPath)
        {
            if (String.IsNullOrWhiteSpace(clonedRepositoryPath))
                return new CreateAllTimeLogFileResponse { IsSuccess = false, Error = "Cloned repository path is empty!" };

            var repositoryName = Path.GetFileNameWithoutExtension(clonedRepositoryPath);
            var logFilePath = Path.Combine(clonedRepositoryPath, $"{repositoryName}.log");
            var isRepositoryUpToDate = _gateway.IsRepositoryUpToDate(clonedRepositoryPath);


            if (isRepositoryUpToDate)
            {
                var logFileAlreadyExists = _gateway.LogFileAlreadyExists(logFilePath);
                if (!logFileAlreadyExists)
                {
                    var logFileCreatedPath = _gateway.CreateLogFile(repositoryName, clonedRepositoryPath);

                    if (!String.IsNullOrWhiteSpace(logFileCreatedPath))
                        return new CreateAllTimeLogFileResponse { IsSuccess = true, LogFilePath = logFileCreatedPath };
                    return new CreateAllTimeLogFileResponse
                        { IsSuccess = false, Error = "Error occured while trying to create log file!" };
                }

                return new CreateAllTimeLogFileResponse { IsSuccess = true, LogFilePath = logFilePath };

            }

            {
                var fetchChangesResult = _gateway.FetchChanges(clonedRepositoryPath);

                if (!fetchChangesResult)
                    return new CreateAllTimeLogFileResponse { IsSuccess = false, Error = "Error trying to fetch changes!" };
                {
                    var createLogFileResult = _gateway.CreateLogFile(repositoryName, clonedRepositoryPath);
                    if (!String.IsNullOrWhiteSpace(createLogFileResult))
                        return new CreateAllTimeLogFileResponse { IsSuccess = true, LogFilePath = createLogFileResult};
                    return new CreateAllTimeLogFileResponse
                        { IsSuccess = false, Error = "Error occured while trying to create log file!" };
                }
            }
            
        }

        public class CreateAllTimeLogFileResponse : BaseResponse
        {
            public string LogFilePath { get; set; }
        }
    }
}
