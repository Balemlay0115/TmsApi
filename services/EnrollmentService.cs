using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public interface IEnrollmentService
{
    Task<EnrollmentRecordDto> EnrollAsync(int studentId, string courseCode, CancellationToken ct);
    Task<EnrollmentRecordDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentRecordDto>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<EnrollmentRecordDto>> GetByCourseAsync(int courseId, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

public class EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public async Task<EnrollmentRecordDto> EnrollAsync(int studentId, string courseCode, CancellationToken ct)
    {
        // 1. Verify Student status
        var student = await context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId, ct);
        if (student is null)
            throw new ArgumentException("Student record not found.");
        if (!student.IsActive)
            throw new InvalidOperationException("Cannot enroll an inactive student.");

        // 2. Verify Course existence and check max capacity rules
        var course = await context.Courses.AsNoTracking()
            .Select(c => new { c.Id, c.Code, c.MaxCapacity, Count = c.Enrollments.Count })
            .FirstOrDefaultAsync(c => c.Code == courseCode, ct);

        if (course is null)
            throw new ArgumentException("Target course code not found.");
        if (course.Count >= course.MaxCapacity)
            throw new InvalidOperationException("The course has reached its maximum structural capacity.");

        // 3. Guard against duplicate registrations
        var exists = await context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == course.Id, ct);
        if (exists)
            throw new InvalidOperationException("Student is already registered for this course.");

        // 4. Save and execute transaction
        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = course.Id
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Enrolled student {StudentId} in {CourseCode} (Record {EnrollmentId})", studentId, courseCode, enrollment.Id);
        return (await GetByIdAsync(enrollment.Id, ct))!;
    }

    public Task<EnrollmentRecordDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EnrollmentRecordDto(e.Id, e.StudentId, e.Course.Code, DateTime.UtcNow))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<EnrollmentRecordDto>> GetAllAsync(CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentRecordDto(e.Id, e.StudentId, e.Course.Code, DateTime.UtcNow))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<EnrollmentRecordDto>> GetByCourseAsync(int courseId, CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentRecordDto(e.Id, e.StudentId, e.Course.Code, DateTime.UtcNow))
            .ToListAsync(ct);

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (enrollment is null) return false;

        context.Enrollments.Remove(enrollment);
        await context.SaveChangesAsync(ct);
        
        logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        return true;
    }
}

public record EnrollmentRecordDto(int Id, int StudentId, string CourseCode, DateTime EnrolledAt);