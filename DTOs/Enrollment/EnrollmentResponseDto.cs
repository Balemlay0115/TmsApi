using System;

namespace TmsApi.Dtos; // Flatten this namespace so it's globally visible inside Dtos

public record EnrollmentRecordDto(int Id, int StudentId, string CourseCode, DateTime EnrolledAt);