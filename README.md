# 🚚 FleetOps

FleetOps is a backend-focused fleet and order management system built with .NET and designed to demonstrate clean architecture, distributed workflows, asynchronous messaging, and reliable backend communication.

---

## ✨ Features

- Order management
- Driver management
- Driver assignment workflow
- Clean Architecture
- CQRS with MediatR
- Entity Framework Core
- SQL Server
- Transactional Outbox Pattern
- RabbitMQ asynchronous messaging
- Publisher Confirms
- Persistent integration events
- Background processing
- Failure tracking and retry handling

---

## 🏗️ Architecture

The project follows Clean Architecture principles with clear separation between:

- Domain
- Application
- Infrastructure
- API

This keeps business logic independent from infrastructure concerns and makes the system easier to maintain, test, and extend.

---

## 🔄 Driver Assignment Saga

FleetOps uses a Saga-based workflow to coordinate the driver assignment process.

The workflow tracks the state of the operation and coordinates the required steps across services.

```text
Started
   ↓
Driver Reserved
   ↓
Completed

If a later step fails, compensating actions can be used to restore system consistency.
📨 Messaging with RabbitMQ
RabbitMQ is used for asynchronous communication between services.
Integration events are published through a dedicated messaging abstraction:
Application
    ↓
IIntegrationEventPublisher
    ↓
RabbitMQ Publisher
    ↓
Exchange: fleetops

Messages are published using:
- Publisher Confirmations
- Persistent delivery
- Message identifiers
- Event type routing keys
- Batch publishing
The RabbitMQ channel is reused and protected from concurrent access.
📦 Transactional Outbox Pattern
FleetOps uses the Transactional Outbox Pattern to improve reliability when publishing integration events.
Instead of publishing directly to RabbitMQ during the business transaction, integration events are first persisted in the database as Outbox Messages.
Business Operation
       ↓
Database Transaction
       ↓
Domain Data + Outbox Message
       ↓
Commit
       ↓
Outbox Processor
       ↓
RabbitMQ

This prevents situations where database changes are committed but the integration event is lost.
⚙️ Outbox Processor
A background service continuously processes pending Outbox Messages.
Current behavior:
- Polls every 5 seconds
- Reads messages in batches of 20
- Publishes batches to RabbitMQ
- Marks confirmed messages as processed
- Tracks failed publishing attempts
- Stores retry count and errors
- Resets the RabbitMQ channel after uncertain failures
OutboxMessages
      ↓
OutboxProcessor
      ↓
RabbitMQ Publisher
      ↓
Publisher Confirmation
     ↙        ↘
Success      Failure
  ↓             ↓
Processed     Retry

🛡️ Reliable Message Publishing
RabbitMQ Publisher Confirms are enabled to make sure messages are acknowledged by the broker before they are marked as processed.
If publishing fails:
- The message remains unprocessed
- The failure is recorded
- Retry information is updated
- The RabbitMQ channel is reset if its state becomes uncertain
This prevents Outbox Messages from being marked as processed before RabbitMQ confirms successful publishing.
🧰 Tech Stack
Backend
- C#
- .NET
- ASP.NET Core
- Entity Framework Core
- MediatR
Database
- SQL Server
Messaging
- RabbitMQ
- Publisher Confirms
- Transactional Outbox Pattern
Architecture & Patterns
- Clean Architecture
- CQRS
- Saga Pattern
- Repository Pattern
- Unit of Work
- Dependency Injection
- BackgroundService
📁 Project Structure
FleetOps
│
└── src
    └── Services
        ├── Order
        │   ├── Domain
        │   ├── Application
        │   ├── Infrastructure
        │   └── API
        │
        └── Driver
            ├── Domain
            ├── Application
            ├── Infrastructure
            └── API

🚀 Current Focus
The project is being expanded with additional distributed-system and resilience patterns.
Planned improvements include:
- Polly resilience strategies
- Retry policies
- Circuit Breaker
- Timeout handling
- Additional Saga compensation scenarios
- Improved observability and logging
- Integration testing
🎯 Purpose
FleetOps is a learning and portfolio project focused on applying production-oriented backend patterns such as:
- Reliable asynchronous communication
- Distributed workflow coordination
- Event-driven architecture
- Failure handling
- Transactional consistency
- Maintainable backend architecture
👨‍💻 Author
Ibrahim Salem
Backend .NET Developer
