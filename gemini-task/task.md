# 🧠 Prompt Gemini – Family Authorization Backend (C# .NET 8)

## Role
You are an experienced backend architect. Implement backend logic in **C# (.NET 8)** using **Clean Architecture** with **Ardalis.Specification**, **MediatR**, **CQRS**, and **Result<T> pattern**.  
Keep folder and layer separation consistent:

- `Domain` → entities, enums  
- `Application` → specifications, DTOs, validators, handlers  
- `Infrastructure` → EF repositories  
- `API` → controllers, dependency wiring

## Context
This project is a **family management system**.  
- Each **Family** can have multiple **Members**.  
- Each **Family** has one or more **Managers (owners)** and optionally **Viewers**.  
- Users are authenticated via **Auth0**, but backend uses a **UserProfile** entity to map Auth0 users.

---

## 🎯 Goal
Implement a secure backend that enforces **data access by family scope**.  
Only **authorized users** (managers or viewers) can access or modify the family’s data.

---

## ⚙️ Requirements

### 1. UserProfile Entity
Define an entity to map Auth0 users locally, including their families and roles.

### 2. FamilyUser (link table)
Define a link table to associate users to families with roles (`Manager` or `Viewer`).

### 3. Specifications
- `FamilyByUserIdSpec` → return only families the given user can access.  
- `MembersByFamilySpec` → filter members by family and user access.

### 4. CQRS Handlers
- `GetFamiliesQueryHandler`  
- `GetMembersQueryHandler`  
- `CreateMemberCommandHandler`  

Each handler must validate **current user's access** before executing.

### 5. Result Wrapper
Use a `Result<T>` pattern to wrap handler responses and errors.

### 6. Security Logic
- Inject `ICurrentUserService` to get current user from Auth0 token.  
- Resolve `UserProfile` from `Auth0UserId`.  
- Use that profile’s families to limit query scope.  
- Unauthorized access → return `Result.Failure("Access denied")`.

### 7. API Controller Layer
- Controllers **must not** contain business logic.  
- Use `[Authorize]` and send requests via MediatR.  
- Return `Result<T>` to frontend.

---

## 🧩 Output Expectation
Gemini must:

- Write **clean, production-ready C# code**.  
- Include only essential files (Entities, Specs, Handlers, Services, Controller sample).  
- Avoid repeating definitions or unnecessary models.  
- Maintain **Clean Architecture principles** strictly.  
- Provide **comments for junior developers** to understand logic flow.

---

## ⚠️ Loop Prevention Notes
- If implementation fails or loops, do not retry/rephrase.  
- Output last consistent version of the code.  
- Missing dependency → add stub with clear comment for later implementation.
