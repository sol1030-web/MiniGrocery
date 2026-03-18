using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace MiniGrocery.Web.Services;

public interface IExchangeRatesService
{
    Task<(DateTime date, decimal usd, decimal eur)> GetLatestAsync(CancellationToken ct);
}

public class ExchangeRatesService : IExchangeRatesService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    public ExchangeRatesService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    public async Task<(DateTime date, decimal usd, decimal eur)> GetLatestAsync(CancellationToken ct)
    {
        var key = $"fx:usdphp:{DateTime.UtcNow:yyyy-MM-dd}";
        if (_cache.TryGetValue<(DateTime, decimal, decimal)>(key, out var cached))
        {
            return cached;
        }
        ExchangeResponse? resp = null;
        try
        {
            var url1 = "https://api.exchangerate.host/latest?base=USD&symbols=PHP";
            resp = await _http.GetFromJsonAsync<ExchangeResponse>(url1, ct);
        }
        catch { resp = null; }
        var date = DateTime.UtcNow.Date;
        var usdToPhp = resp?.rates != null && resp.rates.TryGetValue("PHP", out var php1) ? php1 : 0m;
        if (usdToPhp <= 0)
        {
            try
            {
                var url2 = "https://open.er-api.com/v6/latest/USD";
                var resp2 = await _http.GetFromJsonAsync<ErApiResponse>(url2, ct);
                usdToPhp = resp2?.rates != null && resp2.rates.TryGetValue("PHP", out var php2) ? php2 : usdToPhp;
            }
            catch { /* ignore */ }
        }
        var value = (date, usdToPhp, 0m);
        _cache.Set(key, value, TimeSpan.FromHours(6));
        return value;
    }

    private class ExchangeResponse
    {
        public Dictionary<string, decimal> rates { get; set; } = new();
    }
    private class ErApiResponse
    {
        public Dictionary<string, decimal> rates { get; set; } = new();
    }
}
