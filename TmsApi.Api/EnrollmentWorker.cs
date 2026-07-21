using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.DTOs;
using TmsApi.Application.DTOs.Enrollment;
using TmsApi.Application.Interfaces;

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
        
        // Fixed: Supplied a default CancellationToken to match the service signature
        var records = await service.GetAllAsync(CancellationToken.None);

        _logger.LogInformation("Processed {Count} enrollment records.", records.Count);
    }
}