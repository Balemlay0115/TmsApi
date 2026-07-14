using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService, ICourseService courseService) : ControllerBase
{
    [HttpGet(Name = "ListCourseEnrollments")]
    public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
    {
        var parentCourse = await courseService.GetByIdAsync(courseId, ct);
        if (parentCourse is null) return NotFound();

        var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);
        return Ok(enrollments);
    }

    [HttpGet("{id:int}", Name = "GetEnrollment")]
    public async Task<IActionResult> GetById(int courseId, int id, CancellationToken ct)
    {
        var record = await enrollmentService.GetByIdAsync(id, ct);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(int courseId, [FromBody] EnrollStudentRequest request, CancellationToken ct)
    {
        var parentCourse = await courseService.GetByIdAsync(courseId, ct);
        if (parentCourse is null) return NotFound();

        try
        {
            var record = await enrollmentService.EnrollAsync(request.StudentId, parentCourse.Code, ct);
            return CreatedAtAction(nameof(GetById), new { courseId, id = record.Id }, record);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameters",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int courseId, int id, CancellationToken ct)
    {
        var deleted = await enrollmentService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record EnrollStudentRequest(int StudentId);