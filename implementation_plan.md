# Implementing NotificationService

Based on the highly detailed design spec, here is the proposed implementation plan for the `NotificationService`. This service will be built using .NET 8 following Clean Architecture principles, ensuring scalability, idempotency, and at-least-once delivery.

## User Review Required

> [!IMPORTANT]  
> Please review the chosen message broker and scaling defaults. By default, I will set up the foundational structure with **RabbitMQ** as the primary message bus and **PostgreSQL** for persistence. Kafka structures will be stubbed but left optional according to your specification. Is this acceptable? 

> [!WARNING]
> Since this service interacts with external infrastructure (PostgreSQL, RabbitMQ, Redis), I will create docker-compose definitions for these dependencies to ease local development and testing.

## Proposed Changes

The project will follow a Clean Architecture folder structure under `d:/100_Synapse/NotificationService`.

### 1. Solution & Project Setup
We will create a multi-project solution structure for Clean Architecture:
- `NotificationService.Domain`: Core entities, events, and repository interfaces.
- `NotificationService.Application`: Use cases, message consumers, scheduling/dispatching logic, and retry policies.
- `NotificationService.Infrastructure`: Implementations for RabbitMQ, Redis, PostgreSQL (EF Core), and SignalR.
- `NotificationService.API`: Entry point (`Program.cs`), Dependency Injection, configuration, and middleware.

#### [NEW] `NotificationService.sln`
#### [NEW] `API/NotificationService.API.csproj`
#### [NEW] `Application/NotificationService.Application.csproj`
#### [NEW] `Domain/NotificationService.Domain.csproj`
#### [NEW] `Infrastructure/NotificationService.Infrastructure.csproj`

---

### 2. Domain Layer
Contains the core business model, interfaces, and abstractions (no external dependencies).

#### [NEW] `Domain/Interfaces/IMessageBus.cs`
Abstractions for consuming and producing messages (to keep the infrastructure pluggable).
#### [NEW] `Domain/Interfaces/INotificationSender.cs`
Interface with multiple implementations (SignalR, Email, Push) and a Composite.
#### [NEW] `Domain/Entities/NotificationLog.cs`
Entity for persisting sent notifications to DB (for audit/tracking).
#### [NEW] `Domain/Events/NotificationEvent.cs`
The standard contract of the incoming event structure (with `EventId` and `UserId`).

---

### 3. Application Layer
Handles business rules, idempotency check workflow, and orchestrates delivery.

#### [NEW] `Application/Consumers/NotificationEventConsumer.cs`
The main consumer that pulls from `IMessageBus`. It checks idempotency, processes the message, handles retries, and delegates to the sender.
#### [NEW] `Application/Senders/CompositeSender.cs`
Implements `INotificationSender` to broadcast via all registered channels.
#### [NEW] `Application/Senders/EmailSender.cs` and `PushSender.cs`
Stub or concrete implementations of specific channels.

---

### 4. Infrastructure Layer
Contains all framework-specific and external tooling integrations.

#### [NEW] `Infrastructure/MessageBus/RabbitMQ/RabbitMqBus.cs`
Production-ready RabbitMQ integration handling prefetch = 50-200, DLQ setup, and queue-per-instance logic.
#### [NEW] `Infrastructure/Redis/IdempotencyService.cs`
Redis-based distributed cache check using `EventId` to prevent duplicates.
#### [NEW] `Infrastructure/SignalR/SignalRNotificationSender.cs`
SignalR delivery channel.
#### [NEW] `Infrastructure/SignalR/NotificationHub.cs`
The actual hub clients connect to.
#### [NEW] `Infrastructure/Persistence/NotificationDbContext.cs`
EF Core context for PostgreSQL persistence.

---

### 5. API Layer
Orchestration and runtime configuration.

#### [NEW] `API/Program.cs`
Wire up Dependency Injection, configure SignalR, Redis, MassTransit/RabbitMQ bindings, DB Context.
#### [NEW] `API/appsettings.json`
Configuration strings for Redis, PgSQL, and RabbitMQ.

## Open Questions

1. **Broker Tooling**: Do you want to use a framework like **MassTransit** to handle the RabbitMQ plumbing (which has built-in retry policies, DLQ, and consumers), or would you prefer a custom implementation on top of the plain `RabbitMQ.Client`? (MassTransit heavily simplifies the "horizontal scale" and "retry policy" requirements).
2. **Kafka Option**: Do you need actual Kafka implementation classes now, or just the structural placeholders as an option?
3. **Authentication**: For SignalR, clients typically authenticate to receive messages for their `UserId`. Should I set up JWT Bearer authentication on the hub from the start?

## Verification Plan

### Automated Tests
- Scaffold xUnit test projects.
- Write unit tests for `NotificationEventConsumer` to ensure it skips processing if duplicates are detected (`IdempotencyService` mock).
- Write tests for `CompositeSender` iteration.

### Manual Verification
- We can spin up a `docker-compose` cluster with Redis, Postgres, and RabbitMQ.
- Send a test message directly to the RabbitMQ queue and observe the application processing, persisting to DB, and broadcasting over SignalR.
