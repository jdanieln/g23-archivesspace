using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Shared.Interfaces
{
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command);
    }
}
