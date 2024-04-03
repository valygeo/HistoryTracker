
using Domain.Entities;

namespace Domain
{
    public interface IGetSummaryDataGateway
    {
        public void GetSummaryData(string githubUrl);
    }
}
