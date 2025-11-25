using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.nutrition.DTOs;
public class IngredientDto
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = default!;
    public string? IngredientType { get; set; }
    public decimal? EnergyKcal { get; set; }
    public decimal? ProteinG { get; set; }
    public decimal? FatG { get; set; }
    public decimal? CarbG { get; set; }

    public Guid SchoolId { get; set; }
    public bool IsActive { get; set; }
}
