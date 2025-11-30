using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Plan.DTOs;
public class KsPurchaseOrderLineDto
{
    public int LinesId { get; set; }
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal QuantityGram { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? BatchNo { get; set; }
    public string? Origin { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
