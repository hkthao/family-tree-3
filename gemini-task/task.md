````
You are a senior frontend engineer.  
Implement a new **System Configuration Management feature** for an existing **Vue 3 + Pinia + Vuetify** project.

---

### 🔍 BEFORE YOU START
- Inspect existing **services**, **Pinia stores**, and **Vuetify components** in the codebase.
- Follow existing project conventions: folder structure, naming patterns, import style, API handling, and error management.
- Do **not** introduce new libraries or architectural patterns.

---

### 🧩 BACKEND MODEL REFERENCE

The backend provides this model for each configuration item:

```csharp
public class SystemConfigurationDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? ValueType { get; set; }
    public string? Description { get; set; }
}
````

---

### ⚙️ FEATURE REQUIREMENTS

1. **UI Component**

   * Create a `ConfigView.vue` page using Vuetify 3.
   * Use a tabbed layout or accordion sections for categories:

     * AI Chat
     * Embedding
     * Vector Store
     * Storage
     * System / Fixed
   * Display all configurations fetched from backend.
   * Render input components dynamically based on `ValueType`:

     * `"string"` → text field
     * `"int"` → numeric input
     * `"bool"` → switch
     * `"json"` → JSON textarea editor
   * Show `Description` below each field as helper text.
   * Read-only for fixed or sensitive keys (JWT, ConnectionStrings, API keys).
   * Include “Save” and “Cancel” buttons per tab.

2. **Pinia Store**

   * Create a store (e.g. `stores/configStore.ts`) to manage state:

     * `state`: list of `SystemConfigurationDto`
     * `actions`: `fetchConfigs()`, `updateConfig(key, value)`
     * Handle optimistic updates and error fallback.
     * Follow the same code conventions as other stores in the project.

3. **Service Layer**

   * Create a service (e.g. `services/configService.ts`) that calls backend endpoints:

     * `GET /api/systemconfig` → fetch all configs
     * `PUT /api/systemconfig/{key}` → update one config
   * Reuse the existing HTTP client abstraction (do not create a new one).

4. **UX Requirements**

   * Validate each field based on `ValueType`.
   * Show success/error toasts using the project’s existing notification system.
   * Highlight unsaved changes until “Save” is clicked.
   * Add “Reset to Default” if supported by API.
   * Optionally include a “Test Configuration” section for AIChatSettings.

5. **Code Style**

   * Use `<script setup lang="ts">`.
   * Follow existing TypeScript + Pinia patterns.
   * Keep Vuetify styling consistent (spacing, typography, color scheme).
   * Include proper typing for each config item.

6. **Output**

   * Generate:

     * `src/stores/configStore.ts`
     * `src/services/configService.ts`
     * `src/views/ConfigView.vue`
   * Code must compile immediately and integrate with the current project without breaking anything.
   * Include clear inline comments explaining logic for dynamic rendering, value parsing, and save flow.

---

### 💡 Additional Guidance

* Infer `ValueType` from backend response when rendering inputs.
* Preserve current app code structure and patterns.
* Use computed properties for reactive type casting (e.g., `parseInt`, `JSON.parse`).
* Use Vuetify’s built-in components (`v-text-field`, `v-switch`, `v-select`, `v-textarea`, `v-card`, `v-tabs`).

```