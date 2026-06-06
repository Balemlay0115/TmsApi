
builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddScoped<IEnrollmentService,EnrollmentService>();

builder.Host.UseDefaultServiceProvider(options=>
{
    options.ValidateScopes=true;
    options.ValidateOnBuild=true;
});
app.MapGet("/api/enrollments/worker-smoke",(EnrollmentWorker worker)=>{
    worker.ProcessBatch();
    return Result.Ok("processed");

});
$base="http://localhost:5086"
1..15|ForEach-object - parallel{
    Invoke-WebRequest -Uri"$using:base/api/enrollments/worker-smoke"
-UseBasicParsing | Out-Null}
-ThrottleLimit 15