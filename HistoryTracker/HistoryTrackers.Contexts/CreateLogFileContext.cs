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

        public CreateLogFileResponse Execute(string repositoryUrl, string clonedRepositoryPath)
        {
            if (String.IsNullOrWhiteSpace(repositoryUrl))
                return new CreateLogFileResponse { IsSuccess = false, Error = "Repository url is empty!" };
            if (String.IsNullOrWhiteSpace(clonedRepositoryPath))
                return new CreateLogFileResponse { IsSuccess = false, Error = "Cloned repository path is empty!" };

            var isRepositoryUpToDate = _gateway.IsRepositoryUpToDate(clonedRepositoryPath);

            if (isRepositoryUpToDate)
            {
                var createLogFileResult = _gateway.CreateLogFile(repositoryUrl, clonedRepositoryPath);

                if (!String.IsNullOrWhiteSpace(createLogFileResult))
                    return new CreateLogFileResponse { IsSuccess = true, LogFilePath = createLogFileResult };
                return new CreateLogFileResponse
                    { IsSuccess = false, Error = "Error occured while trying to create log file!" };
            }

            {
                var fetchChangesResult = _gateway.FetchChanges(clonedRepositoryPath);

                if (!fetchChangesResult)
                    return new CreateLogFileResponse { IsSuccess = false, Error = "Error trying to fetch changes!" };
                {
                    var createLogFileResult = _gateway.CreateLogFile(repositoryUrl, clonedRepositoryPath);
                    if (!String.IsNullOrWhiteSpace(createLogFileResult))
                        return new CreateLogFileResponse { IsSuccess = true, LogFilePath = createLogFileResult };
                    return new CreateLogFileResponse
                        { IsSuccess = false, Error = "Error occured while trying to create log file!" };
                }
            }
            
        }

        public class CreateLogFileResponse : BaseResponse
        {
            public string LogFilePath { get; set; }
        }
    }
}
