
using System.Web;
using Domain;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class GenerateCsvWithNumberOfCodeLinesContext
    {
        private readonly IGenerateCsvWithNumberOfCodeLinesGateway _gateway;
        private readonly CloneRepositoryContext _cloneRepositoryContext;

        public GenerateCsvWithNumberOfCodeLinesContext(IGenerateCsvWithNumberOfCodeLinesGateway gateway, CloneRepositoryContext cloneRepositoryContext)
        {
            _gateway = gateway;
            _cloneRepositoryContext = cloneRepositoryContext;
        }

        public GenerateCsvWithNumberOfCodeLinesResponse Execute(string repositoryPath)
        {
            if (String.IsNullOrWhiteSpace(repositoryPath))
                return new GenerateCsvWithNumberOfCodeLinesResponse
                    { IsSuccess = false, Error = "Repository path is null!" };

            repositoryPath = HttpUtility.UrlDecode(repositoryPath);
            var cloneRepositoryResponse = _cloneRepositoryContext.Execute(repositoryPath);

            if (cloneRepositoryResponse.IsSuccess)
            {
                var generateCsv = _gateway.GenerateCsvWithNumberOfCodeLines(cloneRepositoryResponse.ClonedRepositoryPath);
                if (generateCsv)
                    return new GenerateCsvWithNumberOfCodeLinesResponse { IsSuccess = true };
                return new GenerateCsvWithNumberOfCodeLinesResponse
                    { IsSuccess = false, Error = "Error trying to generate csv file!" };
            }

            return new GenerateCsvWithNumberOfCodeLinesResponse
                { IsSuccess = false, Error = cloneRepositoryResponse.Error };
        }
    }

    public class GenerateCsvWithNumberOfCodeLinesResponse : BaseResponse
    {

    }
}
