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

        public CreateLogFileResponse Execute(string githubUrl, string clonedRepositoryPath)
        {
            var createLogFileResult = _gateway.CreateLogFile(githubUrl, clonedRepositoryPath);
            if (!String.IsNullOrWhiteSpace(createLogFileResult))
                return new CreateLogFileResponse { IsSuccess = true, LogFilePath = createLogFileResult };
            return new CreateLogFileResponse
                { IsSuccess = false, Error = "Error occured while trying to create log file!" };
        }

        public class CreateLogFileResponse : BaseResponse
        {
            public string LogFilePath { get; set; }
        }
    }
}
