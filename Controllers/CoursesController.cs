using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var course = await _courseService.GetByIdAsync(id);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var course = await _courseService.CreateAsync(request.CourseCode, request.Title);
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _courseService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateCourseRequest(string CourseCode, string Title);
