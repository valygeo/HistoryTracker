
using Domain;
using HistoryTrackers.Contexts.Base;
using System.Web;

namespace HistoryTrackers.Contexts
{
    public class CreateLogFileContext
    {
        private readonly ICreateLogFileGateway _gateway;
        private readonly CloneRepositoryContext _cloneRepositoryContext;

        public CreateLogFileContext(ICreateLogFileGateway gateway, CloneRepositoryContext cloneRepositoryContext)
        {
            _gateway = gateway;
            _cloneRepositoryContext = cloneRepositoryContext;
        }

        public CreateLogFileResponse Execute(string githubUrl)
        {
            var cloneRepositoryResponse = _cloneRepositoryContext.Execute(githubUrl);
            if (cloneRepositoryResponse.IsSuccess)
            {
                var createLogFileResult = _gateway.CreateLogFile(githubUrl, cloneRepositoryResponse.ClonedRepositoryPath);
                if (createLogFileResult != null)
                    return new CreateLogFileResponse { IsSuccess = true, LogFilePath = createLogFileResult };
                return new CreateLogFileResponse { IsSuccess = false, Error = "Error occured while trying to create log file!" };
            }

            return new CreateLogFileResponse
                { IsSuccess = false, Error = "Erorr occured while trying to clone the repository!" };
        }
          
    }

    public class CreateLogFileResponse : BaseResponse
    {
        public string LogFilePath { get; set; }
    }
}
