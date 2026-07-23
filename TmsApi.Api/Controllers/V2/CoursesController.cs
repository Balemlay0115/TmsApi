using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(ICachedCourseService cachedCourseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool descending = false,
        CancellationToken ct = default)
    {
        var request = new PagedRequest
        {
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 50),
            Search = search,
            OrderBy = orderBy,
            Descending = descending
        };

        var response = await cachedCourseService.GetCoursesAsync(request, ct);

        var totalPages = (int)Math.Ceiling(response.TotalCount / (double)response.PageSize);
        var hasNext = response.Page < totalPages;
        var hasPrevious = response.Page > 1;

        return Ok(new
        {
            data = response.Items,
            meta = new
            {
                totalCount = response.TotalCount,
                page = response.Page,
                pageSize = response.PageSize,
                totalPages,
                hasNext,
                hasPrevious
            },
            links = new
            {
                self = $"/api/v2/courses?page={response.Page}&pageSize={response.PageSize}",
                next = hasNext
                    ? $"/api/v2/courses?page={response.Page + 1}&pageSize={response.PageSize}"
                    : (string?)null,
                prev = hasPrevious
                    ? $"/api/v2/courses?page={response.Page - 1}&pageSize={response.PageSize}"
                    : (string?)null,
                enroll = "/api/v2/enrollments"
            }
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct = default)
    {
        var course = await cachedCourseService.GetCourseAsync(id, ct);
        if (course is null) return NotFound();
        return Ok(course);
    }
}