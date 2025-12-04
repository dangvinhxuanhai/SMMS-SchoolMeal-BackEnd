namespace SMMS.WebAPI.DTOs;

public class UpdateInventoryItemRequest
{
    public decimal? QuantityGram { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? BatchNo { get; set; }
    public string? Origin { get; set; }
}
