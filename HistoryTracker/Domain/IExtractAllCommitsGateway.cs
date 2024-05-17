
namespace Domain
{
    public interface IExtractAllCommitsGateway
    { 
        ICollection<string> ReadLogFile(string logFilePath);
    }
}
