# Quy ước Mã hóa (Code Conventions)

Tài liệu này trình bày các quy ước và hướng dẫn về mã hóa cho dự án "Cây Gia Phả". Việc tuân thủ các quy ước này giúp đảm bảo tính nhất quán, dễ đọc, dễ bảo trì và dễ cộng tác trong quá trình phát triển phần mềm.

## 1. Nguyên tắc Chung

*   **Tính nhất quán:** Luôn ưu tiên tính nhất quán với mã nguồn hiện có trong dự án.
*   **Dễ đọc:** Mã phải dễ đọc và dễ hiểu đối với bất kỳ lập trình viên nào.
*   **Ngắn gọn và rõ ràng:** Viết mã ngắn gọn nhưng vẫn đảm bảo sự rõ ràng về mục đích và chức năng.
*   **DRY (Don't Repeat Yourself):** Tránh lặp lại mã. Tái sử dụng các thành phần hoặc chức năng đã có.
*   **SOLID Principles:** Cố gắng áp dụng các nguyên tắc SOLID khi thiết kế và viết mã.
*   **Testable Code:** Viết mã sao cho dễ dàng kiểm thử.

## 2. Quy ước Đặt tên

### 2.1. Đặt tên chung

*   Sử dụng tiếng Anh rõ ràng, mô tả.
*   Tránh viết tắt trừ khi đó là từ viết tắt phổ biến và được chấp nhận rộng rãi (ví dụ: `Id`, `Dto`).

### 2.2. C# (Backend)

*   **Classes, Interfaces, Enums:** `PascalCase` (ví dụ: `FamilyMember`, `IFamilyService`).
*   **Methods, Properties:** `PascalCase` (ví dụ: `GetFamilyMembers()`, `MemberName`).
*   **Local Variables, Parameters:** `camelCase` (ví dụ: `familyMember`, `id`).
*   **Private Fields:** `_camelCase` hoặc `camelCase` (ví dụ: `_familyRepository` hoặc `familyRepository`), nhất quán trong toàn bộ dự án.
*   **Constants:** `PascalCase` hoặc `UPPER_SNAKE_CASE` (ví dụ: `MaxFamilyMembers` hoặc `MAX_FAMILY_MEMBERS`), nhất quán.
*   **Namespaces:** `PascalCase` (ví dụ: `FamilyTree.Application.FamilyMembers`).

### 2.3. TypeScript/Vue.js (Frontend)

*   **Components:** `PascalCase` cho tên tệp và tên component (ví dụ: `FamilyMemberCard.vue`, `UserList.vue`).
*   **Variables, Functions:** `camelCase` (ví dụ: `familyMember`, `getFamilyMembers()`).
*   **Constants:** `UPPER_SNAKE_CASE` (ví dụ: `API_BASE_URL`, `MAX_ITEMS_PER_PAGE`).
*   **Interfaces, Types:** `PascalCase` hoặc bắt đầu bằng `I` (ví dụ: `IFamilyMember`, `FamilyMember`). Nhất quán với style hiện có trong `src/types/`.
*   **Store Modules:** `camelCase` cho tên tệp và tên module (ví dụ: `familyStore.ts`).

## 3. Định dạng

### 3.1. Thụt lề (Indentation)

*   Sử dụng 4 dấu cách (spaces) cho thụt lề. KHÔNG sử dụng Tab.
*   Đảm bảo cấu hình Editor/IDE của bạn sử dụng 4 dấu cách cho thụt lề.

### 3.2. Độ dài dòng (Line Length)

*   Cố gắng giữ độ dài dòng dưới 120 ký tự để dễ đọc, đặc biệt khi xem mã trên màn hình nhỏ hoặc trong các công cụ so sánh (diff tools).

### 3.3. Dấu ngoặc nhọn (Curly Braces)

*   Sử dụng kiểu dấu ngoặc nhọn K&R cho C# (dấu mở ngoặc trên cùng một dòng với khai báo, dấu đóng ngoặc trên dòng riêng).
*   Ví dụ C#:
    ```csharp
    public class MyClass
    {
        public void MyMethod()
        {
            // code
        }
    }
    ```
*   Sử dụng kiểu Allman cho TypeScript/JavaScript (dấu mở ngoặc trên dòng riêng).
*   Ví dụ TypeScript:
    ```typescript
    function myFunction()
    {
        // code
    }
    ```
    Hoặc phù hợp với cấu hình Prettier/ESLint của dự án.

### 3.4. Khoảng trắng (Whitespace)

*   Sử dụng khoảng trắng một cách hợp lý để tăng cường khả năng đọc.
*   Có một khoảng trắng sau dấu phẩy (`,`) và dấu hai chấm (`:`).
*   Có một khoảng trắng trước và sau các toán tử (ví dụ: `+`, `-`, `=`, `==`).
*   Không có khoảng trắng thừa ở cuối dòng.

## 4. Chú thích (Commenting)

*   **Khi nào viết chú thích:**
    *   Giải thích lý do _tại sao_ một đoạn mã được viết theo cách nào đó, không phải _nó làm gì_ (điều này nên rõ ràng từ mã).
    *   Giải thích các thuật toán phức tạp hoặc logic nghiệp vụ không rõ ràng.
    *   Cảnh báo về các "gotchas" hoặc các trường hợp đặc biệt.
    *   Tạo tài liệu cho các API công cộng (sử dụng XML comments cho C#, JSDoc cho TypeScript).
*   **Khi nào không viết chú thích:**
    *   Không viết chú thích cho mã tự giải thích.
    *   Không để lại các chú thích mã cũ, không sử dụng.
*   **Ngôn ngữ:** Ưu tiên tiếng Việt cho các chú thích nội bộ, tiếng Anh cho tài liệu API công khai hoặc các phần mã có thể được tái sử dụng rộng rãi.

## 5. Xử lý Lỗi (Error Handling)

*   Sử dụng cơ chế xử lý lỗi phù hợp với từng môi trường (ví dụ: `try-catch` cho C#, `Result<T, E>` cho API/Service tầng ứng dụng).
*   Xử lý các ngoại lệ một cách duyên dáng và cung cấp thông báo lỗi có ý nghĩa cho người dùng hoặc log để gỡ lỗi.
*   Tránh bắt các ngoại lệ chung chung (`Exception` trong C#) trừ khi bạn có ý định xử lý chúng một cách cụ thể.
*   Frontend nên hiển thị thông báo lỗi thân thiện với người dùng và log lỗi chi tiết cho nhà phát triển.

## 6. Các Cân nhắc về Hiệu suất

*   Tối ưu hóa các truy vấn cơ sở dữ liệu.
*   Giảm thiểu các yêu cầu mạng.
*   Sử dụng lazy loading hoặc phân trang khi làm việc với lượng dữ liệu lớn.
*   Tránh các vòng lặp lồng nhau không cần thiết trong các đoạn mã nhạy cảm về hiệu suất.

## 7. Các Cân nhắc về Bảo mật

*   **Xác thực và Ủy quyền:** Luôn kiểm tra quyền truy cập của người dùng trước khi thực hiện các hành động nhạy cảm.
*   **Input Validation:** Luôn kiểm tra và làm sạch dữ liệu đầu vào từ người dùng để ngăn chặn các cuộc tấn công như SQL Injection, XSS.
*   **Sensitive Data:** Không lưu trữ dữ liệu nhạy cảm trong mã nguồn hoặc log. Mã hóa dữ liệu nhạy cảm khi lưu trữ.
*   **Logging:** Tránh log thông tin nhạy cảm của người dùng.

## 8. Cụ thể cho C# (Backend)

*   **Async/Await:** Sử dụng `async/await` cho các thao tác không đồng bộ để giữ cho ứng dụng phản hồi nhanh. Luôn sử dụng `ConfigureAwait(false)` khi có thể để tránh deadlock trong các ứng dụng GUI.
*   **LINQ:** Sử dụng LINQ một cách hiệu quả để truy vấn và thao tác dữ liệu.
*   **Dependency Injection:** Sử dụng Dependency Injection (DI) để quản lý các phụ thuộc và tăng cường tính module hóa và khả năng kiểm thử. Tránh sử dụng Service Locator pattern.
*   **Clean Architecture:** Tuân thủ các lớp của Clean Architecture (Domain, Application, Infrastructure, Web) để tách biệt các mối quan tâm.
    *   **Domain:** Chứa các thực thể (Entities), đối tượng giá trị (Value Objects), domain events, và business rules.
    *   **Application:** Chứa các Command, Query, Handler, Interfaces cho các dịch vụ ứng dụng.
    *   **Infrastructure:** Chứa việc triển khai các interfaces từ Application, tích hợp với cơ sở dữ liệu (EF Core), hệ thống tệp, dịch vụ bên ngoài.
    *   **Web:** Chứa các Controllers, DTOs, cấu hình web.

## 9. Cụ thể cho TypeScript/Vue.js (Frontend)

*   **Cấu trúc Component:**
    *   Mỗi component Vue nên được định nghĩa trong một tệp `.vue` riêng biệt.
    *   Sử dụng Composition API với `<script setup>` để tăng cường khả năng đọc và tái sử dụng logic.
    *   Props nên được định nghĩa rõ ràng với kiểu dữ liệu và giá trị mặc định.
    *   Emit events rõ ràng.
*   **State Management (Pinia):**
    *   Sử dụng Pinia để quản lý trạng thái toàn cục của ứng dụng.
    *   Mỗi module store nên có tệp riêng trong `stores/`.
    *   Actions nên là `async` nếu chúng thực hiện các yêu cầu API.
*   **API Calls (`apiClient`):**
    *   Sử dụng `apiClient` tùy chỉnh (được cấu hình với Axios) để gọi API backend.
    *   Xử lý lỗi một cách nhất quán trong `apiClient` hoặc ở lớp Service.
    *   Luôn trả về kiểu `Result<T, ApiError>` từ các service API để xử lý lỗi một cách đồng nhất.
*   **Styling (Vuetify):**
    *   Sử dụng các component và tiện ích của Vuetify.
    *   Viết CSS tùy chỉnh (nếu cần) trong phạm vi `<style scoped>` của component hoặc trong các tệp CSS/SCSS riêng biệt theo quy ước của dự án.
*   **Vue Reactivity:**
    *   Sử dụng `ref` và `reactive` một cách thích hợp.
    *   Hiểu rõ cách Vue phản ứng với các thay đổi dữ liệu để tránh các vấn đề về hiệu suất hoặc cập nhật UI không mong muốn.

## 10. Quy ước Git

*   **Branching Strategy:** Sử dụng Gitflow hoặc một chiến lược phân nhánh tương tự (ví dụ: `main`, `develop`, `feature/*`, `bugfix/*`, `release/*`).
*   **Commit Messages:**
    *   Viết commit message bằng tiếng Anh.
    *   Dòng đầu tiên ngắn gọn (dưới 50 ký tự), mô tả thay đổi.
    *   Dòng thứ hai trống.
    *   Các dòng tiếp theo giải thích chi tiết hơn về _tại sao_ thay đổi được thực hiện, các tác động, và các tham chiếu đến các vấn đề (issues) (ví dụ: `Fix #123`).
    *   Sử dụng thì hiện tại, thể mệnh lệnh (ví dụ: "Add feature X", "Fix bug Y", thay vì "Added feature X", "Fixes bug Y").
    *   Ví dụ:
        ```
        feat: Add user profile page

        This commit introduces a new user profile page, allowing users to view
        and edit their personal information.

        - Implemented `UserProfileView.vue`
        - Added `GET /api/users/{id}` endpoint
        - Updated `router/index.ts` to include new route

        Refs #456
        ```
*   **Pull Requests (PRs):**
    *   Mô tả rõ ràng các thay đổi.
    *   Bao gồm các bước kiểm thử hoặc cách tái hiện lỗi (nếu là fix).
    *   Tham chiếu các issues liên quan.

---
**Lưu ý:** Các quy ước này có thể được điều chỉnh và bổ sung trong quá trình phát triển dự án. Mọi thành viên trong nhóm nên thảo luận và thống nhất về bất kỳ thay đổi nào.
