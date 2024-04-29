
using Domain;
using HistoryTracker.Contexts.Base;
using Domain.Entities;

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

                    if (!string.IsNullOrWhiteSpace(relativePath))
                    {
                        var existingEntity = modulesWithChangeFrequenciesAndAuthors.FirstOrDefault(e => e.EntityPath.Equals(relativePath));
                        if (existingEntity == null)
                        {
                            var entity = new ChangeFrequency
                            {
                                EntityPath = relativePath,
                                Revisions = 1,
                                Authors = new List<string>()
                            };
                            entity.Authors.Add(commit.Author);
                            modulesWithChangeFrequenciesAndAuthors.Add(entity);
                        }
                        else
                        {
                            existingEntity.Revisions++;
                            if(!existingEntity.Authors.Contains(commit.Author))
                                existingEntity.Authors.Add(commit.Author);
                        }
                    }
                }
            }

            return SortInDescendingOrder(modulesWithChangeFrequenciesAndAuthors);
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
