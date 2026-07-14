using System.Collections.Generic;

namespace TmsApi.Dtos;

public record CourseDetailDto(
    int Id, 
    string Code, 
    string Title, 
    int MaxCapacity, 
    int EnrollmentCount, 
    IReadOnlyList<LinkDto> Links
) : CourseResponseDto(Id, Code, Title, MaxCapacity, EnrollmentCount);