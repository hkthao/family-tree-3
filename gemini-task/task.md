## 💬 Prompt Gemini CLI: Multi-Provider Embedding + Vector Store

````
You are an expert C# backend engineer working with Clean Architecture, DDD, and CQRS. 
The project already has a sample embedding provider `CohereEmbeddingProvider` implementing `IEmbeddingProvider`. 
Vector store is designed as a plugin system similar to embedding providers.

🎯 Task:
Implement a **complete multi-provider embedding pipeline** for TextChunk entities and vector store, fully following Clean Architecture and existing project style.

---

### Requirements:

1️⃣ Multi-Provider Embedding
- Use interface `IEmbeddingProvider`:
  ```csharp
  public interface IEmbeddingProvider
  {
      string ProviderName { get; }
      int MaxTextLength { get; }
      Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
  }
````

* Implementations may include:

  * CohereEmbeddingProvider (already exists)
  * OpenAIEmbeddingProvider
  * LocalEmbeddingProvider
* Must support provider selection dynamically (by ProviderName) at runtime.

2️⃣ Command / Handler

* Implement `EmbedChunksCommand : IRequest` with fields:

  * List<TextChunk> Chunks
  * string ProviderName
* Implement `EmbedChunksCommandHandler`:

  * Accepts `EmbedChunksCommand`
  * Picks the right provider dynamically
  * Calls `GenerateEmbeddingAsync` for each chunk
  * Stores embedding in chunk object
  * Upserts chunk to **Vector Store plugin** (see below)

3️⃣ Vector Store Plugin

* Define interface `IVectorStore`:

  ```csharp
  public interface IVectorStore
  {
      Task UpsertAsync(TextChunk chunk, CancellationToken cancellationToken = default);
      Task<List<TextChunk>> QueryAsync(string queryText, int topK, Dictionary<string,string> metadataFilter, CancellationToken cancellationToken = default);
  }
  ```
* Implement at least a dummy/in-memory vector store for testing
* Must support metadata filter: fileId, familyId, category

4️⃣ TextChunk

* Already exists in Domain Layer with Metadata dictionary
* Should store `float[] Embedding` property

5️⃣ Coding Style

* Follow existing project style
* Use async/await, C# 10+
* Proper DI in constructors
* Keep code modular: each provider and vector store is plugable
* Include comments explaining how to add a new provider or vector store

6️⃣ Deliverables

* `EmbedChunksCommand.cs`
* `EmbedChunksCommandHandler.cs`
* Multi-provider embedding system (Cohere, OpenAI, Local)
* Vector store interface + dummy implementation
* Example usage showing:

  * Selecting a provider
  * Generating embeddings for chunks
  * Upserting to vector store
* Unit tests or sample test code for at least one provider

---

⚡ Notes

* Pipeline must allow **future providers/vector stores** to be plugged in without changing command handler logic
* Metadata in TextChunk must be preserved and used in vector store queries
* Return only production-ready C# code, following Clean Architecture

```

---

### 🔍 Giải thích prompt

| Phần | Mục tiêu |
|------|-----------|
| Multi-Provider Embedding | Cho phép runtime switch giữa Cohere / OpenAI / Local |
| Command + Handler | CQRS orchestration, không cần service “to đùng” |
| Vector Store Plugin | Cho phép swap store (Pinecone, in-memory, file-based) |
| Metadata | Giữ scope / filter query chính xác |
| Clean Architecture | Sẵn sàng plug vào dự án hiện tại |

