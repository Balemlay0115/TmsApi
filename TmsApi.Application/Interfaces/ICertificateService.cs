using TmsApi.Domain.Entities;
using TmsApi.Application.DTOs;
using TmsApi.Application.DTOs.Certificate;
namespace TmsApi.Application.Interfaces;

public interface ICertificateService
{
    Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(PagedRequest request, CancellationToken ct);
    Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CertificateResponseDto> CreateAsync(IssueCertificateRequest request, CancellationToken ct);
    Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId, CancellationToken ct);
    Task<bool> RevokeAsync(int id, CancellationToken ct);
}