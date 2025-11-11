using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class PurchaseOrderLineDto
{            
    public int LineId { get; set; }          // Mã dòng đơn hàng
    public int  OrderId { get; set; }          // Mã đơn hàng cha
    public int IngredientId { get; set; }    // Mã nguyên liệu (có thể null)
    public decimal QuantityGram { get; set; }  // Số lượng (gram)
    public decimal? UnitPrice { get; set; }    // Giá đơn vị (có thể null)
    public string? BatchNo { get; set; }       // Mã lô hàng
    public string? Origin { get; set; }        // Nguồn gốc / xuất xứ
    public DateOnly? ExpiryDate { get; set; }  // Ngày hết hạn
    public string IngredientName { get; set; } = string.Empty;
    public string IngredientType { get; set; } = string.Empty;
    // 🔹 Thuộc tính tính toán (tự động)
    public decimal TotalPrice => (decimal)(QuantityGram * (UnitPrice ?? 0));
}
