using Polly;

namespace Reviews.API.Clients;

public interface ICourtServiceClient
{
    Task<bool> CourtExistsAsync(Guid courtId, CancellationToken cancellationToken);
}

public class CourtServiceClient : ICourtServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<bool> _resiliencyPolicy;

    public CourtServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _resiliencyPolicy = Policy<bool>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));
    }

    public async Task<bool> CourtExistsAsync(Guid courtId, CancellationToken cancellationToken)
    {
        return await _resiliencyPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"/api/courts/{courtId}", cancellationToken);
            return response.IsSuccessStatusCode;
        });
    }
}