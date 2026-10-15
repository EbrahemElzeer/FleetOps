namespace FleetOps.Order.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = null!;
    public int Port { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}