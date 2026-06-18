namespace TmsApi.Entities;
<<<<<<< HEAD

public class Student
{
    public int Id { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
=======
public class Student
{
    public int Id {get; set;}
    public required string RegistrationNumber {get; set;}
    public required string Name {get; set;}
    public decimal Gpa {get; set;}
    public bool IsActive {get; set;} = true;
    public ICollection<Enrollment> Enrollments {get; set;} = new List<Enrollment>();
>>>>>>> bd0f3b7fdfdd91a82ed438331a081e072376c112
}