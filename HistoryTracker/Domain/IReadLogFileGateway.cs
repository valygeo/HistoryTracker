
namespace Domain
{
    public interface IReadLogFileGateway
    {
        ICollection<string> ReadLogFile(string logFilePath);
    }
}
