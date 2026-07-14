using System.Collections.Generic;

namespace TmsApi.Dtos;

public class StudentDetailDto : StudentResponseDto
{
    public List<LinkDto> Links { get; set; } = [];
}