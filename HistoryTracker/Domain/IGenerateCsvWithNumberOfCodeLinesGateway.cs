
namespace Domain
{
    public interface IGenerateCsvWithNumberOfCodeLinesGateway
    {
        string GenerateCsvWithNumberOfCodeLines(string repositoryPath, string clocPath);
    }
}
