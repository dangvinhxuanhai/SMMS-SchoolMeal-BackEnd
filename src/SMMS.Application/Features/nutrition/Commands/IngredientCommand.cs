using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;

namespace SMMS.Application.Features.nutrition.Commands;
public class CreateIngredientCommand : UpsertIngredientRequest, IRequest<IngredientDto>
{
    [JsonIgnore]
    public Guid SchoolId { get; set; }
    [JsonIgnore]
    public Guid? CreatedBy { get; set; }
}

public class UpdateIngredientCommand : UpsertIngredientRequest, IRequest<IngredientDto>
{
    [JsonIgnore]
    public int IngredientId { get; set; }
}

public class DeleteIngredientCommand : IRequest
{
    public int IngredientId { get; set; }

    /// <summary>
    /// false = soft delete (IsActive = 0),
    /// true  = hard delete (xóa quan hệ rồi xóa Ingredient).
    /// </summary>
    public bool HardDelete { get; set; } = false;
}
