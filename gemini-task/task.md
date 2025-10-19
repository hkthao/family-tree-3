You are a senior frontend engineer. I need you to implement a **Configuration UI feature** in an existing Vue 3 project that uses **Pinia for state management** and **Vuetify for UI components**. Before generating code, you must:

1. **Check existing code style**:
   - Look at other services, Pinia stores, and UI components in the project.
   - Reuse coding patterns, folder structure, naming conventions, and Vuetify styling.
   - Do not introduce new architectural patterns or break current conventions.

2. **Functionality requirements**:
   - Create a **Configuration page/component** to manage admin-editable system settings.
   - Group settings into **tabs** (AI Chat, Embedding, Vector Store, Storage, System / Fixed).
   - Editable fields:
       - String → text input
       - Boolean → switch / checkbox
       - Integer → number input
       - Enum / Provider → select/dropdown
       - JSON objects → textarea or JSON editor
   - Read-only fields for fixed settings (DB connections, JWT, CORS).

3. **Pinia store**:
   - Create a store for **config state** with actions to:
       - Fetch all settings from backend service.
       - Update individual settings (with optimistic UI update).
       - Handle caching / state management as per project style.
   - Follow patterns of other existing stores (modules, actions, getters).

4. **Service integration**:
   - Integrate with **ConfigurationProvider backend service** (via API) to:
       - Fetch settings (`GET /api/systemconfig`)
       - Update settings (`PUT /api/systemconfig/{key}`)
   - Include error handling and success notifications.

5. **UI behavior**:
   - Tabs or accordion for setting categories.
   - Save / Cancel buttons per section or global.
   - Inline validation based on type (required, number >0, enum options valid).
   - Optionally: live preview/test area for AIChatSettings.

6. **Code style requirements**:
   - Follow Vuetify 3 component syntax.
   - Use script setup `<script setup>` with TypeScript.
   - Use Pinia store modules consistent with other features.
   - Keep styling consistent (spacing, class names, colors) with other project components.
   - No hard-coded API URLs; use existing project service pattern.

7. **Output**:
   - Generate the **Vue component**, **Pinia store**, and **service** scaffold.
   - Do not generate example API keys or secrets.

Your output should be **ready to drop into the project** and compile without breaking existing features.
