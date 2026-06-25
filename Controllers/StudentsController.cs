using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly TmsDbContext _db;

    public System.Security.Cryptography.RandomNumberGenerator _rng;

    public StudentsController(IStudentService studentService, TmsDbContext db)
    {
        _studentService = studentService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _studentService.GetAllAsync();
        return Ok(students);
    }

    [HttpGet("nplusone-test")]
    public async Task<IActionResult> TestNPlusOne(CancellationToken cancellationToken)
    {
        var students = await _db.Students.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var s in students)
        {
            var count = await _db.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id, cancellationToken);
            Console.WriteLine($"{s.Name}: {count} enrollments");
        }
        return Ok();
    }

    [HttpGet("shaped-test")]
    public async Task<IActionResult> TestShaped(CancellationToken cancellationToken)
    {
        var report = await _db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        foreach (var r in report)
            Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");

        return Ok(report);
    }

    [HttpPost("concurrency-test/{id}")]
    public async Task<IActionResult> TestConcurrency(int id)
    {
        var clientA = await _db.Students.FindAsync(id);
        var clientB = await _db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

        if (clientA == null || clientB == null) return NotFound();

        var uniqueStamp = DateTime.UtcNow.Ticks;
        clientA.Name = $"Client A - {uniqueStamp}";
        await _db.SaveChangesAsync();

        _db.ChangeTracker.Clear();

        _db.Entry(clientB).State = EntityState.Modified;
        clientB.Name = $"Client B - {uniqueStamp}";

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            Console.WriteLine("Concurrency exception caught successfully.");
            return Conflict("A concurrency conflict occurred.");
        }

        return Ok();
    }

    [HttpGet("audit-test/{id}")]
    public async Task<IActionResult> TestAuditStamp(int id)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        var lastUpdatedValue = _db.Entry(student).Property<DateTime>("LastUpdated").CurrentValue;

        return Ok(new
        {
            student.Id,
            student.Name,
            student.Version,
            LastUpdated = lastUpdatedValue
        });
    }

    [HttpDelete("soft-delete-test/{id}")]
    public async Task<IActionResult> TestSoftDelete(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();

        student.IsDeleted = true;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("soft-delete-verify/{id}")]
    public async Task<IActionResult> VerifySoftDelete(int id)
    {
        var normalQueryStudent = await _db.Students
            .FirstOrDefaultAsync(s => s.Id == id);

        var ignoredFilterStudent = await _db.Students
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);

        return Ok(new
        {
            FoundWithNormalQuery = normalQueryStudent != null,
            FoundWithIgnoreFilters = ignoredFilterStudent != null,
            IsDeletedValue = ignoredFilterStudent?.IsDeleted
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var student = await _studentService.GetByIdAsync(id);
        return student is not null ? Ok(student) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var student = await _studentService.CreateAsync(request.Name, request.Email);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateStudentRequest(string Name, string Email);