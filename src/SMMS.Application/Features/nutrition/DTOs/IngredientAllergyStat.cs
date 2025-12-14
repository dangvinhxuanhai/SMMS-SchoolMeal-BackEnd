using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.nutrition.DTOs;
public sealed class IngredientAllergyStat
{
    public int IngredientId { get; set; }
    public int AllergicStudentCount { get; set; }
}
