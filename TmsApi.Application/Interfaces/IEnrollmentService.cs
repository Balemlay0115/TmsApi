using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TmsApi.Application.DTOs.Enrollment;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentRecordDto> EnrollAsync(int studentId, string courseCode, CancellationToken ct);
    Task<EnrollmentRecordDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentRecordDto>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<EnrollmentRecordDto>> GetByCourseAsync(int courseId, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);

    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task AddAsync(Enrollment enrollment, CancellationToken ct);
    Task<List<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct);
}