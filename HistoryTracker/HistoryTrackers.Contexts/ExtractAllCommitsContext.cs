
using Domain.Entities;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class ExtractAllCommitsContext
    {
        public ExtractAllCommitsResponse Execute(ICollection<string> result)
        {
            List<Commit> commits = new List<Commit>();
            var commitToAdd = new Commit();

            for (int i = 0; i < result.Count; i++)
            {
                var line = result.ElementAt(i);
                if (line.StartsWith("["))
                {
                    commitToAdd = new Commit();
                    string[] parts = line.Split(' ', 4);
                    if (parts.Length >= 4)
                    {
                        commitToAdd.Id = parts[0];
                        commitToAdd.Author = parts[1];
                        commitToAdd.CommitDate = parts[2];
                        commitToAdd.Message = parts[3];
                    }

                    if (i + 1 < result.Count)
                    {
                        var nextLine = result.ElementAt(i + 1);
                        if (nextLine.StartsWith("["))
                        {
                            commits.Add(commitToAdd);
                            commitToAdd = new Commit();
                        }
                    }
                }

                else if (!String.IsNullOrWhiteSpace(line) && char.IsDigit(line[0]))
                {
                    var commitDetails = new CommitDetails();
                    string[] parts = line.Split('\t', 3);

                    if (parts.Length >= 3)
                    {
                        commitDetails.RowsAdded = int.Parse(parts[0]);
                        commitDetails.RowsDeleted = int.Parse(parts[1]);
                        commitDetails.EntityChangedName = parts[2];
                        commitToAdd.CommitDetails.Add(commitDetails);
                    }
                }

                else if (String.IsNullOrWhiteSpace(line))
                {
                    commits.Add(commitToAdd);
                }
            }

            if (!commits.Contains(commitToAdd))
                commits.Add(commitToAdd);

            return new ExtractAllCommitsResponse{IsSuccess = true, Commits = commits};
        }
    }

    public class ExtractAllCommitsResponse : BaseResponse
    {
        public ICollection<Commit> Commits { get; set; }
    }
}
