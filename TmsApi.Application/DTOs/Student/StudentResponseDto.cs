namespace TmsApi.Application.DTOs.Student;

public class StudentResponseDto
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal GPA { get; set; }
}