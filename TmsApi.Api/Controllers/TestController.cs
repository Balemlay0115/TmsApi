using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine("\n>> STEP 2: Appending a sorting clause...");
        var orderQuery = query.OrderBy(s => s.Name);
        
        Console.WriteLine("\n>> STEP 3: Materializing query into a C# List...");
        var results = orderQuery.ToList();

        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA))
                .ToList();
            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("students-paged")]
    public async Task<IActionResult> GetStudentsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var pagedStudents = await context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(pagedStudents);
    }

    [HttpGet("courses-top5")]
    public async Task<IActionResult> GetTopCourses()
    {
        var topCourses = await context.Courses
            .Select(c => new
            {
                c.Id,
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync();

        return Ok(topCourses);
    }

    [HttpGet("students-with-courses")]
    public async Task<IActionResult> GetStudentsWithCourses()
    {
        var data = await context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("query-1-active-high-gpa")]
    public async Task<IActionResult> Query1ActiveStudentsHighGPA()
    {
        Console.WriteLine("\n>>> Query 1: Count active students with GPA >= 3.0");
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        Console.WriteLine($">>> Result: {count} active students with GPA >= 3.0\n");
        return Ok(new { query = "Active students with GPA >= 3.0", count });
    }

    [HttpGet("query-2-courses-most-enrollments")]
    public async Task<IActionResult> Query2CoursesWithMostEnrollments()
    {
        Console.WriteLine("\n>>> Query 2: Which courses have the most enrollments, sorted descending?");
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        Console.WriteLine($">>> Result: {list.Count} courses returned\n");
        return Ok(new { query = "Courses sorted by enrollment count", list });
    }

    [HttpGet("query-3-average-gpa-per-course")]
    public async Task<IActionResult> Query3AverageGPAPerCourse()
    {
        Console.WriteLine("\n>>> Query 3: What is the average GPA per course?");
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        Console.WriteLine($">>> Result: {list.Count} courses with average GPA calculated\n");
        return Ok(new { query = "Average GPA per course", list });
    }

    [HttpGet("query-4a-students-no-enrollments-subquery")]
    public async Task<IActionResult> Query4AStudentsNoEnrollmentsSubquery()
    {
        Console.WriteLine("\n>>> Query 4A: Students with zero enrollments (Subquery approach)");
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        Console.WriteLine($">>> Result: {list.Count} students with no enrollments\n");
        return Ok(new { query = "Students with no enrollments (Subquery)", list });
    }

    [HttpGet("query-4b-students-no-enrollments-leftjoin")]
    public async Task<IActionResult> Query4BStudentsNoEnrollmentsLeftJoin()
    {
        Console.WriteLine("\n>>> Query 4B: Students with zero enrollments (LeftJoin approach - EF Core 10)");
        var list = await context.Students
            .LeftJoin(context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e })
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync();

        Console.WriteLine($">>> Result: {list.Count} students with no enrollments\n");
        return Ok(new { query = "Students with no enrollments (LeftJoin)", list });
    }
}