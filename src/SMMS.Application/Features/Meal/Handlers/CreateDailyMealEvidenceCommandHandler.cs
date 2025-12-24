using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.Command;
using SMMS.Application.Features.Meal.Interfaces;

namespace SMMS.Application.Features.Meal.Handlers;
public class CreateDailyMealEvidenceCommandHandler
    : IRequestHandler<CreateDailyMealEvidenceCommand, long>
{
    private readonly IDailyMealEvidenceRepository _repo;

    public CreateDailyMealEvidenceCommandHandler(
        IDailyMealEvidenceRepository repo)
    {
        _repo = repo;
    }

    public async Task<long> Handle(
        CreateDailyMealEvidenceCommand request,
        CancellationToken ct)
    {
        return await _repo.CreateAsync(
            request.DailyMealId,
            request.EvidenceUrl,
            request.Caption,
            request.UploadedBy,
            ct);
    }
}
