using TmsApi.Dtos;

namespace TmsApi.Services;

public interface ICertificateService
{
    Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(PagedRequest request, CancellationToken ct);
    Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CertificateResponseDto> CreateAsync(IssueCertificateRequest request, CancellationToken ct);
    Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId, CancellationToken ct);
    Task<bool> RevokeAsync(int id, CancellationToken ct);
}