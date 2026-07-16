using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CertificateService(TmsDbContext db, ILogger<CertificateService> logger) : ICertificateService
{
    public async Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(PagedRequest request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = request.PageSize;

        var query = db.Certificates.AsNoTracking();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CertificateResponseDto
            {
                Id = c.Id,
                SerialNumber = c.SerialNumber,
                IssuedAt = c.IssuedAt,
                StudentId = c.StudentId,
                StudentName = c.Student.Name,
                CourseTitle = c.Course.Title
            })
            .ToListAsync(ct);

        // Instantiate using only your required init-only fields
        return new PagedResponse<CertificateResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Certificates
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CertificateResponseDto
            {
                Id = c.Id,
                SerialNumber = c.SerialNumber,
                IssuedAt = c.IssuedAt,
                StudentId = c.StudentId,
                StudentName = c.Student.Name,
                CourseTitle = c.Course.Title
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId, CancellationToken ct)
    {
        return await db.Certificates
            .AnyAsync(c => c.StudentId == studentId && c.CourseId == courseId, ct);
    }

    public async Task<CertificateResponseDto> CreateAsync(IssueCertificateRequest request, CancellationToken ct)
    {
        var studentName = await db.Students.Where(s => s.Id == request.StudentId).Select(s => s.Name).FirstOrDefaultAsync(ct) ?? "Unknown Student";
        var courseTitle = await db.Courses.Where(c => c.Id == request.CourseId).Select(c => c.Title).FirstOrDefaultAsync(ct) ?? "Unknown Course";

        var certificate = new Certificate
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            SerialNumber = $"CERT-2026-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            IssuedAt = DateTime.UtcNow
        };

        db.Certificates.Add(certificate);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Issued certificate {SerialNumber} to student {StudentId}", certificate.SerialNumber, request.StudentId);

        return new CertificateResponseDto
        {
            Id = certificate.Id,
            SerialNumber = certificate.SerialNumber,
            IssuedAt = certificate.IssuedAt,
            StudentId = certificate.StudentId,
            StudentName = studentName,
            CourseTitle = courseTitle
        };
    }

    public async Task<bool> RevokeAsync(int id, CancellationToken ct)
    {
        var certificate = await db.Certificates.FindAsync([id], cancellationToken: ct);
        if (certificate is null) return false;

        // Since we don't have IsActive, "Revoking" is done by removing the Certificate row from the DB
        db.Certificates.Remove(certificate);
        await db.SaveChangesAsync(ct);

        logger.LogWarning("Revoked/Deleted certificate {Id}", id);
        return true;
    }
}