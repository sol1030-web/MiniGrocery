using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using MiniGrocery.Web.Models;

namespace MiniGrocery.Web.Services;

public interface IHolidayService
{
    Task<List<HolidayDto>> GetHolidaysAsync(int year, CancellationToken ct);
}

public class HolidayService : IHolidayService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    public HolidayService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    public async Task<List<HolidayDto>> GetHolidaysAsync(int year, CancellationToken ct)
    {
        var key = $"holidays:{year}:PH";
        if (_cache.TryGetValue<List<HolidayDto>>(key, out var cached))
        {
            return cached;
        }
        var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/PH";
        var resp = await _http.GetFromJsonAsync<List<NagerHoliday>>(url, ct);
        var list = resp?.Select(h => new HolidayDto { Date = h.date, Name = h.localName }).OrderBy(h => h.Date).ToList() ?? new List<HolidayDto>();
        _cache.Set(key, list, TimeSpan.FromDays(30));
        return list;
    }

    private class NagerHoliday
    {
        public DateTime date { get; set; }
        public string localName { get; set; } = string.Empty;
    }
}
