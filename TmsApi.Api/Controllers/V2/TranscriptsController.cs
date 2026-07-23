using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController(ITranscriptJobQueue jobQueue) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public IActionResult RequestTranscript([FromBody] TranscriptRequestDto request)
    {
        var jobId = jobQueue.EnqueueJob(request.StudentId);
        var statusUrl = $"/api/v2/transcripts/{jobId}";

        // 202 Accepted with Location header pointing to status polling URL
        return Accepted(statusUrl, new { jobId, status = "Pending", statusUrl });
    }

    [HttpGet("{jobId:guid}")]
    public IActionResult GetStatus(Guid jobId)
    {
        var status = jobQueue.GetJobStatus(jobId);
        if (status is null) return NotFound();

        return Ok(status);
    }
}