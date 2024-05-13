
using Domain;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class GenerateCsvWithNumberOfCodeLinesContext
    {
        private readonly IGenerateCsvWithNumberOfCodeLinesGateway _gateway;

        public GenerateCsvWithNumberOfCodeLinesContext(IGenerateCsvWithNumberOfCodeLinesGateway gateway)
        {
            _gateway = gateway;
        }

        public GenerateCsvWithNumberOfCodeLinesResponse Execute(string clonedRepositoryPath)
        {
            var generatedCsvPath = _gateway.GenerateCsvWithNumberOfCodeLines(clonedRepositoryPath);
            if (!string.IsNullOrWhiteSpace(generatedCsvPath))
                return new GenerateCsvWithNumberOfCodeLinesResponse { IsSuccess = true, GeneratedCsvPath = generatedCsvPath};
            return new GenerateCsvWithNumberOfCodeLinesResponse { IsSuccess = false, Error = "Error trying to generate csv file!" };
        }
    }

    public class GenerateCsvWithNumberOfCodeLinesResponse : BaseResponse
    {
        public string GeneratedCsvPath { get; set; }
    }
}
