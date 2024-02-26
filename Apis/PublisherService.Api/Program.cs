using Consul;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PublisherService.Api;
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

app.MapGet("/api/publishers", () =>
{
    return new List<Publisher>
    {
        new(1, "Penguin Random House", "Penguin Random House LLC is an American multinational conglomerate publishing company formed in 2013 from the merger of Random House and Penguin Group."),
        new(2, "Hachette Livre", "Hachette Livre, a subsidiary of the French media company Lagardère Group, is a publishing company."),
        new(3, "HarperCollins", "HarperCollins Publishers LLC is one of the world's largest publishing companies and is one of the Big Five English-language publishing companies."),
        new(4, "Macmillan Publishers", "Macmillan Publishers Ltd is a British publishing company owned by the German Holtzbrinck Publishing Group."),
        new(5, "Simon & Schuster", "Simon & Schuster, Inc., a subsidiary of ViacomCBS, is a publishing company founded in New York City in 1924 by Richard L. Simon and M. Lincoln Schuster."),
        new(6, "Houghton Mifflin Harcourt", "Houghton Mifflin Harcourt is a publisher of textbooks, instructional technology materials, assessments, reference works, and fiction and non-fiction for both young readers and adults."),
        new(7, "Scholastic", "Scholastic Corporation is an American multinational publishing, education and media company known for publishing, selling, and distributing books and educational materials for schools, teachers, parents, and children."),
        new(8, "Harlequin Enterprises", "Harlequin Enterprises Limited is a Toronto-based company that publishes")
    };
}).WithName("GetPublishers").WithOpenApi();

app.UseConsul();

app.Run();

internal record Publisher(int Id, string Name, string Description);