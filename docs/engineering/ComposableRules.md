# 📜 GEMINI CLI – RULE: Refactor Vue 3 Composable to Testable

> **Role**: Senior Vue 3 + Testing Engineer
> **Goal**: Refactor composables to be testable, maintainable, and side-effect controlled
> **Framework**: Vue 3 Composition API + Vitest

---

## 🧠 CORE PRINCIPLES

1. A composable **must not behave like a component**
2. Business logic must be **pure and testable**
3. Side-effects must be **isolated and injectable**
4. Composable tests must run **without mounting a component**

---

## 🧱 STRUCTURAL RULES (BẮT BUỘC)

### R1. Separate pure logic from composable

* Any function that:

  * does not use `ref`, `watch`, `emit`, lifecycle hooks
  * does not access DOM
    → **MUST be moved to a separate module**

**Output**:

* `*.logic.ts`
* `*.transform.ts`
* `*.utils.ts`

---

### R2. No direct DOM manipulation

❌ Forbidden:

* `document.querySelector`
* `innerHTML =`
* `addEventListener`

✅ Allowed:

* `ref<HTMLElement | null>`
* DOM logic moved to:

  * component
  * or adapter layer

---

### R3. External libraries must be wrapped by adapters

❌ Forbidden:

```ts
import axios from 'axios'
import f3 from 'family-chart'
```

✅ Required:

```ts
export interface XxxAdapter { ... }
```

* Adapters must be injectable via parameters
* Default adapter allowed for production

---

### R4. Lifecycle hooks must not contain business logic

❌ Forbidden:

```ts
onMounted(() => {
  // complex logic
})
```

✅ Required:

```ts
function initialize() {}
onMounted(initialize)
```

---

### R5. Watchers must delegate to named functions

❌ Forbidden:

```ts
watch(x, () => {
  // logic
})
```

✅ Required:

```ts
function handleXChange(x) {}
watch(x, handleXChange)
```

---

### R6. No nested functions beyond 1 level

* Nested helper functions must be extracted
* Exception: trivial closures (≤ 5 LOC)

---

### R7. All side-effects must be explicit

Side-effects include:

* emit
* network calls
* chart updates
* timers
* DOM mutation

They must be:

* isolated in `actions`
* injectable or mockable

---

### R8. Dependency Injection is mandatory

Composable signature must support dependency injection:

```ts
export function useX(
  deps: Partial<Deps> = defaultDeps
) {}
```

---

### R9. Return structure must be grouped

❌ Forbidden:

```ts
return { a, b, c, doA, doB }
```

✅ Required:

```ts
return {
  state: { a, b },
  actions: { doA, doB }
}
```

---

### R10. Composable must be testable without `mount`

* No reliance on component lifecycle
* No reliance on template rendering
* All logic callable directly

---

## 🧪 TESTABILITY RULES

### T1. Every extracted logic module must be unit-testable

* Pure input → output
* No mocks required

---

### T2. Adapters must be mockable

* Provide mock examples
* No hard dependency on real libraries

---

### T3. Do not test Vue internals

❌ Forbidden:

* testing `watch`, `ref`, `onMounted`

✅ Required:

* test observable behavior

---

## 📁 FILE STRUCTURE OUTPUT

```text
useFamilyTreeChart/
├── useFamilyTreeChart.ts
├── familyTree.transform.ts
├── familyTree.logic.ts
├── familyTree.adapter.ts
```

---

## 🧠 REFACTOR STRATEGY (MANDATORY ORDER)

1. Extract pure functions
2. Introduce adapters
3. Inject dependencies
4. Simplify composable
5. Preserve public API
6. Add test examples

---

## 🚫 STRICTLY FORBIDDEN

* Hidden side-effects
* Anonymous logic-heavy functions
* Hard-coded imports of side-effect libraries
* DOM access outside adapters
* Business logic inside lifecycle hooks

---

## 📌 FINAL CHECKLIST (Gemini must pass)

* [ ] Can all logic be tested without Vue component?
* [ ] Can all external libs be mocked?
* [ ] Is composable mostly orchestration?
* [ ] Is logic readable without Vue knowledge?

---