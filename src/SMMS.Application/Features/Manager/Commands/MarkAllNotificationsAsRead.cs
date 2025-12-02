using MediatR;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.Application.Features.Manager.Commands;

public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<bool>;

public class MarkAllNotificationsAsReadHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, bool>
{
    private readonly IManagerAccountRepository _repo;

    public MarkAllNotificationsAsReadHandler(IManagerAccountRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        await _repo.MarkAllNotificationsAsReadAsync(request.UserId);
        return true;
    }
}
