using TmsApi.Dtos;

namespace TmsApi.Services;

public interface IStudentService
{
    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct);
    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}