# RabbitMQ Demo in .NET (Producer + Consumer)

This project demonstrates how to send and receive messages using RabbitMQ from .NET console applications.

It includes:
- Producer → publishes “OrderCreated” messages
- Consumer → receives and processes them
- docker-compose.yml → runs RabbitMQ locally

------------------------------------------------------------
Requirements
------------------------------------------------------------
- .NET SDK (6 or 8)
- Docker Desktop (recommended) or RabbitMQ installed manually

------------------------------------------------------------
Start RabbitMQ
------------------------------------------------------------
Run in project folder:

docker compose up -d

RabbitMQ Management Portal:
http://localhost:15672

Login:
user: guest
pass: guest

Ports:
AMQP: 5672  
UI  : 15672

------------------------------------------------------------
RUN THE CONSUMER FIRST
------------------------------------------------------------
dotnet run --project Consumer

Expected Consumer output WAITING:

Consumer started. Press Ctrl+C to quit.

(No messages yet until Producer publishes.)

------------------------------------------------------------
RUN THE PRODUCER
------------------------------------------------------------
dotnet run --project Producer

The producer prompts for input:

Producer started. Type an order id (or press Enter to send a random one). Ctrl+C to quit.
> 

------------------------------------------------------------
EXAMPLE: Publishing Orders
------------------------------------------------------------
USER INPUT:
> 1001

Producer OUTPUT:
Published order: order1001 ($123.99)

USER INPUT:
> (just press Enter)

Producer OUTPUT:
Published order: order2222 ($87.99)

Each Enter sends a new order.

------------------------------------------------------------
EXPECTED CONSUMER OUTPUT WHEN MESSAGES ARRIVE
------------------------------------------------------------

[x] Received: order1001 $123.99 (at 2025-11-10T03:50:18.223Z)
[x] Received: order2222 $87.99 (at 2025-11-10T03:50:26.178Z)

Every line = one message the consumer successfully processed and ACKed.

------------------------------------------------------------
HOW TO VERIFY IN THE RABBITMQ PORTAL
------------------------------------------------------------

1. Open the UI in browser:
   http://localhost:15672

2. Go to **"Queues"** tab at the top.

3. Click the Queue:
   demo.queue

4. You will see:
   - **Ready**           = messages waiting to be consumed
   - **Unacked**         = messages delivered but not acknowledged yet
   - **Total**           = Ready + Unacked

When Consumer is running normally:
- Messages arrive → appear briefly → get processed → disappear → Ready returns to 0.

If Consumer is **not** running:
- Messages accumulate in **Ready**.

------------------------------------------------------------
MESSAGE FLOW (Simple)
------------------------------------------------------------
Producer → Exchange: demo.exchange → Queue: demo.queue → Consumer

Routing Key Used:
orders.created

------------------------------------------------------------
STOP EVERYTHING
------------------------------------------------------------
docker compose down

------------------------------------------------------------
COMMON ISSUES
------------------------------------------------------------
1) Producer fails to connect:
   → Ensure docker is running
   → Ensure container is up:  docker ps

2) Messages stuck in queue:
   → Run the consumer so messages can be acknowledged

3) UI not opening:
   → Ensure port 15672 is free and container running:
     docker logs rabbitmq-demo

------------------------------------------------------------
End of File
------------------------------------------------------------
