using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BuyItCommerce.Application.Abstractions.Idempotency;

public static class RequestHashGenerator
{
    public static string Compute<T>(T request)
    {
        var json = JsonSerializer.Serialize(request);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }
}
