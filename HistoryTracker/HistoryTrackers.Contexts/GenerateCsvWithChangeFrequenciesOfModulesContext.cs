
using System.Text;
using System.Text.RegularExpressions;
using Domain;
using HistoryTracker.Contexts.Base;
using Domain.Entities;
using static System.Collections.Specialized.BitVector32;

namespace HistoryTracker.Contexts
{
    public class GenerateCsvWithChangeFrequenciesOfModulesContext
    {
        private readonly IGenerateCsvWithChangeFrequenciesOfModulesGateway _gateway;
        private readonly CreateLogFileContext _createLogFileContext;
        private readonly ReadLogFileContext _readLogFileContext;
        private readonly ExtractAllCommitsContext _extractAllCommitsContext;

        public GenerateCsvWithChangeFrequenciesOfModulesContext(IGenerateCsvWithChangeFrequenciesOfModulesGateway gateway,
            CreateLogFileContext createLogFileContext, ReadLogFileContext readLogFileContext,
            ExtractAllCommitsContext extractAllCommitsContext)
        {
            _gateway = gateway;
            _createLogFileContext = createLogFileContext;
            _readLogFileContext = readLogFileContext;
            _extractAllCommitsContext = extractAllCommitsContext;
        }

        public GetChangeFrequenciesOfModulesResponse Execute(string clonedRepositoryPath)
        {
            var repositoryName = Path.GetFileNameWithoutExtension(clonedRepositoryPath);
            var csvFileName = $"{repositoryName}_change_frequencies_of_modules.csv";
            var csvFilePath = Path.Combine(clonedRepositoryPath, csvFileName);

            var createLogFileResponse = _createLogFileContext.Execute(clonedRepositoryPath);
            if (createLogFileResponse.IsSuccess)
            {
                var readLogFileResponse = _readLogFileContext.Execute(createLogFileResponse.LogFilePath);
                if (readLogFileResponse.IsSuccess)
                {
                    var extractAllCommitsResponse = _extractAllCommitsContext.Execute(readLogFileResponse.LogFileContent);
                    var revisionsOfModules = GetChangeFrequenciesAndAuthors(extractAllCommitsResponse); 
                    var createCsvResponse =  _gateway.CreateCsvFileWithChangeFrequenciesOfModules(revisionsOfModules, csvFilePath);
                    if(createCsvResponse)
                        return new GetChangeFrequenciesOfModulesResponse { IsSuccess = true, GeneratedCsvPath = csvFilePath};
                    return new GetChangeFrequenciesOfModulesResponse { IsSuccess = false, Error = "Error trying to create csv file!" };
                }
                return new GetChangeFrequenciesOfModulesResponse { IsSuccess = false, Error = readLogFileResponse.Error };
            }
            return new GetChangeFrequenciesOfModulesResponse { IsSuccess = false, Error = createLogFileResponse.Error };
        }
        
        private ICollection<ChangeFrequency> GetChangeFrequenciesAndAuthors(ExtractAllCommitsResponse commits)
        {
            var modulesWithChangeFrequenciesAndAuthors = new List<ChangeFrequency>();
            
            foreach (var commit in commits.Commits)
            {
                foreach (var commitDetail in commit.CommitDetails)
                {
                    var pathWithWindowsSlashes = commitDetail.EntityChangedName.Replace("/", "\\");
                    var pathWithoutComma = pathWithWindowsSlashes.Replace(",", "");
                    var relativePath = $".\\{pathWithoutComma}";

                    if (IsThereARenameOrMove(relativePath))
                    {
                       var initialAndNewIntegratedValue =  ProcessRenameOrMovePattern(relativePath);
                       relativePath = initialAndNewIntegratedValue[1];
                       foreach (var module in modulesWithChangeFrequenciesAndAuthors)
                       {
                           if (module.EntityPath == initialAndNewIntegratedValue[0])
                           {
                               module.EntityPath = relativePath;
                           }
                       }
                    }

                    if (!string.IsNullOrWhiteSpace(relativePath))
                    {
                        var existingEntity = modulesWithChangeFrequenciesAndAuthors.FirstOrDefault(e => e.EntityPath.Equals(relativePath));
                        if (existingEntity == null)
                        {
                            var entity = new ChangeFrequency
                            {
                                EntityPath = relativePath,
                                Revisions = 1,
                                Authors = commit.Author
                            };
                            modulesWithChangeFrequenciesAndAuthors.Add(entity);
                        }
                        else
                        {
                            existingEntity.Revisions++;
                            var stringBuilder = new StringBuilder(existingEntity.Authors);
                            if (!existingEntity.Authors.Contains(commit.Author))
                            {
                                stringBuilder.Append(';').Append(commit.Author);
                                existingEntity.Authors = stringBuilder.ToString();
                            }
                        }
                    }
                }
            }
            return SortInDescendingOrder(modulesWithChangeFrequenciesAndAuthors);
        }
        private static string[] ProcessRenameOrMovePattern(string value)
        {
            //Regex renameOrMovePattern = new Regex(@"\/\{+[a-zA-Z0-9\/\.]*[\s]=>[\s][a-zA-Z0-9\/\.]*[\s]*\}");
            Regex renameOrMovePattern = new Regex(@"\\{+[^{}]*[\s]=>[\s][^{}]*[\s]*\}");

            var matches = renameOrMovePattern.Matches(value);

            if (matches.Count > 0)
            {
                var names = matches[0].Value.Replace("\\{", "").Replace("}", "").Split(new[] { " => " }, StringSplitOptions.None);
                var initial = names[0];
                var @new = names[1];

                var initialIntegrated = value.Replace(matches[0].Value, string.IsNullOrEmpty(initial) ? "" : "\\" + initial);
                var newIntegrated = value.Replace(matches[0].Value, string.IsNullOrEmpty(@new) ? "" : "\\" + @new);

                return new[] { initialIntegrated, newIntegrated };
            }
            return new string[] { };
        }

        private static bool IsThereARenameOrMove(string result)
        {
            if (string.IsNullOrEmpty(result))
                return false;

            return result.IndexOf("{", StringComparison.InvariantCulture) > -1;
        }
        private static ICollection<ChangeFrequency> SortInDescendingOrder(
            ICollection<ChangeFrequency> changeFrequenciesAndAuthorsOfModules)
        {
            return changeFrequenciesAndAuthorsOfModules.OrderByDescending(module => module.Revisions).ToList();
        }

        public class GetChangeFrequenciesOfModulesResponse : BaseResponse
        {
            public string GeneratedCsvPath { get; set; }
        }
    }
}
