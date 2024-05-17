
namespace Domain
{
    public interface IExtractCommitsForSpecifiedPeriodFromLogFileGateway
    {
        ICollection<string> ReadLogFile(string logFilePath);
    }
}
