using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var userName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";

const string exchangeName = "demo.exchange";
const string queueName = "demo.queue";
const string routingKey = "orders.created";

var factory = new ConnectionFactory
{
    HostName = hostName,
    UserName = userName,
    Password = password,
    DispatchConsumersAsync = true // use async consumer
};

// graceful shutdown support
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare exchange/queue & bind (idempotent)
channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

// QoS (prefetch) — good for fair dispatch
channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.Received += async (model, ea) =>
{
    try
    {
        var json = Encoding.UTF8.GetString(ea.Body.Span);
        var msg = JsonSerializer.Deserialize<OrderCreated>(json);

        // simulate work
        Console.WriteLine($"[x] Received: {msg?.OrderId} ${msg?.Amount} (at {msg?.TimestampUtc:O})");
        await Task.Delay(300); // pretend processing

        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[!] Error: {ex.Message}");
        // Option 1: requeue
        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);

        // Option 2 (alternative): dead-lettering setup could route to a DLQ; then set requeue: false
        // channel.BasicNack(ea.DeliveryTag, false, requeue: false);
    }
};

var consumerTag = channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

Console.WriteLine("Consumer started. Press Ctrl+C to quit.");
try
{
    while (!cts.IsCancellationRequested)
    {
        await Task.Delay(200, cts.Token);
    }
}
catch (TaskCanceledException) { /* shutting down */ }

channel.BasicCancel(consumerTag);

public record OrderCreated
{
    public string OrderId { get; init; } = default!;
    public DateTime TimestampUtc { get; init; }
    public decimal Amount { get; init; }
}
