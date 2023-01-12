using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

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

var isShutdownInProcess = false;

app.MapPut("/shutdown/now", () =>
{
    Process.Start("shutdown", "/s");

    return Results.Ok();
});

app.MapPut("/shutdown", (double seconds) =>
{
    if (!isShutdownInProcess)
    {
        isShutdownInProcess = true;

        return Results.Ok();
    }
    else
    {
        return Results.Conflict("System shutdown is already in process");
    }
});

app.MapPut("/shutdown/stop", () =>
{
    if (isShutdownInProcess)
    {
        isShutdownInProcess = false;

        return Results.Ok();
    }
    else
    {
        return Results.Conflict("System shutdown is already stopped");
    }
});

app.MapGet("/shutdown/state", () =>
{
    if (isShutdownInProcess)
    {
        return Results.Ok(12);
    }
    else
    {
        return Results.Ok(-1);
    }
});


app.Run();
