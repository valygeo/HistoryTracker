
using Domain.Entities;

namespace Domain
{
    public interface IMergeChangeFrequenciesAndNumberOfCodeLinesGateway
    {
        bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(ICollection<ChangeFrequency> changeFrequenciesMetric, ICollection<CodeMetric> codeMetric);
    }
}
