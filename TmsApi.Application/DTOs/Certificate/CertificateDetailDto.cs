using System.Collections.Generic;
using TmsApi.Application.DTOs.Certificate;
namespace TmsApi.Application.DTOs.Certificate;
public class CertificateDetailDto : CertificateResponseDto
{
    public List<LinkDto> Links { get; set; } = [];
}