using Domain;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class CloneRepositoryContext
    {
        private readonly ICloneRepositoryGateway _gateway;

        public CloneRepositoryContext(ICloneRepositoryGateway gateway)
        {
            _gateway = gateway;
        }

        public CloneRepositoryResponse Execute(string githubUrl)
        {
            var directoryPathWhereRepositoryWillBeCloned = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClonedRepositories");
            var repositoryName = Path.GetFileNameWithoutExtension(new Uri(githubUrl).AbsolutePath.TrimStart('/'));
            var repositoryPath = Path.Combine(directoryPathWhereRepositoryWillBeCloned, repositoryName);

            if (!Directory.Exists(repositoryPath))
            {
                var cloneResponse = _gateway.CloneRepository(githubUrl,directoryPathWhereRepositoryWillBeCloned);
                if(cloneResponse)
                    return new CloneRepositoryResponse { IsSuccess = true, ClonedRepositoryPath = repositoryPath};
                return new CloneRepositoryResponse { IsSuccess = false };
            }
            return new CloneRepositoryResponse { IsSuccess = false, Error = "Repository already cloned!", ClonedRepositoryPath = repositoryPath};
        }
    }

    public class CloneRepositoryResponse : BaseResponse
    {
        public string ClonedRepositoryPath { get; set; }
    }
}
