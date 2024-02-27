using BookService.Api;
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

app.MapGet("/api/books", () =>
{
    return new
    {
        books = new List<Book>
        {
            new(1, "The Catcher in the Rye", "The Catcher in the Rye is a novel by J. D. Salinger, partially published in serial form in 1945–1946 and as a novel in 1951."),
            new(2, "To Kill a Mockingbird", "To Kill a Mockingbird is a novel by Harper Lee published in 1960."),
            new(3, "1984", "Nineteen Eighty-Four: A Novel, often referred to as 1984, is a dystopian social science fiction novel by the English novelist George Orwell."),
            new(4, "The Great Gatsby", "The Great Gatsby is a 1925 novel by American writer F. Scott Fitzgerald."),
            new(5, "One Hundred Years of Solitude", "One Hundred Years of Solitude is a landmark 1967 novel by Colombian author Gabriel García Márquez that tells the multi-generational story of the Buendía family, whose patriarch, José Arcadio Buendía, founded the town of Macondo, a fictitious town in the country of Colombia."),
            new(6, "Brave New World", "Brave New World is a dystopian social science fiction novel by English author Aldous Huxley, written in 1931 and published in 1932."),
            new(7, "The Grapes of Wrath", "The Grapes of Wrath is an American realist novel written by John Steinbeck and published in 1939."),
            new(8, "The Lord of the Rings", "The Lord of the Rings is an epic high-fantasy novel by the English author and scholar J. R. R. Tolkien.")
        }
    };
}).WithName("GetBooks").WithOpenApi();

app.UseConsul();

app.Run();

internal record Book(int Id, string Title, string Description);