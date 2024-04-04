
using Domain;
using Domain.Entities;
using HistoryTrackers.Contexts.Base;
using System.Web;

namespace HistoryTrackers.Contexts
{
    public class GetSummaryDataContext
    {
        private readonly IGetSummaryDataGateway _gateway;
        private readonly CreateLogFileContext _createLogFileContext;

        public GetSummaryDataContext(IGetSummaryDataGateway gateway, CreateLogFileContext createLogFileContext)
        {
            _gateway = gateway;
            _createLogFileContext = createLogFileContext;
        }
        public GetSummaryDataResponse Execute(string githubUrl)
        {
            if (String.IsNullOrWhiteSpace(githubUrl))
                return new GetSummaryDataResponse { IsSuccess = false, Error = "Github url is empty!" };
            {
                githubUrl = HttpUtility.UrlDecode(githubUrl);
                var createLogFileResponse =  _createLogFileContext.Execute(githubUrl);

                if (createLogFileResponse.IsSuccess)
                {
                    var result = _gateway.GetSummaryData(createLogFileResponse.LogFilePath);
                    return new GetSummaryDataResponse { FileContent = result };
                }
                return new GetSummaryDataResponse { IsSuccess = false, Error = "Creation of the log file failed!" };
            }
        }
    }

    public class GetSummaryDataResponse : BaseResponse
    {
        public string FileContent { get; set; }
        public SummaryData SummaryData { get; set; }
    }
}
