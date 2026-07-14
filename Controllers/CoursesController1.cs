using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(ICourseService courseService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null) return NotFound();

        // 1. Build link paths safely using structural routing metadata
        var selfPath = linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!;
        var enrollmentsPath = linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id })!;

        // 2. Build out base HATEOAS collection array
        var links = new List<LinkDto>
        {
            new(selfPath, "self", "GET"),
            new(selfPath, "update", "PUT"),
            new(selfPath, "delete", "DELETE"),
            new(enrollmentsPath, "enrollments", "GET")
        };

        // 3. Conditional validation rules engine evaluation to hide/show links dynamically
        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(enrollmentsPath, "enroll", "POST"));
        }

        
       var detailDto = new CourseDetailDto(
            course.Id,
            course.Code,
            course.Title,
            course.MaxCapacity,
            course.EnrollmentCount,
            links
        );

        return Ok(detailDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}