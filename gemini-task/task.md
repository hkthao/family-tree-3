Refactor the existing Pinia store in `families.ts` and generate a complete unit test file, using Dependency Injection for services.

Current store:
- Pinia setup-style store: defineStore('families', ...)
- Manages Family entities with:
  - state: families[], loading, error, total
  - actions: fetchAll, fetchOne, add, update, remove
- Uses axios for real API requests
- Previously supported mock data mode via useMockData (should be removed)

Refactor requirements:

1. Create a new service file `family.service.ts`:
   - Move all axios calls (fetchFamilies, fetchFamilyById, addFamily, updateFamily, removeFamily) into this service.
   - Export an interface `FamilyServiceType` describing these functions.
   - Service functions should only return data (no state mutation).
   - There should be **two implementations**:
       a) RealFamilyService (uses axios)
       b) MockFamilyService (uses mockFamilies + simulateAsyncOperation)
   - Production code can inject the real service, test/UI can inject the mock service.

2. Update `families.ts` store:
   - Keep the store name and file unchanged.
   - Store only manages reactive state: families, loading, error, total.
   - Store actions receive a **service instance via dependency injection**.
   - Actions call service functions and update state accordingly.
   - Preserve setup-style store and TypeScript types.
   - Remove `useMockData` flag entirely.

3. TypeScript:
   - Keep existing Family interface.
   - Ensure all functions and state are properly typed.
   - Define service interface (`FamilyServiceType`) for injection.

4. Generate a complete Vitest unit test file `families.spec.ts`:
   - Mock the service using MockFamilyService or spies.
   - Test all store actions: fetchAll, fetchOne, add, update, remove.
   - Include both success and error cases for each action.
   - Test state updates: families, loading, error, total.
   - Use setup-style Pinia store.
   - Keep tests concise, readable, and easy to maintain.

Output:
- Refactored `families.ts` store file with dependency injection of service.
- New `family.service.ts` file containing:
    - FamilyServiceType interface
    - RealFamilyService (axios implementation)
    - MockFamilyService (mock data implementation)
- Vitest test file `families.spec.ts` covering all actions and error handling using dependency injection.
