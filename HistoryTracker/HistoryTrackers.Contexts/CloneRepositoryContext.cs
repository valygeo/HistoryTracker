using System.Web;
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

        public CloneRepositoryResponse Execute(string repositoryUrl)
        {
            if (String.IsNullOrWhiteSpace(repositoryUrl))
                return new CloneRepositoryResponse { IsSuccess = false, Error = "Repository url is empty!" };
            repositoryUrl = HttpUtility.UrlDecode(repositoryUrl);
            var directoryPathWhereRepositoryWillBeCloned = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClonedRepositories");
            var repositoryName = Path.GetFileNameWithoutExtension(new Uri(repositoryUrl).AbsolutePath.TrimStart('/'));
            var repositoryPath = Path.Combine(directoryPathWhereRepositoryWillBeCloned, repositoryName);

            if (!Directory.Exists(repositoryPath))
            {
                var cloneResponse = _gateway.CloneRepository(repositoryUrl,directoryPathWhereRepositoryWillBeCloned);
                if(cloneResponse)
                    return new CloneRepositoryResponse { IsSuccess = true, ClonedRepositoryPath = repositoryPath};
                return new CloneRepositoryResponse { IsSuccess = false , Error = "Error trying to clone the repository!"};
            }
            return new CloneRepositoryResponse { IsSuccess = true, ClonedRepositoryPath = repositoryPath};
        }
    }

    public class CloneRepositoryResponse : BaseResponse
    {
        public string ClonedRepositoryPath { get; set; }
    }
}
