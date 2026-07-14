using System;

namespace TmsApi.Dtos;

public class CertificateResponseDto
{
    public int Id { get; init; }
    public string SerialNumber { get; init; } = null!;
    public DateTime IssuedAt { get; init; }
    public int StudentId { get; init; }
    public string StudentName { get; init; } = null!;
    public string CourseTitle { get; init; } = null!;
}