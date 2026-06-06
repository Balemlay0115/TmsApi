var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.MapControllers();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/assessments/results",()=>Results.Ok(new
{
    courseCode="CS-001",
    studentId="S-001",
    letterGrade="A"
}));

// app.MapControllers();

app.Run();
