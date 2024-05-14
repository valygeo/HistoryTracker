
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class CreateLogFileFromSpecifiedPeriodContext
    {
        private readonly ICreateLogFileFromSpecifiedPeriodGateway _gateway;

        public CreateLogFileFromSpecifiedPeriodContext(ICreateLogFileFromSpecifiedPeriodGateway gateway)
        {
            _gateway = gateway;
        }

        public CreateLogFileFromSpecifiedPeriodResponse Execute(CreateLogFileFromSpecifiedPeriodData createLogFileData)
        {
            if (String.IsNullOrWhiteSpace(createLogFileData.clonedRepositoryPath))
                return new CreateLogFileFromSpecifiedPeriodResponse { IsSuccess = false, Error = "Cloned repository path is empty!" };

            var repositoryName = Path.GetFileNameWithoutExtension(createLogFileData.clonedRepositoryPath);
            var logFilePath = Path.Combine(createLogFileData.clonedRepositoryPath, $"{repositoryName}.log");
            var isRepositoryUpToDate = _gateway.IsRepositoryUpToDate(createLogFileData.clonedRepositoryPath);


            if (isRepositoryUpToDate)
            {
                var logFileAlreadyExists = _gateway.LogFileAlreadyExists(logFilePath);
                if (!logFileAlreadyExists)
                {
                    var logFileCreatedPath = _gateway.CreateLogFile(createLogFileData);

                    if (!String.IsNullOrWhiteSpace(logFileCreatedPath))
                        return new CreateLogFileFromSpecifiedPeriodResponse { IsSuccess = true, GeneratedLogFilePath = logFileCreatedPath };
                    return new CreateLogFileFromSpecifiedPeriodResponse
                    { IsSuccess = false, Error = "Error occured while trying to create log file!" };
                }

                return new CreateLogFileFromSpecifiedPeriodResponse { IsSuccess = true, GeneratedLogFilePath = logFilePath };

            }

            {
                var fetchChangesResult = _gateway.FetchChanges(createLogFileData.clonedRepositoryPath);

                if (!fetchChangesResult)
                    return new CreateLogFileFromSpecifiedPeriodResponse { IsSuccess = false, Error = "Error trying to fetch changes!" };
                {
                    var createLogFileResult = _gateway.CreateLogFile(createLogFileData);
                    if (!String.IsNullOrWhiteSpace(createLogFileResult))
                        return new CreateLogFileFromSpecifiedPeriodResponse { IsSuccess = true, GeneratedLogFilePath = createLogFileResult };
                    return new CreateLogFileFromSpecifiedPeriodResponse
                    { IsSuccess = false, Error = "Error occured while trying to create log file!" };
                }
            }
        }
    }

    public class CreateLogFileFromSpecifiedPeriodResponse : BaseResponse
    {
        public string GeneratedLogFilePath { get; set; }
    }
}
