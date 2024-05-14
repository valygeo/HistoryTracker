
namespace Domain.MetaData
{
    public class CreateLogFileFromSpecifiedPeriodData
    {
        public string repositoryName { get; set; }
        public string clonedRepositoryPath { get; set; }
        public DateTime periodStartDate { get; set; }
        public DateTime periodEndDate { get; set; }

    }
}
