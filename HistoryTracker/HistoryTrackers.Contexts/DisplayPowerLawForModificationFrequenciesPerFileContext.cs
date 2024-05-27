
using Domain.MetaData;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class DisplayPowerLawForModificationFrequenciesPerFileContext
    {
        public DisplayPowerLawForModificationFrequenciesPerFileResponse Execute(string filePath)
        {
            var fileContent = File.ReadAllLines(filePath);
            var changeFrequenciesPerFile = new List<ChangeFrequency>();
            for (int i = 1; i < fileContent.Length; i++)
            {
                var lineParts = fileContent[i].Split(',',2);
                var changeFrequency = new ChangeFrequency
                {
                    EntityPath = lineParts[0],
                    Revisions = int.Parse(lineParts[1])
                };
                changeFrequenciesPerFile.Add(changeFrequency);
            }
            return new DisplayPowerLawForModificationFrequenciesPerFileResponse { IsSuccess = true, ChangeFrequenciesPerFile = changeFrequenciesPerFile };
        }
    }

    public class DisplayPowerLawForModificationFrequenciesPerFileResponse : BaseResponse
    {
        public ICollection<ChangeFrequency> ChangeFrequenciesPerFile { get; set; }
    }
}
