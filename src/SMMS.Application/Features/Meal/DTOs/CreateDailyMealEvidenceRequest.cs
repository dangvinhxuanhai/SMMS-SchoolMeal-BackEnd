using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SMMS.Application.Features.Meal.DTOs;
public class CreateDailyMealEvidenceRequest
{
    public IFormFile File { get; set; } = null!;
    public string? Caption { get; set; }
}
