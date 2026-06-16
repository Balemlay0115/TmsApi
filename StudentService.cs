namespace TmsApi;

public interface IStudentService
{
    Task<StudentRecord> CreateAsync(string name, string email);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class StudentService : IStudentService
{
    private readonly Dictionary<string, StudentRecord> _store = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public Task<StudentRecord> CreateAsync(string name, string email)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var student = new StudentRecord(id, name, email, DateTime.UtcNow);
        _store[id] = student;
        _logger.LogInformation("Created student {StudentId} {StudentName}", id, name);
        return Task.FromResult(student);
    }

    public Task<StudentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        IReadOnlyList<StudentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed)
        {
            _logger.LogInformation("Deleted student {StudentId}", id);
        }
        else
        {
            _logger.LogWarning("Delete failed; student {StudentId} not found", id);
        }
        return Task.FromResult(removed);
    }
}

public record StudentRecord(string Id, string Name, string Email, DateTime CreatedAt);
