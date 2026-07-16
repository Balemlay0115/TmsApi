using System.Collections.Generic;

namespace TmsApi.Dtos;

public class CertificateDetailDto : CertificateResponseDto
{
    public List<LinkDto> Links { get; set; } = [];
}