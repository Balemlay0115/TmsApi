namespace TmsApi.Entities;
public class Course
{
<<<<<<< HEAD
    public int Id { get; set; } // surrogate primary key — internal, used by foreign keys
public required string Code { get; set; } // natural key —human-readable (uniqueness configured in Session 2)
public required string Title { get; set; }
public int Capacity { get; set; }
// Navigation property for many-to-many relationship
public ICollection<Enrollment> Enrollments { get; set; } =new List<Enrollment>();
=======
public int Id { get; set; }

public required string Code { get; set; } 

public required string Title { get; set; }
public int Capacity { get; set; }

public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
>>>>>>> bd0f3b7fdfdd91a82ed438331a081e072376c112
}