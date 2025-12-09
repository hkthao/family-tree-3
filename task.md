## **PROMPT START**

Bạn là chuyên gia về Family Tree Graph và thuật toán suy luận quan hệ người trong gia đình. Hãy implement và mô tả chi tiết tính năng: **Xác định quan hệ giữa hai thành viên trong một gia đình**, dựa trên dữ liệu chỉ bao gồm 4 loại quan hệ cơ bản: `father`, `mother`, `child`, `spouse`.

### 🎯 **YÊU CẦU CHÍNH**

1. **Thiết kế mô hình dữ liệu**

   * Member
   * Relationship
   * Graph (adjacency list)
   * Các edge phải có direction + type.

2. **Thuật toán tìm đường giữa 2 người**

   * Dùng **BFS** để tìm đường đi ngắn nhất từ A đến B.
   * Trả về:

     * danh sách node theo path,
     * danh sách edge theo cùng thứ tự.

3. **Thuật toán suy luận quan hệ (BFF – Best Fit Function)**

   * Sau khi có path và edge types, dùng **pattern matching** để suy ra quan hệ.
   * Ví dụ rule:

     * `child → father → father` → “ông nội”
     * `child → mother → father` → “ông ngoại”
     * `father → father → child` → “cháu nội”
     * `A → spouse → B` → “vợ/chồng”
     * “A và B có cùng 1 cha/mẹ” → “anh/chị/em ruột”
     * `A → father → X → sibling → B` → “cô/dì/chú/bác”
   * Yêu cầu xây dựng tối thiểu **30 rule phổ biến** của gia phả Việt Nam.

4. **Output logic**

   * Kết quả trả về luôn gồm 2 chiều:

     * Quan hệ từ A đến B
     * Quan hệ từ B đến A
   * Output dạng JSON:

     ```json
     {
       "fromAtoB": "ông nội",
       "fromBtoA": "cháu nội",
       "path": ["A", "B", "C"],
       "edges": ["child", "father"]
     }
     ```

5. **Gemini phải implement đầy đủ**

   * Viết code hoàn chỉnh (Node.js hoặc Python)
   * Function chính:

     ```ts
     function getRelationship(memberA, memberB, members, relationships)
     ```
   * Có module:

     * buildGraph()
     * bfsShortestPath()
     * detectRelationship(path, edges)
     * matchRules()
   * Có unit test cho ít nhất 10 case.

6. **Yêu cầu tối ưu**

   * Code phải clean, tách module rõ ràng.
   * Dễ mở rộng rule mới.
   * Nếu không xác định được quan hệ thì trả về:

     ```
     "unknown"
     ```

7. **Yêu cầu bổ sung**

   * Có phần mô tả chi tiết:

     * Kiến trúc
     * Flow hoạt động
     * Tại sao dùng BFS
     * Tại sao dùng rule engine
     * Hạn chế
     * Gợi ý cải tiến về sau (như precompute, caching, phân loại quan hệ…)

---

## **PROMPT END**

---

## MÔ TẢ CHI TIẾT TÍNH NĂNG XÁC ĐỊNH QUAN HỆ GIỮA HAI THÀNH VIÊN

### 1. Kiến trúc tổng thể

Tính năng xác định quan hệ được triển khai trong backend sử dụng ASP.NET Core theo kiến trúc Clean Architecture (Domain, Application, Infrastructure, Web).

