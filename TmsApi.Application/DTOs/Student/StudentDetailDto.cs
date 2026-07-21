using System.Collections.Generic;
using TmsApi.Application.DTOs;
using TmsApi.Application.DTOs.Student;

namespace TmsApi.Application.DTOs.Student;

public class StudentDetailDto : StudentResponseDto
{
    public List<LinkDto> Links { get; set; } = [];
}