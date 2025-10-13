gemini --prompt "
You are an expert C# backend developer and AI assistant engineer.

Task:
Review the C# class 'ChatWithAssistantQueryHandler' that handles user chat requests with AI. 
Currently, it always calls the AI provider regardless of whether relevant context exists in VectorStore. 
We want to improve the logic so that:

1. If the user message is general chat / greeting / small talk, or there is no relevant context in VectorStore:
   - Do not attempt to retrieve context for RAG.
   - Call the AI provider with a special fallback prompt.
   - The fallback prompt should be friendly, concise, and safe (do not invent technical data).
   - Examples: greetings, thanks, short casual conversation.

2. If relevant context exists:
   - Continue using current RAG logic (combine context chunks with user message).

3. Optional: implement simple intent detection (greeting / small talk / question) to decide when to fallback.

Requirements:
- Produce a **C# implementation** inside 'ChatWithAssistantQueryHandler'.
- Keep the current Clean Architecture, CQRS, MediatR, ResultWrapper patterns.
- Ensure proper logging of fallback events.
- Write clear, maintainable, extensible code.
- Include comments explaining fallback logic and intent detection.

Output:
Return the **full improved C# handler code**, ready to replace the existing class.
" --source ./backend/Application/AI/Chat/Queries/ChatWithAssistantQueryHandler.cs
