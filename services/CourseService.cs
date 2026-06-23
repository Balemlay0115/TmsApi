namespace TmsApi;

public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string courseCode, string title);
    Task<CourseRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class CourseService : ICourseService
{
    private readonly Dictionary<string, CourseRecord> _store = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<CourseRecord> CreateAsync(string courseCode, string title)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var course = new CourseRecord(id, courseCode, title, DateTime.UtcNow);
        _store[id] = course;
        _logger.LogInformation("Created course {CourseId} {CourseCode}", id, courseCode);
        return Task.FromResult(course);
    }

    public Task<CourseRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var course);
        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        IReadOnlyList<CourseRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed)
        {
            _logger.LogInformation("Deleted course {CourseId}", id);
        }
        else
        {
            _logger.LogWarning("Delete failed; course {CourseId} not found", id);
        }
        return Task.FromResult(removed);
    }
}

public record CourseRecord(string Id, string CourseCode, string Title, DateTime CreatedAt);
