
using Domain.MetaData;
using HistoryTracker.Contexts.Base;
using System.Text.RegularExpressions;
using Domain;
using System.Globalization;

namespace HistoryTracker.Contexts
{
    public class ExtractCommitsForSpecifiedPeriodFromLogFileContext
    {
        private readonly IExtractCommitsForSpecifiedPeriodFromLogFileGateway _gateway;

        public ExtractCommitsForSpecifiedPeriodFromLogFileContext(
            IExtractCommitsForSpecifiedPeriodFromLogFileGateway gateway)
        {
            _gateway = gateway;
        }

        public ExtractAllCommitsForSpecifiedPeriodResponse Execute(string logFilePath, string endDatePeriod)
        {
            var logFileContent = _gateway.ReadLogFile(logFilePath);
            List<Commit> commitsBeforeEndDate = new List<Commit>();
            List<Commit> commitsAfterEndDate = new List<Commit>();
            var commitToAdd = new Commit();

            if (!CheckIfEndDateIsGreaterOrEqualsThanFirstCommitDate(logFileContent, endDatePeriod))
                return new ExtractAllCommitsForSpecifiedPeriodResponse
                    { IsSuccess = false, Error = "End date should be greater than first commit date!" };
            {
                for (int i = 0; i < logFileContent.Count; i++)
                {
                    var line = logFileContent.ElementAt(i);
                    if (line.StartsWith("["))
                    {
                        commitToAdd = new Commit();
                        string[] parts = line.Split(' ', 4);
                        if (parts.Length >= 4)
                        {
                            if (IsDate(parts[2]))
                            {
                                commitToAdd.Id = parts[0];
                                commitToAdd.Author = parts[1];
                                commitToAdd.CommitDate = parts[2];
                                commitToAdd.Message = parts[3];
                            }
                            else
                            {
                                string[] partsForMultipleNameOfAuthor = line.Split(' ', 5);
                                commitToAdd.Id = partsForMultipleNameOfAuthor[0];
                                commitToAdd.Author = partsForMultipleNameOfAuthor[1] + partsForMultipleNameOfAuthor[2];
                                commitToAdd.CommitDate = partsForMultipleNameOfAuthor[3];
                                commitToAdd.Message = partsForMultipleNameOfAuthor[4];
                            }
                        }

                        if (i + 1 < logFileContent.Count)
                        {
                            var nextLine = logFileContent.ElementAt(i + 1);
                            if (nextLine.StartsWith("["))
                            {
                                if (IsCommitInSpecificPeriod(commitToAdd.CommitDate, endDatePeriod))
                                {
                                    commitsBeforeEndDate.Add(commitToAdd);
                                    commitToAdd = new Commit();
                                }
                                else if (!IsCommitInSpecificPeriod(commitToAdd.CommitDate, endDatePeriod))
                                {
                                    commitsAfterEndDate.Add(commitToAdd);
                                    commitToAdd = new Commit();
                                }

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
                        if (IsCommitInSpecificPeriod(commitToAdd.CommitDate, endDatePeriod))
                        {
                            commitsBeforeEndDate.Add(commitToAdd);
                        }
                        else if (!IsCommitInSpecificPeriod(commitToAdd.CommitDate, endDatePeriod))
                        {
                            commitsAfterEndDate.Add(commitToAdd);
                        }
                    }
                }

                if (!commitsBeforeEndDate.Contains(commitToAdd) &&
                    IsCommitInSpecificPeriod(commitToAdd.CommitDate, endDatePeriod))
                    commitsBeforeEndDate.Add(commitToAdd);

                if (!commitsAfterEndDate.Contains(commitToAdd) &&
                    !IsCommitInSpecificPeriod(commitToAdd.CommitDate, endDatePeriod))
                    commitsAfterEndDate.Add(commitToAdd);

                return new ExtractAllCommitsForSpecifiedPeriodResponse
                {
                    IsSuccess = true, CommitsBeforeEndDate = commitsBeforeEndDate,
                    CommitsAfterEndDate = commitsAfterEndDate
                };
            }
        }

        private static bool CheckIfEndDateIsGreaterOrEqualsThanFirstCommitDate(ICollection<string> logFileContent,
            string endDatePeriod)
        {
            var endDateIsGreaterOrEqualsThanFirstCommitDate = true;
            for (int i = logFileContent.Count - 1; i >= 0; i--)
            {
                var line = logFileContent.ElementAt(i);
                if (line.StartsWith("["))
                {
                    string[] commitParts = line.Split(' ', 4);
                    if (commitParts.Length >= 4)
                    {
                        if (IsDate(commitParts[2]))
                        {
                            endDateIsGreaterOrEqualsThanFirstCommitDate =
                                DateTime.Parse(endDatePeriod, CultureInfo.InvariantCulture) >=
                            DateTime.Parse(commitParts[2], CultureInfo.InvariantCulture);

                        }
                        else
                        {
                            string[] partsForMultipleNameOfAuthor = line.Split(' ', 5);
                            endDateIsGreaterOrEqualsThanFirstCommitDate =
                                DateTime.Parse(endDatePeriod, CultureInfo.InvariantCulture) >=
                                DateTime.Parse(partsForMultipleNameOfAuthor[3], CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            return endDateIsGreaterOrEqualsThanFirstCommitDate;
        }

        private static bool IsDate(string date)
        {
            var regex = "^\\d{4}-\\d{2}-\\d{2}$";
            return Regex.IsMatch(date, regex);
        }

        private static bool IsCommitInSpecificPeriod(string commitDateTime, string endDatePeriod)
        {
            if (DateTime.Parse(commitDateTime, CultureInfo.InvariantCulture) <=
                DateTime.Parse(endDatePeriod, CultureInfo.InvariantCulture))
                return true;
            return false;
        }
    }

    public class ExtractAllCommitsForSpecifiedPeriodResponse : BaseResponse
    {
        public ICollection<Commit> CommitsBeforeEndDate { get; set; }
        public ICollection<Commit> CommitsAfterEndDate { get; set; }
    }
}

