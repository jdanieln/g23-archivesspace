using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Shared.Interfaces
{
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
