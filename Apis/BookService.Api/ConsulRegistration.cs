using Consul;

namespace BookService.Api;

public static class ConsulRegistration
{
    public static void UseConsul(this IApplicationBuilder app)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
        var consulConfig = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("ConsulRegistration");
        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        var serviceName = consulConfig["Consul:ServiceName"];
        //var serviceId = $"{serviceName}-{Guid.NewGuid()}";
        var serviceId = $"{serviceName}";
        var serviceAddress = consulConfig["Consul:ServiceHost"];
        var servicePort = consulConfig["Consul:ServicePort"];
        var serviceScheme = consulConfig["Consul:ServiceScheme"];
        var HealthCheckPath = consulConfig["Consul:HealthCheckPath"];
        var serviceTTL = consulConfig["Consul:ServiceTTL"];
        var serviceTags = new[] { "authors", "api" };
        var healthCheckUrl = $"{serviceScheme}://{serviceAddress}:{servicePort}/{HealthCheckPath!.TrimStart('/')}";

        var registration = new AgentServiceRegistration
        {
            ID = serviceId,
            Name = serviceName,
            Address = serviceAddress,
            Port = int.Parse(servicePort!),
            Tags = serviceTags,
            Check = new AgentServiceCheck
            {
                HTTP = healthCheckUrl,
                Interval = TimeSpan.FromSeconds(10),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
            }
        };

        logger.LogInformation("Deregistering service {ServiceName} with ID {ServiceId}", serviceName, serviceId);
        consulClient.Agent.ServiceDeregister(serviceId).Wait();
        logger.LogInformation("Registering service {ServiceName} with ID {ServiceId}", serviceName, serviceId);
        consulClient.Agent.ServiceRegister(registration).Wait();
        logger.LogInformation("Service {ServiceName} with ID {ServiceId} registered", serviceName, serviceId);

        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Deregistering service {ServiceName} with ID {ServiceId}", serviceName, serviceId);
            consulClient.Agent.ServiceDeregister(serviceId).Wait();
            logger.LogInformation("Service {ServiceName} with ID {ServiceId} deregistered", serviceName, serviceId);
        });
    }
}
