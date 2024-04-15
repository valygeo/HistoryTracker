
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

        public GenerateCsvWithNumberOfCodeLinesResponse Execute(string repositoryPath)
        {
            if (String.IsNullOrWhiteSpace(repositoryPath))
                return new GenerateCsvWithNumberOfCodeLinesResponse
                    { IsSuccess = false, Error = "Repository path is null!" };
            var generateCsv = _gateway.GenerateCsvWithNumberOfCodeLines(repositoryPath);
            if (generateCsv)
                return new GenerateCsvWithNumberOfCodeLinesResponse { IsSuccess = true };
            return new GenerateCsvWithNumberOfCodeLinesResponse
                { IsSuccess = false, Error = "Error trying to generate csv file!" };
        }
    }

    public class GenerateCsvWithNumberOfCodeLinesResponse : BaseResponse
    {

    }
}
