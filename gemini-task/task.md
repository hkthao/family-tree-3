gemini update --repo . --ask "Repo hiện tại đang có nhiều code xử lý lỗi trực tiếp bằng try/catch trong các store, service và API client. Anh hãy refactor toàn bộ code để sử dụng Result/Either pattern (có thể tham khảo cách implement trong fp-ts hoặc tự viết một utility Result<T, E>).

Yêu cầu chi tiết:

Tạo một Result type:
type Result<T, E> = { ok: true; value: T } | { ok: false; error: E };
hoặc dùng kiểu tương tự (Either cũng được).

Trong tất cả các service (ví dụ MemberService, FamilyService…), khi gọi API:

Thay vì throw error, hãy luôn trả về Result.

Ví dụ: return { ok: true, value: data } hoặc return { ok: false, error }.

Trong các store (Pinia stores), thay đổi logic:

Khi nhận Result từ service, check if (result.ok) để set state.

Nếu !result.ok, hãy gán error state chuẩn, thay vì để exception propagate.

Code store cần gọn gàng hơn, không bị lặp đi lặp lại phần try/catch.

Viết lại unit test tương ứng:

Test cả case ok và case error.

Đảm bảo store update state đúng khi service trả về error.

Áp dụng cho toàn bộ store chính trong repo (family.store, member.store, v.v.), và service liên quan.

Code sau refactor cần:

Type-safe (không dùng any).

Giảm lặp lại trong xử lý lỗi.

Đảm bảo test (npx vitest run) vẫn pass."