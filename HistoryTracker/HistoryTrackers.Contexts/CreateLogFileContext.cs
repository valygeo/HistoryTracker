using Domain;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class CreateLogFileContext
    {
        private readonly ICreateLogFileGateway _gateway;

        public CreateLogFileContext(ICreateLogFileGateway gateway)
        {
            _gateway = gateway;
        }

        public CreateLogFileResponse Execute(string clonedRepositoryPath)
        {
            if (String.IsNullOrWhiteSpace(clonedRepositoryPath))
                return new CreateLogFileResponse { IsSuccess = false, Error = "Cloned repository path is empty!" };

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
                        return new CreateLogFileResponse { IsSuccess = true, LogFilePath = logFileCreatedPath };
                    return new CreateLogFileResponse
                        { IsSuccess = false, Error = "Error occured while trying to create log file!" };
                }

                return new CreateLogFileResponse { IsSuccess = true, LogFilePath = logFilePath };

            }

            {
                var fetchChangesResult = _gateway.FetchChanges(clonedRepositoryPath);

                if (!fetchChangesResult)
                    return new CreateLogFileResponse { IsSuccess = false, Error = "Error trying to fetch changes!" };
                {
                    var createLogFileResult = _gateway.CreateLogFile(repositoryName, clonedRepositoryPath);
                    if (!String.IsNullOrWhiteSpace(createLogFileResult))
                        return new CreateLogFileResponse { IsSuccess = true, LogFilePath = createLogFileResult , ChangesFetched = true};
                    return new CreateLogFileResponse
                        { IsSuccess = false, Error = "Error occured while trying to create log file!" };
                }
            }
            
        }

        public class CreateLogFileResponse : BaseResponse
        {
            public string LogFilePath { get; set; }
            public bool ChangesFetched { get; set; }
        }
    }
}
