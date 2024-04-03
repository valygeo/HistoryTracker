
using Domain;
using Domain.Entities;

namespace HistoryTrackers.Contexts
{
    public class GetSummaryDataContext
    {
        private readonly IGetSummaryDataGateway _gateway;

        public GetSummaryDataContext(IGetSummaryDataGateway gateway)
        {
            _gateway = gateway;
        }
        public GetSummaryDataResponse Execute()
        {
            var url = "";
            _gateway.GetSummaryData(url);
            return new GetSummaryDataResponse();
        }
    }

    public class GetSummaryDataResponse
    {
        public SummaryData SummaryData { get; set; }
    }
}
