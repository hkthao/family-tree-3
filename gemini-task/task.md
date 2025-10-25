You are acting as a senior .NET architect specialized in DDD, CQRS, and scalable notification systems.

Context:
I am building a Family Tree Management App (Genealogy App) using ASP.NET Core + MySQL + DDD + CQRS.
The server has limited resources (only 4GB RAM), so performance and simplicity are critical.
Repository pattern is NOT used — DbContext is accessed directly in application layer.

We already have a base interface:
public interface INotificationService
{
    Task SendNotification(NotificationMessage message, CancellationToken cancellationToken = default);
}

Current challenge:
We need to build a Notification Template System that supports multi-channel templates and low-resource execution.

The problem:
- Each notification channel (Firebase, Email, InApp) supports different content formats.
- Email templates may use rich HTML.
- Firebase and InApp notifications must use plain text or Markdown.
- Templates must support placeholders (e.g. {UserName}, {FamilyName}, {ActionUrl}).
- Templates are stored in MySQL and can be edited dynamically.
- CQRS will be used: Command for create/update templates, Query for retrieve.
- We must avoid heavy abstractions, so no repository layer.
- Templates must be cached in memory for faster retrieval.
- Each channel will be a separate record (1 record = 1 channel of 1 event).

The goal:
Generate a complete, production-ready implementation of the Notification Template System.

The design must include:
1. Domain layer:
   - Entity: NotificationTemplate
   - Enums: NotificationType, NotificationChannel, TemplateFormat
   - BaseAuditableEntity if needed

2. Application layer:
   - CQRS Command/Query Handlers for:
     - CreateNotificationTemplateCommand
     - UpdateNotificationTemplateCommand
     - DeleteNotificationTemplateCommand
     - GetNotificationTemplateByIdQuery
     - GetNotificationTemplatesQuery
   - DTOs for commands and queries
   - Validation (FluentValidation or simple inline checks)
   - NotificationTemplateService that handles caching and rendering logic

3. Infrastructure layer:
   - AppDbContext with DbSet<NotificationTemplate>
   - MySQL-compatible EntityTypeConfiguration
   - EF Core migration script

4. Rendering:
   - TemplateRenderer class using Fluid or RazorEngine for placeholder replacement
   - Render(string template, object data) → string

5. Example usage:
   - How a domain event (e.g., NewFamilyMemberAddedEvent) triggers NotificationService → TemplateRenderer → Channel Dispatcher

6. Database schema suggestion for MySQL:
   - Table: notification_templates
     - id (Guid)
     - event_type (varchar)
     - channel (varchar)
     - subject (varchar)
     - body (text)
     - content_type (varchar)
     - placeholders (json)
     - language_code (varchar)
     - is_active (bool)
     - created_at / updated_at

7. CQRS + DbContext conventions:
   - Namespace structure:
     - Domain/Entities
     - Domain/Enums
     - Application/Notifications/Commands
     - Application/Notifications/Queries
     - Application/Common/Interfaces
     - Infrastructure/Persistence
     - Infrastructure/Services

8. TypeScript/Vue Admin UI conventions (for future template management):
   - Folder names use kebab-case (e.g. notification-template)
   - Vue file names use PascalCase (e.g. NotificationTemplateList.vue, NotificationTemplateForm.vue)
   - Component naming follows TypeScript and Vue 3 Composition API best practices

Requirements:
- Use async/await properly
- No external message queue
- No repository pattern
- Code must be modular, clean, and ready to integrate into existing DDD solution
- Optimize for readability and maintainability

