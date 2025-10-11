# Prompt: Implement Embedding Backend with Provider Abstraction

## Context
Xây dựng feature Embedding trong ASP.NET Core backend:
- Sinh embedding từ **đa provider**: OpenAI, Cohere, Local model
- Backend có sẵn **VectorStoreFactory / PineconeService**, không cần implement lại
- Dự án cá nhân: giới hạn token usage
- Clean Architecture + CQRS + ResultWrapper + Ardalis Specification

## Requirements

### 1️⃣ Configuration
- Có `EmbeddingSettings` tổng hợp nhiều provider và provider hiện tại
- Cấu hình qua `appsettings.json`
- Dễ mở rộng thêm provider mới

### 2️⃣ Provider Abstraction
- Tạo interface `IEmbeddingProvider` cho từng provider
- Implement OpenAI, Cohere, Local provider
- Local provider hỗ trợ test offline
- `EmbeddingService` orchestrate: chọn provider dựa trên config, gọi `GenerateEmbeddingAsync`

### 3️⃣ Vector Store Integration
- `EmbeddingService` dùng **VectorStoreFactory / PineconeService** hiện có để upsert vector hoặc query nearest vectors
- Không cần tạo lại Pinecone service

### 4️⃣ Constraints
- Giới hạn độ dài text để tiết kiệm token
- Không log API key
- Có cơ chế mock hoặc local fallback để test offline
- Clean Architecture: tách rõ Application / Infrastructure / API, không để business logic trong controller

### 5️⃣ Deliverables
- Tạo tất cả service, provider, interface cần thiết cho backend (không bao gồm Pinecone service)
- Đăng ký DI cho các service