using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var userName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";

const string exchangeName = "demo.exchange";
const string routingKey = "orders.created";

var factory = new ConnectionFactory
{
    HostName = hostName,
    UserName = userName,
    Password = password,
    DispatchConsumersAsync = true // harmless here, good default
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Idempotent declarations — safe to call repeatedly
channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);

Console.WriteLine("Producer started. Type an order id (or press Enter to send a timestamped one). Ctrl+C to quit.");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    var orderId = string.IsNullOrWhiteSpace(input) ? Guid.NewGuid().ToString("N") : input.Trim();

    var msg = new OrderCreated
    {
        OrderId = orderId,
        TimestampUtc = DateTime.UtcNow,
        Amount = Random.Shared.Next(10, 500) + 0.99m
    };

    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

    var props = channel.CreateBasicProperties();
    props.ContentType = "application/json";
    props.DeliveryMode = 2; // 2 = persistent

    channel.BasicPublish(
        exchange: exchangeName,
        routingKey: routingKey,
        mandatory: false,
        basicProperties: props,
        body: body);

    Console.WriteLine($"Published order: {msg.OrderId} (${msg.Amount})");
}

public record OrderCreated
{
    public string OrderId { get; init; } = default!;
    public DateTime TimestampUtc { get; init; }
    public decimal Amount { get; init; }
}
