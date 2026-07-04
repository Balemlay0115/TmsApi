using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using Tms.Api.Dtos;

namespace Tms.Api.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct) =>
        _context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created enrollment {EnrollmentId} for student {StudentId} in course {CourseId}",
            enrollment.Id, enrollment.StudentId, enrollment.CourseId);

        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }

        public async Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct)
        {
            var list = await _context.Enrollments
                .AsNoTracking()
                .Select(e => new EnrollmentResponseDto(
                    e.Id,
                    e.CourseId,
                    e.StudentId,
                    e.EnrolledAt))
                .ToListAsync(ct);

            return list;
        }
}

