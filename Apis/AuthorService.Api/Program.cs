using AuthorService.Api;
using Consul;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Runtime.InteropServices;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IHostLifetime>(sp => new DelayedShutdownHostLifetime(
    sp.GetRequiredService<IHostApplicationLifetime>(),
    TimeSpan.FromSeconds(0) // ... or whatever delay is appropriate for your service.
));

builder.Services.AddSingleton<IConsulClient, ConsulClient>(options => new ConsulClient(consulConfig =>
{
    var address = new Uri($"{builder.Configuration["Consul:Host"]}:{builder.Configuration["Consul:Port"]}");
    consulConfig.Address = address;
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var json = JsonSerializer.Serialize(new
        {
            Status = report.Status.ToString(),
            Environment = builder.Environment.EnvironmentName,
            Application = builder.Environment.ApplicationName,
            Platform = RuntimeInformation.FrameworkDescription
        });

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    }
});

app.MapGet("/api/authors", () =>
{
    return new
    {
        authors = new List<Author>
        {
            new(1, "James Baldwin", "James Arthur Baldwin was an American novelist, playwright, essayist, poet, and activist."),
            new(2, "Toni Morrison", "Toni Morrison was an American novelist, essayist, editor, teacher, and professor emeritus at Princeton University."),
            new(3, "Maya Angelou", "Maya Angelou was an American poet, memoirist, and civil rights activist."),
            new(4, "Langston Hughes", "Langston Hughes was an American poet, social activist, novelist, playwright, and columnist."),
            new(5, "Zora Neale Hurston", "Zora Neale Hurston was an American author, anthropologist, and filmmaker."),
            new(6, "Alice Walker", "Alice Walker is an American novelist, short story writer, poet, and social activist."),
            new(7, "Ralph Ellison", "Ralph Waldo Ellison was an American novelist, literary critic, and scholar."),
            new(8, "Richard Wright", "Richard Nathaniel Wright was an American author of novels, short stories, poems, and non-fiction."),
            new(9, "Octavia Butler", "Octavia Estelle Butler was an American science fiction author."),
            new(10, "Terry McMillan", "Terry McMillan is an American author. Her work is characterized")
        }
    };
}).WithName("GetAuthors").WithOpenApi();

app.UseConsul();

app.Run();

internal record Author(int Id, string Name, string Bio);