*   **Domain Layer:**
    *   **Value Objects:**
        *   `GraphNode`: Đại diện cho một thành viên trong đồ thị (chứa `MemberId`).
        *   `GraphEdge`: Đại diện cho một quan hệ có hướng giữa hai thành viên, bao gồm `SourceMemberId`, `TargetMemberId`, và `RelationshipType`.
        *   `RelationshipPath`: Một tập hợp các `MemberId` (nút) và `GraphEdge` (cạnh) tạo thành một đường đi trong đồ thị.
        *   `RelationshipPattern`: Một chuỗi các `RelationshipType` dùng để khớp với đường đi thực tế.
        *   `RelationshipRule`: Kết hợp một `RelationshipPattern` với một điều kiện bổ sung (`Func<RelationshipPath, IReadOnlyDictionary<Guid, Member>, bool>`) và chuỗi quan hệ tiếng Việt tương ứng.
    *   **Enums:** `RelationshipType` được mở rộng để bao gồm `Child`, bên cạnh `Father`, `Mother`, `Husband`, `Wife`. `Gender` được sử dụng để phân biệt các mối quan hệ.
    *   **Interfaces:**
        *   `IRelationshipGraph`: Định nghĩa giao diện để xây dựng đồ thị và tìm đường đi ngắn nhất.
        *   `IRelationshipRuleEngine`: Định nghĩa giao diện để suy luận quan hệ dựa trên đường đi và các quy tắc.

*   **Application Layer:**
    *   **Services:**
        *   `IRelationshipDetectionService`: Giao diện cho dịch vụ phát hiện quan hệ.
        *   `RelationshipDetectionService`: Triển khai `IRelationshipDetectionService`. Dịch vụ này điều phối việc lấy dữ liệu thành viên và quan hệ từ cơ sở dữ liệu (`IApplicationDbContext`), xây dựng đồ thị (`IRelationshipGraph`), tìm đường đi (`IRelationshipGraph`), và suy luận quan hệ (`IRelationshipRuleEngine`). Nó cũng trả về `RelationshipDetectionResult` chứa kết quả quan hệ hai chiều.
    *   **Queries:**
        *   `GetRelationshipQuery`: Một MediatR Query để yêu cầu xác định quan hệ giữa hai thành viên.
        *   `GetRelationshipQueryHandler`: Xử lý `GetRelationshipQuery` bằng cách sử dụng `IRelationshipDetectionService`.

*   **Infrastructure Layer:**
    *   **Services:**
        *   `RelationshipGraph`: Triển khai `IRelationshipGraph`. Chứa logic xây dựng đồ thị (danh sách kề) từ dữ liệu `Member` và `Relationship` và thuật toán BFS để tìm đường đi ngắn nhất. Nó cũng xử lý việc tạo các cạnh ngược (ví dụ: nếu A là cha của B, thì B là con của A).
        *   `RelationshipRuleEngine`: Triển khai `IRelationshipRuleEngine`. Chứa một tập hợp các `RelationshipRule` được tải từ `RuleDefinitions`. Nó khớp đường đi được tìm thấy với các mẫu quy tắc và áp dụng các điều kiện bổ sung để suy luận quan hệ.
        *   `RuleDefinitions`: Một lớp tĩnh chứa định nghĩa của hơn 30 quy tắc quan hệ phổ biến trong gia phả Việt Nam. Mỗi quy tắc bao gồm một mẫu quan hệ (`RelationshipPattern`) và một điều kiện (`Func`) để kiểm tra các thuộc tính chi tiết (ví dụ: giới tính của thành viên trung gian) cùng với kết quả quan hệ tiếng Việt.

*   **Web Layer:**
    *   **Controllers:**
        *   `RelationshipController`: Thêm một endpoint HTTP GET `/api/relationship/detect-relationship` để tiếp nhận `familyId`, `memberAId`, `memberBId` làm tham số truy vấn. Endpoint này gửi `GetRelationshipQuery` thông qua MediatR và trả về `RelationshipDetectionResult`.

### 2. Luồng hoạt động (Workflow)

