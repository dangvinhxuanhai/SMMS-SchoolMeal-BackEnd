using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Domain.Entities.purchasing;

[Table("PurchaseOrderLines", Schema = "purchasing")]
public partial class PurchaseOrderLine
{
    [Key]
    public int LinesId { get; set; }

    public int OrderId { get; set; }

    public int IngredientId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal QuantityGram { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? UnitPrice { get; set; }

    [StringLength(100)]
    public string? BatchNo { get; set; }

    [StringLength(255)]
    public string? Origin { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey("IngredientId")]
    [InverseProperty("PurchaseOrderLines")]
    public virtual Ingredient Ingredient { get; set; } = null!;

    [ForeignKey("OrderId")]
    [InverseProperty("PurchaseOrderLines")]
    public virtual PurchaseOrder Order { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PurchaseOrderLines")]
    public virtual User User { get; set; } = null!;
}
