using TmsApi.Dtos;
using TmsApi.Entities; // Ensure this is imported so Course entity is recognized

namespace TmsApi.Services;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
    
    // Added for Exercise 2 to allow command handlers to evaluate business constraints
    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);
}