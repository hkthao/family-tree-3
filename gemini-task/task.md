You are an expert .NET architect specializing in DDD, scalable messaging, and notification systems.  
I am building a Genealogy Management App (Family Tree App) using ASP.NET Core + MySQL + DDD architecture.  
The system already defines this interface:

public interface INotificationService
{
    Task SendNotification(NotificationMessage message, CancellationToken cancellationToken = default);
}

I want to implement a complete Notification feature that can send messages via multiple channels:
- Firebase (for mobile push notifications)
- Email (via SMTP)
- In-App (for both Web and Mobile UI)
and display notifications on both the web toolbar and mobile app in real time.

Constraints:
- Resource-limited environment (only 4GB RAM)
- Using MySQL
- Vector DB and resource server are external
- The architecture must follow DDD principles
- Notifications should be queued and sent asynchronously to avoid blocking requests
- Should be extensible to add future channels (e.g., SMS, Webhook)

Requirements:
1. Design the domain model for notification (e.g., Notification, NotificationChannel, NotificationType, NotificationStatus).
2. Implement the infrastructure layer for each delivery channel (Firebase, Email, InApp).
3. Implement the application service layer (NotificationService) that decides which channel(s) to use based on user preferences.
4. Implement event-driven publishing when certain domain events occur (e.g., new family member added, relationship confirmed, user invited).
5. Show how to push In-App notifications to clients in real time using SignalR.
6. Suggest an optimal database schema for MySQL to store notification data.
7. Optimize for low memory footprint and minimal background service load.
8. Include example C# code snippets for:
   - Domain entities and value objects
   - NotificationService implementation
   - Background worker or message queue processor
   - SignalR hub for live updates
   - Example usage from application layer

Finally, summarize the overall architecture with a short explanation of how components interact (Domain Events → Notification Service → Delivery Channels → User UI).
