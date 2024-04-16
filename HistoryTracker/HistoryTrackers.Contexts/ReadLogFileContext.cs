
using Domain;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class ReadLogFileContext
    {
        private readonly IReadLogFileGateway _gateway;

        public ReadLogFileContext(IReadLogFileGateway gateway)
        {
            _gateway = gateway;
        }

        public ReadLogFileResponse Execute(string logFilePath)
        {
            if (!String.IsNullOrWhiteSpace(logFilePath))
            {
                var logFileContent = _gateway.ReadLogFile(logFilePath);
                if (logFileContent.Count > 0)
                    return new ReadLogFileResponse { IsSuccess = true, LogFileContent = logFileContent };
                return new ReadLogFileResponse { IsSuccess = false, Error = "Log file is empty or doesn't exist!" };
            }
            return new ReadLogFileResponse { IsSuccess = false, Error = "Log file path is null!" };
        }
    }

    public class ReadLogFileResponse : BaseResponse
    {
        public ICollection<string> LogFileContent { get; set; }
    }
}
