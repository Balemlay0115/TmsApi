using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;



public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.GPA).HasColumnType("numeric(3,2)");
        builder.Property(s => s.IsActive).IsRequired();

        builder.Property<DateTime>("LastUpdated");
        builder.Property(s => s.Version).IsRowVersion();

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}