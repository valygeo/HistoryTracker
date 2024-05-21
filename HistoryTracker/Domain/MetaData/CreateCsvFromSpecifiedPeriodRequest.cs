
namespace Domain.MetaData
{
    public class CreateCsvFromSpecifiedPeriodRequest
    {
        public string RepositoryName { get; set; }
        public string ClonedRepositoryPath { get; set; }
        public DateTime PeriodEndDate { get; set; }
    }
}