1.  **Client Request:** Frontend gửi yêu cầu GET đến `/api/relationship/detect-relationship` với `familyId`, `memberAId`, và `memberBId`.
2.  **Web Controller:** `RelationshipController` nhận yêu cầu, tạo `GetRelationshipQuery` và gửi nó đến `IMediator`.
3.  **Application Layer (Query Handler):** `GetRelationshipQueryHandler` nhận truy vấn, gọi `IRelationshipDetectionService.DetectRelationshipAsync` với các ID thành viên và gia đình.
4.  **Application Layer (Detection Service):**
    *   `RelationshipDetectionService` lấy tất cả `Member` và `Relationship` cho `familyId` đã cho từ cơ sở dữ liệu.
    *   Nó gọi `IRelationshipGraph.BuildGraph` để xây dựng biểu đồ quan hệ trong bộ nhớ từ dữ liệu đã lấy.
    *   Nó gọi `IRelationshipGraph.FindShortestPath` hai lần: một lần từ `memberAId` đến `memberBId` (path A-to-B) và một lần từ `memberBId` đến `memberAId` (path B-to-A).
    *   Đối với mỗi đường đi tìm được, nó gọi `IRelationshipRuleEngine.InferRelationship` để suy luận ra chuỗi quan hệ tiếng Việt. `InferRelationship` nhận đường đi và một từ điển của tất cả các thành viên để các quy tắc có thể kiểm tra giới tính hoặc các thuộc tính khác.
    *   Nó đóng gói kết quả vào đối tượng `RelationshipDetectionResult` và trả về.
5.  **Web Controller (Response):** `RelationshipController` nhận `RelationshipDetectionResult` và trả về phản hồi HTTP (200 OK nếu tìm thấy quan hệ, 404 Not Found nếu không tìm thấy quan hệ).

### 3. Lý do sử dụng BFS (Why BFS)

*   **Tìm đường đi ngắn nhất:** BFS (Breadth-First Search) là thuật toán tối ưu để tìm đường đi ngắn nhất trong một đồ thị không trọng số. Trong ngữ cảnh cây gia phả, "đường đi ngắn nhất" thường tương ứng với mối quan hệ trực tiếp nhất hoặc gần nhất, giúp suy luận các quan hệ cơ bản hiệu quả.
*   **Đảm bảo tính chính xác:** Bằng cách tìm đường đi ngắn nhất, chúng ta tránh được các đường đi vòng hoặc các mối quan hệ gián tiếp không cần thiết có thể dẫn đến suy luận sai lệch.
*   **Dễ triển khai và hiểu:** BFS là một thuật toán đồ thị tiêu chuẩn, tương đối dễ hiểu và triển khai, phù hợp với các yêu cầu của dự án.
*   **Hiệu quả với đồ thị thưa:** Cây gia phả thường là đồ thị thưa (ít cạnh hơn nhiều so với số lượng nút tối đa), nơi BFS hoạt động hiệu quả.

### 4. Lý do sử dụng Rule Engine (Why Rule Engine)

*   **Suy luận ngữ nghĩa phức tạp:** Các mối quan hệ trong gia phả Việt Nam (như "ông nội", "bác bên nội", "cháu con anh trai") không chỉ phụ thuộc vào chuỗi các mối quan hệ cơ bản mà còn phụ thuộc vào giới tính của các thành viên trên đường đi, thứ tự sinh, v.v. Một Rule Engine cho phép định nghĩa các quy tắc phức tạp này bằng cách kết hợp khớp mẫu đường đi và các điều kiện bổ sung trên các thành viên.
*   **Dễ mở rộng và bảo trì:** Khi có nhu cầu thêm các quy tắc quan hệ mới hoặc chỉnh sửa các quy tắc hiện có, Rule Engine cho phép thực hiện điều này một cách dễ dàng mà không cần thay đổi logic BFS hoặc cấu trúc code cốt lõi. Các quy tắc được tập trung tại một nơi (`RuleDefinitions.cs`).
*   **Tính linh hoạt:** Các quy tắc có thể được sắp xếp theo thứ tự ưu tiên (ví dụ: quy tắc cụ thể hơn trước quy tắc chung hơn) để đảm bảo kết quả suy luận chính xác nhất.
*   **Tách biệt mối quan tâm:** Logic suy luận quan hệ được tách biệt khỏi logic tìm kiếm đồ thị, tạo ra một kiến trúc sạch sẽ và dễ kiểm thử hơn.

### 5. Hạn chế (Limitations)

