namespace TmsApi.DTOs;

public class CourseResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int MaxCapacity { get; set; }
}

public record CreateCourseRequest(string Code, string Title, int MaxCapacity);
