using System;

namespace TmsApi.Application.DTOs.Enrollment;

public class EnrollmentRecordDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Grade { get; set; }

    // Default parameterless constructor
    public EnrollmentRecordDto() { }

    // 4-argument constructor required by EnrollmentService.cs
    public EnrollmentRecordDto(int id, int studentId, string courseCode, DateTime enrollmentDate)
    {
        Id = id;
        StudentId = studentId;
        CourseTitle = courseCode;
        EnrollmentDate = enrollmentDate;
    }
}