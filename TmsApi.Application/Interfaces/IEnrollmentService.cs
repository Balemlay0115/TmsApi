using TmsApi.Dtos;
using TmsApi.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TmsApi.Services;

public interface IEnrollmentService
{
    // --- Existing Module 6 Signatures ---
    Task<EnrollmentRecordDto> EnrollAsync(int studentId, string courseCode, CancellationToken ct);
    Task<EnrollmentRecordDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentRecordDto>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<EnrollmentRecordDto>> GetByCourseAsync(int courseId, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);

    // --- Added for Module 7, Exercise 2 (CQRS Signatures) ---
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task AddAsync(Enrollment enrollment, CancellationToken ct);
    Task<List<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct);
}