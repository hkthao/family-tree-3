gemini generate ui --framework vue --library vuetify --task "
Refactor frontend để tách data layer với Pinia, hỗ trợ mock data khi dev và API thật khi deploy.

### Yêu cầu
1. **Pinia Stores**
   - Tạo store cho từng domain chính:
     - `useFamiliesStore`: quản lý gia đình (families).
     - `useMembersStore`: quản lý thành viên (members).
     - `useFamilyEventsStore`: quản lý sự kiện gia đình (family events).
   - Mỗi store có state:
     - `items`: danh sách entity
     - `loading`: boolean
     - `total`: number
     - `error`: string | null
   - Actions chuẩn hóa:
     - `fetchAll(search, page, perPage)`
     - `add(entity)`
     - `update(entity)`
     - `delete(id)`
   - **Switch logic**:
     - Nếu `VITE_USE_MOCK=true` → trả mock data (JSON hoặc object cứng trong `data/mock/`).
     - Nếu `VITE_USE_MOCK=false` → gọi API thật (axios/fetch).

2. **Mock Data**
   - Tạo file trong `data/mock/`:
     - `families.mock.ts`
     - `members.mock.ts`
     - `familyEvents.mock.ts`
   - Dữ liệu mẫu có đầy đủ field cần thiết để test UI.

3. **API Integration**
   - Gọi API thật khi production:
     - GET `/api/families`, `/api/members`, `/api/family-events`
     - POST, PUT, DELETE theo chuẩn REST.
   - Axios config riêng trong `plugins/axios.ts`.

4. **Env Config**
   - `.env.development`: `VITE_USE_MOCK=true`
   - `.env.production`: `VITE_USE_MOCK=false`

### Output mong muốn
- Source code Vue 3 + Vuetify + Pinia refactor xong.
- Có thể chạy dev với mock data.
- Khi build production sẽ tự động kết nối API thật.
- Code clean, dễ mở rộng, chuẩn enterprise.
"
