Bạn là senior developer, nhiệm vụ của bạn là refactor lại test code cho Pinia stores trong project Vue 3.

Hiện tại tôi có 2 file test:

- tests/family.store.spec.ts
- tests/member.store.spec.ts

Cả hai file này đều có các test CRUD (fetch, add, update, delete) lặp lại logic.  
Yêu cầu của tôi:

1. Tạo 1 file helper `tests/shared/sharedStoreTestUtils.ts` để gom code khởi tạo store (Pinia + mock service).
2. Tạo 1 file helper `tests/shared/crudTests.ts` chứa function `defineCrudTests` để gom toàn bộ các test CRUD (fetch/add/update/delete).
3. Refactor `family.store.spec.ts` và `member.store.spec.ts` sao cho:
   - Chỉ cần import `defineCrudTests`.
   - Truyền vào: `storeName`, `useStore`, `mockService`, `entitySample`.
   - Toàn bộ logic test CRUD sẽ dùng lại từ helper, không viết lại.
4. Đảm bảo test vẫn chạy bằng Vitest, không bị fail.
5. Giữ code rõ ràng, có comment giải thích.

Mục tiêu cuối cùng: giảm duplication, cấu trúc chuyên nghiệp như team QA/FE chuyên nghiệp.

Hãy viết lại toàn bộ code mới cho tôi (bao gồm: `sharedStoreTestUtils.ts`, `crudTests.ts`, `family.store.spec.ts`, `member.store.spec.ts`).
