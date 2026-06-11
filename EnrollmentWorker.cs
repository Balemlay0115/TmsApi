namespace TmsApi;

public class EnrollmentWorker
{
    private readonly ILogger<EnrollmentWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(ILogger<EnrollmentWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task ProcessBatch()
    {
        _logger.LogInformation("Enrollment worker started batch processing.");

        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        var records = await service.GetAllAsync();

        _logger.LogInformation("Processed {Count} enrollment records.", records.Count);
    }
}
