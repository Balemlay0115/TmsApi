namespace TmsApi.Application.DTOs.Course;

public record CreateCourseRequest(string Code, string Title, int MaxCapacity);