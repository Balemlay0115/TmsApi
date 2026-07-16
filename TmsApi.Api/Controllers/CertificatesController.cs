using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/certificates")]
[Tags("Certificates")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CertificatesController(
    ICertificateService certificateService, 
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CertificateResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List certificates with pagination")]
    [EndpointDescription("Returns a paginated list of issued certificates.")]
    public async Task<IActionResult> GetCertificates([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await certificateService.GetCertificatesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCertificateById))]
    [ProducesResponseType(typeof(CertificateDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get certificate by ID")]
    [EndpointDescription("Returns detailed certificate metadata with contextual verification HATEOAS links.")]
    public async Task<IActionResult> GetCertificateById(int id, CancellationToken ct)
    {
        var certificate = await certificateService.GetByIdAsync(id, ct);
        if (certificate is null) return NotFound();

        var selfPath = linkGenerator.GetPathByName(HttpContext, nameof(GetCertificateById), new { id })!;
        var studentPath = linkGenerator.GetPathByName(HttpContext, "GetById", new { id = certificate.StudentId }) 
                          ?? $"/api/students/{certificate.StudentId}";

        var links = new List<LinkDto>
        {
            new(selfPath, "self", "GET"),
            new(studentPath, "student", "GET"),
            new($"{selfPath}/download", "download", "GET"),
            new($"{selfPath}/verify", "verify", "POST")
        };

        var detailDto = new CertificateDetailDto
        {
            Id = certificate.Id,
            SerialNumber = certificate.SerialNumber,
            IssuedAt = certificate.IssuedAt,
            StudentId = certificate.StudentId,
            StudentName = certificate.StudentName,
            CourseTitle = certificate.CourseTitle,
            Links = links
        };

        return Ok(detailDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Issue a new certificate")]
    public async Task<IActionResult> IssueCertificate([FromBody] IssueCertificateRequest request, CancellationToken ct)
    {
        if (await certificateService.ExistsForStudentAndCourseAsync(request.StudentId, request.CourseId, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Certificate already issued",
                Detail = $"A certificate has already been issued to student {request.StudentId} for this course.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await certificateService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCertificateById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Revoke/Delete a certificate")]
    public async Task<IActionResult> Revoke(int id, CancellationToken ct)
    {
        var success = await certificateService.RevokeAsync(id, ct);
        if (!success) return NotFound();

        return NoContent();
    }
}