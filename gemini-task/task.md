You are an expert .NET developer experienced in **refactoring**, **clean architecture**, **DDD**, and **multi-channel notification systems**.

Task: Refactor an existing `NotificationService` so that it can send notifications through **multiple services** (e.g., Novu Cloud, SMTP email, Slack, SMS) **without being tightly coupled to any specific service**.

Requirements:

1. **Architecture**
   - Use **Dependency Injection** to inject notification providers.
   - Define an **INotificationProvider** interface with a `SendAsync(NotificationMessage message)` method.
   - `NotificationService` should be **service-agnostic**: it only calls the interface, not the concrete implementation.
   - Support **multiple providers at the same time** (e.g., send to both Novu Cloud and Slack).

2. **NotificationMessage**
   - Create a `NotificationMessage` DTO containing:
     - RecipientId (string)
     - Title (string)
     - Body (string)
     - Optional metadata dictionary

3. **Provider examples**
   - Implement at least two sample providers:
     1. `NovuNotificationProvider` using **Novu Cloud SDK**
     2. `EmailNotificationProvider` using SMTP
   - Providers should be **fully independent** and **replaceable**.

4. **Service usage**
   - `NotificationService` exposes a method `SendNotificationAsync(NotificationMessage message)` which:
     - Iterates all registered providers
     - Calls `SendAsync` for each provider

5. **Code quality**
   - Use **async/await**
   - Proper **DI registration** in Program.cs
   - Include comments explaining the purpose of each class/interface
   - Keep code modular for easy extension to more providers later

6. **Output**
   - Generate full C# code ready to use in a .NET 8 ASP.NET Core project
   - Include:
     - `INotificationProvider` interface
     - `NotificationMessage` DTO
     - `NotificationService` class
     - Two example providers (`NovuNotificationProvider` and `EmailNotificationProvider`)
     - Example `Program.cs` showing DI registration
   - Output **only code**, no explanations.
