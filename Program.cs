var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddScoped<IEnrollmentService,EnrollmentService>();

builder.Host.UseDefaultServiceProvider(options=>
{
    options.ValidateScopes=true;
    options.ValidateOnBuild=true;
});

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
app.MapGet("/api/enrollments/worker-smoke",(EnrollmentWorker worker)=>{
    worker.ProcessBatch();
    return Result.Ok("processed");

});
// app.MapControllers();

app.Run();
