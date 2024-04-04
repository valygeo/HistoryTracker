
using Domain;
using HistoryTrackers.Contexts.Base;
using System.Web;

namespace HistoryTrackers.Contexts
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

            if (!Directory.Exists(Path.Combine(directoryPathWhereRepositoryWillBeCloned, repositoryName)))
            {
                var cloneResponse = _gateway.CloneRepository(githubUrl,directoryPathWhereRepositoryWillBeCloned);
                if(cloneResponse)
                    return new CloneRepositoryResponse { IsSuccess = true, ClonedRepositoryPath = Path.Combine(directoryPathWhereRepositoryWillBeCloned, repositoryName)};
                return new CloneRepositoryResponse { IsSuccess = false };
            }
            return new CloneRepositoryResponse { Error = "Repository already cloned!",ClonedRepositoryPath = Path.Combine(directoryPathWhereRepositoryWillBeCloned,repositoryName)};
        }
    }

    public class CloneRepositoryResponse : BaseResponse
    {
        public string ClonedRepositoryPath { get; set; }
    }
}
