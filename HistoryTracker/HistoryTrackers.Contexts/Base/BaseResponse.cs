
namespace HistoryTrackers.Contexts.Base
{
    public abstract class BaseResponse
    {
        public bool IsSuccess { get; set; } = true;

        public string Error { get; set; } 
        public ICollection<string> Errors { get; set; } = new List<string>();
    }
}
