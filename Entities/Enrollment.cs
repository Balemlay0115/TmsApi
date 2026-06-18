using System;
namespace TmsApi.Entities;
public class Enrollment
{
public int Id { get; set; }
public int StudentId { get; set; }
public int CourseId { get; set; }
<<<<<<< HEAD
public decimal? Grade { get; set; } // Nullable, as studentmay be currently enrolled
public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;// Navigation properties back to entities
=======
public decimal? Grade { get; set; }
public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

>>>>>>> bd0f3b7fdfdd91a82ed438331a081e072376c112
public Student Student { get; set; } = null!;
public Course Course { get; set; } = null!;
}