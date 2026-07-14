namespace TmsApi.Dtos;

public class StudentResponseDto
{
    public int Id { get; init; }
    public string RegistrationNumber { get; init; } = null!;
    public string Name { get; init; } = null!;
    public decimal GPA { get; init; }
}