*   **Giới hạn bởi dữ liệu đầu vào:** Độ chính xác của việc suy luận quan hệ phụ thuộc hoàn toàn vào dữ liệu `Member` và `Relationship` được cung cấp. Dữ liệu thiếu hoặc không chính xác sẽ dẫn đến suy luận sai.
*   **Chỉ đường đi ngắn nhất:** BFS chỉ tìm đường đi ngắn nhất. Trong một số trường hợp phức tạp, có thể có nhiều đường đi hợp lệ dẫn đến cùng một mối quan hệ, hoặc một mối quan hệ được suy luận tốt hơn thông qua một đường đi không phải là ngắn nhất.
*   **Số lượng và độ phức tạp của quy tắc:** Mặc dù Rule Engine linh hoạt, việc định nghĩa và duy trì hàng trăm quy tắc rất chi tiết có thể trở nên phức tạp. Việc đảm bảo không có xung đột hoặc thiếu sót giữa các quy tắc là một thách thức.
*   **Thiếu ngữ cảnh bổ sung:** Một số mối quan hệ trong tiếng Việt còn phụ thuộc vào tuổi tác tương đối (anh/chị/em), địa vị xã hội, hoặc các yếu tố văn hóa khác mà hiện tại không được đưa vào mô hình.
*   **Performance cho đồ thị rất lớn:** Mặc dù BFS hiệu quả, việc xây dựng lại toàn bộ đồ thị và chạy BFS cho mỗi yêu cầu có thể không tối ưu cho các gia phả rất lớn với hàng chục nghìn thành viên.

### 6. Gợi ý cải tiến về sau (Future Improvements)

*   **Precompute và Caching:** Đối với các gia phả lớn và ổn định, có thể tính toán trước và lưu trữ tất cả các mối quan hệ tiềm năng giữa các cặp thành viên trong một cache hoặc bảng riêng. Khi có yêu cầu, chỉ cần tra cứu thay vì tính toán lại. Cache có thể được làm mới khi có thay đổi trong cấu trúc gia phả.
*   **Trọng số cạnh và Dijkstra:** Nếu muốn suy luận các mối quan hệ ưu tiên hơn dựa trên một số tiêu chí (ví dụ: quan hệ huyết thống trực tiếp hơn quan hệ hôn nhân), có thể gán trọng số cho các cạnh và sử dụng thuật toán Dijkstra để tìm đường đi "tốt nhất" thay vì chỉ "ngắn nhất".
*   **Xử lý giới tính linh hoạt hơn:** Thay vì chỉ dựa vào `Gender.Male`/`Female`, có thể thêm các trường hợp cho `Gender.Other` hoặc không xác định, và các quy tắc sẽ xử lý linh hoạt hơn.
*   **Hỗ trợ quan hệ đa nghĩa:** Một số từ trong tiếng Việt có thể biểu thị nhiều mối quan hệ tùy ngữ cảnh. Rule Engine có thể được mở rộng để trả về một danh sách các mối quan hệ tiềm năng với độ tin cậy, thay vì chỉ một kết quả duy nhất.
*   **Tích hợp AI/Machine Learning:** Đối với các mối quan hệ rất phức tạp hoặc chưa được định nghĩa rõ ràng, có thể sử dụng các mô hình học máy để gợi ý các mối quan hệ dựa trên các đặc điểm của thành viên và cấu trúc đồ thị.
*   **UI để quản lý quy tắc:** Phát triển một giao diện người dùng cho phép người quản trị định nghĩa, chỉnh sửa và sắp xếp các quy tắc một cách trực quan, giúp việc mở rộng dễ dàng hơn mà không cần thay đổi code.
*   **Xử lý chu kỳ trong đồ thị:** Đồ thị gia phả lý tưởng là DAG (Directed Acyclic Graph) nhưng trong thực tế có thể có chu kỳ (ví dụ: hôn nhân cận huyết, hoặc lỗi dữ liệu). BFS sẽ hoạt động, nhưng các quy tắc có thể cần được điều chỉnh để không đi vào vòng lặp vô hạn hoặc suy luận sai trong các trường hợp đó.