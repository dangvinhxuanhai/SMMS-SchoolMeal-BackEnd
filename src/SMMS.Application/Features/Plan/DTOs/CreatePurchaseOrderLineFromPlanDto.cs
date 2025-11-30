using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Plan.DTOs;
public class CreatePurchaseOrderLineFromPlanDto
{
    public int IngredientId { get; set; }
    public decimal UnitPrice { get; set; }

    // Nếu muốn đặt số lượng khác Plan (mua ít hơn/chia đợt) thì truyền vào
    public decimal? QuantityOverrideGram { get; set; }

    public string? BatchNo { get; set; }
    public string? Origin { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
