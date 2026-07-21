namespace TmsApi.Application.DTOs.Enrollment;

public class EnrollmentResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
}