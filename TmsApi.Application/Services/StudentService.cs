using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class StudentService(TmsDbContext db, ILogger<StudentService> logger) : IStudentService
{
    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = request.PageSize;

        var query = db.Students.AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                RegistrationNumber = s.RegistrationNumber,
                Name = s.Name,
                GPA = s.GPA
            })
            .ToListAsync(ct);

        // Instantiate using your required & init properties perfectly matching your record definition
        return new PagedResponse<StudentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var student = await db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (student is null) return null;

        return new StudentResponseDto
        {
            Id = student.Id,
            RegistrationNumber = student.RegistrationNumber,
            Name = student.Name,
            GPA = student.GPA
        };
    }

    public async Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var student = new Student
        {
            Name = request.Name,
            RegistrationNumber = $"TMS-2026-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            GPA = 0.0m, 
            IsActive = true
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created student {StudentId} with Reg Number {RegNum}", student.Id, student.RegistrationNumber);

        return new StudentResponseDto
        {
            Id = student.Id,
            RegistrationNumber = student.RegistrationNumber,
            Name = student.Name,
            GPA = student.GPA
        };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var student = await db.Students.FindAsync([id], cancellationToken: ct);
        if (student is null)
        {
            logger.LogWarning("Delete failed: Student {StudentId} not found.", id);
            return false;
        }

        student.IsDeleted = true; // Soft delete flag
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Soft-deleted student {StudentId}", id);
        return true;
    }
}