using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.DTOs.Course;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Caching;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CourseService(
    TmsDbContext context,
    HybridCache cache,
    ILogger<CourseService> logger) : ICourseService
{
    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        string key = CacheKeys.CourseById(id);
        var dbHit = false;

        var result = await cache.GetOrCreateAsync(
            key,
            (context, id),
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} fetching from DB", key);

                return await state.context.Courses
                    .AsNoTracking()
                    .Where(c => c.Id == state.id)
                    .Select(c => new CourseResponseDto(
                        c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
                    .FirstOrDefaultAsync(token);
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct
        );

        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);

        return result;
    }

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);

        // Exercise 3 Step 6: Invalidate on write path
        logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CoursesTag);
        await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);

        return (await GetByIdAsync(course.Id, ct))!;
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    public Task<Course?> GetByCodeAsync(string code, CancellationToken ct) =>
        context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == code, ct);

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        string key = CacheKeys.CoursesPaged(
            request.Page,
            request.PageSize,
            request.Search,
            request.OrderBy,
            request.Descending
        );

        var dbHit = false;

        var response = await cache.GetOrCreateAsync(
            key,
            (context, request),
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} fetching from DB", key);

                IQueryable<Course> query = state.context.Courses.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(state.request.Search))
                {
                    query = query.Where(c => EF.Functions.ILike(c.Title, $"%{state.request.Search}%")
                                          || EF.Functions.ILike(c.Code, $"%{state.request.Search}%"));
                }

                var totalCount = await query.CountAsync(token);

                query = state.request.OrderBy?.ToLower() switch
                {
                    "code" => state.request.Descending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
                    "maxcapacity" => state.request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
                    _ => state.request.Descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title)
                };

                var items = await query
                    .Skip((state.request.Page - 1) * state.request.PageSize)
                    .Take(state.request.PageSize)
                    .Select(c => new CourseResponseDto(
                        c.Id,
                        c.Code,
                        c.Title,
                        c.MaxCapacity,
                        c.Enrollments.Count))
                    .ToListAsync(token);

                return new PagedResponse<CourseResponseDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = state.request.Page,
                    PageSize = state.request.PageSize
                };
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct
        );

        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);

        return response;
    }
}