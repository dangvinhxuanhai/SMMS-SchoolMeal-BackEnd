using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Features.nutrition.Queries;

namespace SMMS.Application.Features.nutrition.Handlers;
public class GetIngredientsQueryHandler
        : IRequestHandler<GetIngredientsQuery, IReadOnlyList<IngredientDto>>
{
    private readonly IIngredientRepository _repository;

    public GetIngredientsQueryHandler(IIngredientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<IngredientDto>> Handle(
        GetIngredientsQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetListAsync(
            request.SchoolId,
            request.Keyword,
            request.IncludeInactive,
            cancellationToken);

        var result = entities
            .Select(e => new IngredientDto
            {
                IngredientId = e.IngredientId,
                IngredientName = e.IngredientName,
                IngredientType = e.IngredientType,
                EnergyKcal = e.EnergyKcal,
                ProteinG = e.ProteinG,
                FatG = e.FatG,
                CarbG = e.CarbG,
                SchoolId = e.SchoolId,
                IsActive = e.IsActive
            })
            .ToList()
            .AsReadOnly();

        return result;
    }
}

public class GetIngredientByIdQueryHandler
       : IRequestHandler<GetIngredientByIdQuery, IngredientDto?>
{
    private readonly IIngredientRepository _repository;

    public GetIngredientByIdQueryHandler(IIngredientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IngredientDto?> Handle(
        GetIngredientByIdQuery request,
        CancellationToken cancellationToken)
    {
        var e = await _repository.GetByIdAsync(request.IngredientId, cancellationToken);
        if (e == null) return null;

        return new IngredientDto
        {
            IngredientId = e.IngredientId,
            IngredientName = e.IngredientName,
            IngredientType = e.IngredientType,
            EnergyKcal = e.EnergyKcal,
            ProteinG = e.ProteinG,
            FatG = e.FatG,
            CarbG = e.CarbG,
            SchoolId = e.SchoolId,
            IsActive = e.IsActive
        };
    }
}
