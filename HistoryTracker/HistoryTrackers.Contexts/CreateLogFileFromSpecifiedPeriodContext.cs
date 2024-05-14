
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class CreateLogFileFromSpecifiedPeriodContext
    {

    }

    public class CreateLogFileFromSpecifiedPeriodResponse : BaseResponse
    {
        public string GeneratedLogFilePath { get; set; }
    }
}
