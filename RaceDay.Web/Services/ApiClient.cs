using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _context;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public ApiClient(
        HttpClient http,
        IHttpContextAccessor context,
        IConfiguration configuration)
    {
        _http = http;
        _context = context;
        _baseUrl = configuration["ApiBaseUrl"]?.TrimEnd('/')
            ?? "http://localhost:5170";
    }

    private void AddToken()
    {
        _http.DefaultRequestHeaders.Authorization = null;

        var token = _context.HttpContext?.Session.GetString("Token");

        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AddToken();

        var response = await _http.GetAsync($"{_baseUrl}{endpoint}");

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        AddToken();

        var response = await _http.PostAsJsonAsync(
            $"{_baseUrl}{endpoint}", data, JsonOptions);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        AddToken();

        var response = await _http.PutAsJsonAsync(
            $"{_baseUrl}{endpoint}", data, JsonOptions);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        AddToken();

        var response = await _http.DeleteAsync($"{_baseUrl}{endpoint}");
        return response.IsSuccessStatusCode;
    }
}