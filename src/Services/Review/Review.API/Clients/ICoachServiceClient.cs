using Polly;

namespace Reviews.API.Clients;

public interface ICoachServiceClient
{
    Task<bool> CoachExistsAsync(Guid coachId, CancellationToken cancellationToken);
}

public class CoachServiceClient : ICoachServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<bool> _resiliencyPolicy;

    public CoachServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _resiliencyPolicy = Policy<bool>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));
    }

    public async Task<bool> CoachExistsAsync(Guid coachId, CancellationToken cancellationToken)
    {
        return await _resiliencyPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"/api/coaches/{coachId}", cancellationToken);
            return response.IsSuccessStatusCode;
        });
    }
}