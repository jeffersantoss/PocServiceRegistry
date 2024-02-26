using Microsoft.Extensions.Options;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOcelot(builder.Configuration).AddConsul();

builder.Services.Configure<FileConfiguration>(builder.Configuration.GetSection("Ocelot"), options => options.BindNonPublicProperties = true);
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptionsMonitor<FileConfiguration>>().CurrentValue);

var app = builder.Build();

app.MapGet("/", () => "The Gateway is running!");

var ocelotConfigMonitor = app.Services.GetRequiredService<IOptionsMonitor<FileConfiguration>>();

// Atualiza a configuração do Ocelot quando a configuração do arquivo mudar
ocelotConfigMonitor.OnChange(async (newConfig) =>
{
    await app.UseOcelot();
});

// Inicia o Ocelot
app.UseOcelot().Wait();

app.Run();