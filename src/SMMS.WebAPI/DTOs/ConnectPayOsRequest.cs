namespace SMMS.WebAPI.DTOs;

public sealed class ConnectPayOsRequest
{
    public string ClientId { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string ChecksumKey { get; set; } = default!;
}
