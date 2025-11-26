using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.foodmenu.Interfaces;

namespace SMMS.Application.Features.foodmenu.Handlers;
public class LogAiSelectionCommandHandler
        : IRequestHandler<LogAiSelectionCommand>
{
    private readonly IMenuRecommendResultRepository _repo;

    public LogAiSelectionCommandHandler(IMenuRecommendResultRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(
            LogAiSelectionCommand request,
            CancellationToken cancellationToken)
    {
        var selectedTuples = request.SelectedItems
            .Select(x => (x.FoodId, x.IsMain));

        await _repo.MarkChosenAsync(
            request.UserId,
            request.SessionId,
            selectedTuples,
            cancellationToken);
    }
}
