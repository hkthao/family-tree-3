
---

## 📌 MÔ TẢ NHIỆM VỤ REFACTOR BACKEND DOMAIN (CHO GEMINI CLI)

### 🎯 Mục tiêu

Refactor domain `Event` để **hỗ trợ lịch dương, lịch âm (VN) và lặp theo năm**, phục vụ UI calendar custom (React Native).
Giữ đúng tư duy **DDD, Aggregate Root, Value Object**, tránh logic hiển thị trong domain.

---

## 1️⃣ Bối cảnh hiện tại

* Backend dùng **C# / .NET / EF Core**
* Domain có `Event` là **Aggregate Root**
* Hiện đang lưu:

  ```csharp
  DateTime? StartDate;
  DateTime? EndDate;
  ```
* Cách này **KHÔNG phù hợp** cho:

  * Lịch âm
  * Sự kiện lặp theo năm (giỗ, sinh nhật)
  * Convert âm → dương theo từng năm

---

## 2️⃣ Yêu cầu refactor (bắt buộc)

### 2.1 Loại bỏ khái niệm “ngày hiển thị” khỏi domain

* ❌ Không dùng `StartDate`, `EndDate` cho event âm
* ✅ Domain chỉ lưu **ngày gốc (source of truth)**

---

### 2.2 Bổ sung Enum

```csharp
public enum CalendarType
{
    Solar = 1,
    Lunar = 2
}

public enum RepeatRule
{
    None = 0,
    Yearly = 1
}
```

---

### 2.3 Tạo Value Object cho ngày ÂM

```csharp
public class LunarDate : ValueObject
{
    public int Day { get; private set; }
    public int Month { get; private set; }
    public bool IsLeapMonth { get; private set; }

    private LunarDate() { }

    public LunarDate(int day, int month, bool isLeapMonth)
    {
        Day = day;
        Month = month;
        IsLeapMonth = isLeapMonth;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Day;
        yield return Month;
        yield return IsLeapMonth;
    }
}
```

* `LunarDate` là **Value Object**
* EF Core phải map được (owned entity)

---

## 3️⃣ Refactor Event Aggregate Root

### 3.1 Nguyên tắc

* Một Event **chỉ có 1 loại lịch**
* Không được vừa có SolarDate vừa có LunarDate
* Logic convert **KHÔNG đặt trong Entity**

---

### 3.2 Cấu trúc Event sau refactor

```csharp
public class Event : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }

    public CalendarType CalendarType { get; private set; }

    // Chỉ dùng cho Solar event
    public DateTime? SolarDate { get; private set; }

    // Chỉ dùng cho Lunar event
    public LunarDate? LunarDate { get; private set; }

    public RepeatRule RepeatRule { get; private set; }

    public EventType Type { get; private set; }
    public string? Color { get; private set; }

    public Guid? FamilyId { get; private set; }
    public Family? Family { get; private set; }

    private readonly HashSet<EventMember> _eventMembers = new();
    public IReadOnlyCollection<EventMember> EventMembers => _eventMembers;

    private Event() { }
}
```

---

### 3.3 Factory methods (bắt buộc)

```csharp
public static Event CreateSolarEvent(
    string name,
    string code,
    EventType type,
    DateTime solarDate,
    RepeatRule repeatRule,
    Guid? familyId
)

public static Event CreateLunarEvent(
    string name,
    string code,
    EventType type,
    LunarDate lunarDate,
    RepeatRule repeatRule,
    Guid? familyId
)
```

* Không cho phép `new Event()` từ bên ngoài
* Validate:

  * Solar → chỉ có SolarDate
  * Lunar → chỉ có LunarDate

---

## 4️⃣ Những thứ KHÔNG được làm

❌ Không:

* Convert lunar → solar trong Entity
* Sinh event theo năm trong Entity
* Thêm logic UI / calendar vào domain

👉 Những việc này thuộc **Application / Domain Service**

---

## 5️⃣ Chuẩn bị cho bước tiếp theo (chỉ để định hướng)

Sau refactor, backend sẽ có:

* `EventOccurrenceService`:

  * Input: Event + year
  * Output: danh sách **solar dates để hiển thị**
* API:

  ```
  GET /events/calendar?year=YYYY&month=MM
  ```

(UI chỉ render, không xử lý âm lịch)

---

## 6️⃣ Tiêu chí hoàn thành (Definition of Done)

* Event hỗ trợ:

  * Dương lịch
  * Âm lịch (ngày + tháng + tháng nhuận)
  * Lặp theo năm
* Không còn phụ thuộc vào `StartDate/EndDate`
* Domain đúng DDD:

  * Aggregate Root
  * Value Object
* EF Core mapping hợp lệ

---

## 7️⃣ Lưu ý quan trọng

* Ưu tiên **refactor tối thiểu**, không phá EventMember
* Giữ nguyên EventType, FamilyId
* Có thể cần migration DB (ghi chú nếu cần)

---

## 🔥 Ghi chú cho Gemini CLI

> Đây là refactor **domain-level**, không phải UI
> Hãy ưu tiên tính đúng đắn, khả năng mở rộng cho lịch âm VN
> Không tối ưu premature, không thêm logic convert

---

