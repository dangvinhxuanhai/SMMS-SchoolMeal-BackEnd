using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.Command;
using SMMS.Application.Features.Meal.Interfaces;

namespace SMMS.Application.Features.Meal.Handlers;
public class DeleteDailyMealEvidenceCommandHandler
    : IRequestHandler<DeleteDailyMealEvidenceCommand>
{
    private readonly IDailyMealEvidenceRepository _repo;

    public DeleteDailyMealEvidenceCommandHandler(
        IDailyMealEvidenceRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(
        DeleteDailyMealEvidenceCommand request,
        CancellationToken ct)
    {
        await _repo.DeleteAsync(request.EvidenceId, ct);
    }
}
