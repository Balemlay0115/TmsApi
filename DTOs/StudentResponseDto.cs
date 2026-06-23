namespace TmsApi.DTOs;

public class StudentResponseDto
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal GPA { get; set; }
}