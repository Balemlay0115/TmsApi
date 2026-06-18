using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using TmsApi;
//<<<<<<< HEAD
using TmsApi.Data;
using TmsApi.Entities;
using Scalar.AspNetCore;

//>>>>>>> bd0f3b7fdfdd91a82ed438331a081e072376c112
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddProblemDetails();

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddControllers();

builder.Services.AddOpenApi();


builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDbContext> (options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information)
.EnableSensitiveDataLogging());

//<<<<<<< HEAD
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
// Middleware pipeline


//>>>>>>> bd0f3b7fdfdd91a82ed438331a081e072376c112
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/scaler/v1", () => Results.Ok(new
{
    status = "ok",
    version = "v1"
}));

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-001",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke", async (EnrollmentWorker worker) =>
{
    await worker.ProcessBatch();
    return Results.Ok("processed");
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Seed test Data at Startup

using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate();

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0002", Name = "BobJones", GPA = 2.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true } 
        };
        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new() { Code = "CS-101", Title = "Introduction to ComputerScience", Capacity = 30 },
            new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I", Capacity=40 }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges();

        var enrollments = new List<Enrollment>
        {
          new() {StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m},
          new() {StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m},
          new() {StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m} , 
          new() {StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m}    
        };
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

app.Run();
