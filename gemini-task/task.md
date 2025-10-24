You are an expert .NET architect specialized in ASP.NET Core, DDD, CQRS, and scalable notification systems.

I am building a Family Tree application using ASP.NET Core 8 + MySQL.
I already have this interface defined:

public interface INotificationService
{
    Task SendNotification(NotificationMessage message, CancellationToken cancellationToken = default);
}

I want you to implement a complete **Notification Template System** with the following constraints and goals:

---

### 🧩 Architecture requirements:
- Use **CQRS pattern** (Command + Query + Handler) with MediatR.
- Use **DbContext directly (no repository)**.
- Follow DDD layering (Domain, Application, Infrastructure).
- Keep resource usage low (only 4GB RAM available).
- Support multiple notification channels: Email, Firebase Push, and InApp (SignalR).
- Templates should use placeholder syntax like `{{UserName}}`, `{{FamilyName}}`.
- Template rendering should replace placeholders dynamically.
- Use dependency injection properly.

---

### 🧱 Implementation requirements:
1. Define the **Domain Entities**:
   - NotificationTemplate (Id, EventType, Channel, Subject, Body, IsActive)
   - NotificationMessage (Id, Channel, Content, RecipientId, CreatedAt, Status)
2. Implement **NotificationDbContext** using Entity Framework Core with MySQL.
3. Implement **CQRS commands/queries**:
   - `CreateNotificationTemplateCommand`
   - `UpdateNotificationTemplateCommand`
   - `GetNotificationTemplateByEventTypeQuery`
   - `ListNotificationTemplatesQuery`
4. Implement **Application Service (NotificationService)**:
   - Use DbContext directly (no repository abstraction).
   - Load template by event type + channel.
   - Replace placeholders in the template body.
   - Send message via appropriate channel dispatcher.
5. Implement **NotificationChannelDispatchers**:
   - use SMTP
   - use Firebase Admin SDK
   - use SignalR Hub
   - Use a simple interface.
6. Implement **SignalR Hub** for real-time InApp notifications.
7. Add **EF Core migrations** and sample seeding for templates (3 events minimum).
8. Include example **CQRS usage**:
   - Create new template
   - Send notification by event type
9. Use **.NET dependency injection configuration** in Program.cs.

---

