
using Domain.Entities;

namespace Domain
{
    public interface IGetSummaryDataGateway
    {
        public ICollection<string> GetSummaryData(string logFilePath);
    }
}
