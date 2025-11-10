# RabbitMQ Demo in .NET (Producer + Consumer)

This project demonstrates how to use RabbitMQ from .NET console applications.
It includes:
- Producer (publishes messages)
- Consumer (receives messages)
- docker-compose.yml to run RabbitMQ locally

------------------------------------------------------------
Requirements
------------------------------------------------------------
- .NET SDK (6 or 8)
- Docker Desktop (recommended) OR RabbitMQ installed manually

------------------------------------------------------------
Project Structure
------------------------------------------------------------
RabbitDemo/
  Producer/
    Producer.csproj
    Program.cs
  Consumer/
    Consumer.csproj
    Program.cs
  docker-compose.yml
  README.md

------------------------------------------------------------
Running RabbitMQ using Docker
------------------------------------------------------------
Open a terminal inside the project folder and run:

docker compose up -d

RabbitMQ UI:
http://localhost:15672
Username: guest
Password: guest

Ports:
- AMQP: 5672  (used by .NET apps)
- UI:   15672

------------------------------------------------------------
Running the Producer
------------------------------------------------------------
The Producer publishes JSON messages to RabbitMQ.

Run:
dotnet run --project Producer

Type anything and press Enter to send OR press Enter with no input to send a random order.

------------------------------------------------------------
Running the Consumer
------------------------------------------------------------
The Consumer subscribes and processes messages from Queue: demo.queue

Run:
dotnet run --project Consumer

You should now see messages printed as they are received.

------------------------------------------------------------
Message Routing Used
------------------------------------------------------------
Exchange: demo.exchange  (type: direct)
Queue:    demo.queue
Routing Key: orders.created

Flow:
Producer → Exchange → Queue → Consumer

------------------------------------------------------------
Stopping RabbitMQ
------------------------------------------------------------
docker compose down

------------------------------------------------------------
Troubleshooting
------------------------------------------------------------
1) Error: cannot connect to RabbitMQ
   → Ensure Docker Desktop is running
   → Ensure RabbitMQ container is up (docker ps)

2) Error: open //./pipe/dockerDesktopLinuxEngine
   → Start Docker Desktop
   → Ensure WSL2 backend is enabled

3) Messages stuck in queue
   → Start Consumer so messages can be acknowledged

------------------------------------------------------------
Next Enhancements (optional)
------------------------------------------------------------
- Add a Dead Letter Queue for failed messages
- Use a Topic Exchange for pattern routing
- Add Serilog structured logging
- Containerize Producer and Consumer

------------------------------------------------------------
End
------------------------------------------------------------
