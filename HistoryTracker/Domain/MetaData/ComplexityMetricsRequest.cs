
namespace Domain.MetaData
{
    public class ComplexityMetricsRequest
    {
        public string RepositoryUrl { get; set; }
        public DateTime StartDatePeriod { get; set; }
        public DateTime EndDatePeriod { get; set; }
    }
}
