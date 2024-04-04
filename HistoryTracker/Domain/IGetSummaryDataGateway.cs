
using Domain.Entities;

namespace Domain
{
    public interface IGetSummaryDataGateway
    {
        public string GetSummaryData(string logFilePath);
    }
}
