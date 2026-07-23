using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.DTOs.Course;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public interface ICachedCourseService
{
    Task<CourseResponseDto?> GetCourseAsync(int id, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
    Task InvalidateCourseCacheAsync(CancellationToken ct);
}

public class CachedCourseService(
    HybridCache cache,
    ICourseService courseService,
    ILogger<CachedCourseService> logger) : ICachedCourseService
{
    public async Task<CourseResponseDto?> GetCourseAsync(int id, CancellationToken ct)
    {
        var key = CacheKeys.CourseById(id);
        var dbHit = false;

        var dto = await cache.GetOrCreateAsync(
            key,
            (courseService, id),
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                return await state.courseService.GetByIdAsync(state.id, token);
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);

        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);

        return dto;
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        var key = CacheKeys.CoursesPaged(
            request.Page,
            request.PageSize,
            request.Search,
            request.OrderBy,
            request.Descending);

        var dbHit = false;

        var result = await cache.GetOrCreateAsync(
            key,
            (courseService, request),
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                return await state.courseService.GetCoursesAsync(state.request, token);
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);

        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);

        return result;
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken ct)
    {
        logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CoursesTag);
        await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
    }
}