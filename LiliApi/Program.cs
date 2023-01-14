using System.Diagnostics;

var webApplicationOptions = new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
    ApplicationName = Process.GetCurrentProcess().ProcessName
};

var builder = WebApplication.CreateBuilder(webApplicationOptions);
builder.Host.UseWindowsService();

builder.Services.AddCors(options =>
                options.AddPolicy("MyPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    }
                )
            );


var app = builder.Build();

app.UseCors("MyPolicy");

DateTime? shutdownPlannedAt = null;

app.MapPut("/shutdown/now", () =>
{
    Process.Start("shutdown", "/s /t 60");

    return Results.Ok();
});

app.MapPut("/shutdown", (double seconds) =>
{
    if (!shutdownPlannedAt.HasValue)
    {
        Process.Start("shutdown", $"/s /t {seconds}");

        shutdownPlannedAt = DateTime.Now.AddSeconds(seconds);

        return Results.Ok();
    }
    else
    {
        return Results.Conflict("System shutdown is already in process");
    }
});

app.MapPut("/shutdown/stop", () =>
{
    if (shutdownPlannedAt.HasValue)
    {
        Process.Start("shutdown", $"-a");

        shutdownPlannedAt = null;

        return Results.Ok();
    }
    else
    {
        return Results.Conflict("System shutdown is already stopped");
    }
});

app.MapGet("/shutdown/state", () =>
{
    if (shutdownPlannedAt.HasValue)
    {
        return Results.Ok((int)(shutdownPlannedAt.Value - DateTime.Now).TotalSeconds);
    }
    else
    {
        return Results.Ok(-1);
    }
});


app.Run();
