using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMMS.Application.Features.billing.Helpers;
public static class PayOsSignatureHelper
{
    public static bool Verify(JsonElement dataElement, string signature, string checksumKey)
    {
        // Chuyển JsonElement -> Dictionary<string, string>
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var prop in dataElement.EnumerateObject())
        {
            var key = prop.Name;
            var valueElement = prop.Value;

            string valueStr = valueElement.ValueKind switch
            {
                JsonValueKind.Null => "",
                JsonValueKind.String => valueElement.GetString() ?? "",
                JsonValueKind.Number => valueElement.GetRawText(), // giữ nguyên chuỗi số
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Object or JsonValueKind.Array
                    => valueElement.GetRawText(), // trường hợp hiếm
                _ => valueElement.ToString()
            };

            if (valueStr is "null" or "undefined")
            {
                valueStr = "";
            }

            dict[key] = valueStr;
        }

        // Sort key alphabet
        var orderedKeys = dict.Keys
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();

        var sb = new StringBuilder();
        for (int i = 0; i < orderedKeys.Count; i++)
        {
            var key = orderedKeys[i];
            var value = dict[key];

            if (i > 0) sb.Append('&');
            sb.Append(key);
            sb.Append('=');
            sb.Append(value);
        }

        var dataToSign = sb.ToString();

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
        var hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return string.Equals(hashHex, signature, StringComparison.OrdinalIgnoreCase);
    }
}
