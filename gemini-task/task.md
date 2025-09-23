Bạn là 1 senior frontend engineer. Hãy tạo 1 Vue 3 component dùng chung tên là `Lookup.vue`.  

Yêu cầu:
1. Props:
   - `dataSource`: có thể là 
       a) Pinia store (có state chứa list, ví dụ familyStore.families) 
       b) Array<any>
   - `value`: id hiện tại (string | number | null).
   - `displayExpr`: tên field để hiển thị (ví dụ "name").
   - `valueExpr`: tên field dùng làm value (ví dụ "id").
2. Behavior:
   - Nếu `dataSource` là store: 
     - Ưu tiên lấy từ state (ví dụ `store.families`).
     - Nếu rỗng thì gọi hàm `fetch...()` của store nếu có.
   - Nếu `dataSource` là array: dùng trực tiếp.
3. Render:
   - Component render `<select>` với danh sách option.
   - Option `value` = `valueExpr`, hiển thị = `displayExpr`.
   - Hỗ trợ `v-model:value` 2 chiều.
4. Viết code bằng TypeScript, có type rõ ràng, dễ mở rộng.
5. áp dụng cho MemberList.vue
6. Comment đầy đủ để junior dễ hiểu.
