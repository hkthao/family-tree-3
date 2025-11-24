# 1 — Tổng quan ngắn

**Story Memory** là một sub-feature trong *AI Memorial Studio*.
Chức năng chính: người dùng tạo nhiều *memory* cho 1 member; mỗi memory có thể kèm ảnh (optional), raw text input và được AI chuyển thành story hoàn chỉnh. Kết quả lưu vào DB, hiển thị ở profile member và timeline.

Key flows:

1. Create Memory (Upload image optional + raw text + chọn style)
2. Photo Analysis (vision → JSON context)
3. Generate Story (LLM sử dụng rawText + photoAnalysis + style)
4. Edit & Save
5. List / View / Delete memories

---

# 2 — Nhiệm vụ Gemini CLI phải sinh/implement

(Theo thứ tự ưu tiên)

1. Frontend pages & components (Vue):

   * Route: `/members/:memberId/memories`
   * Components: `MemoryList.vue`, `MemoryCreate.vue`, `PhotoAnalyzerPreview.vue`, `StoryEditor.vue`, `MemoryDetail.vue`, `BeforeAfterSlider.vue`
2. Backend scaffold (ASP.NET Core WebAPI):

   * Controller: `MemoriesController` với endpoints (see API section)
   * Services: `IPhotoAnalysisService`, `IStoryGenerationService`, `IMemoryService`
   * Models / DTOs / EF Core Entities (see Data Model)
3. DB migration (EF Core) cho `Memory` & `PhotoAnalysisResult`
4. AI integration helpers:

   * `VisionClient` wrapper (calls Vision model or internal service)
   * `LLMClient` wrapper (calls GenAI for story generation)
   * Retry, timeout, rate-limit handling
5. Prompts files for Vision & Story (store under `/ai/prompts/*.md`)
6. Unit tests skeleton + basic e2e test plan
7. Basic UI i18n keys (vi/en)

---

# 3 — UI/UX Flow (chi tiết, đủ để dev code)

## A. Memory List (Route: `/members/:memberId/memories`)

* Header: `Avatar | Name | (Add Memory button)`
* Shows cards: title, 1-line excerpt, date, tags, thumbnail
* Action: View / Edit / Delete

## B. Create Memory (Modal hoặc Page `/members/:id/memories/new`)

Step 1 — Choose Photo (optional)

* Drag-drop or select file (accept: jpg/png/heic)
* Preview thumbnail
* Buttons: `Analyze Photo` | `Skip to Text`

Step 2 — Photo Analysis (if user clicked Analyze)

* Call `/api/memory/analyze-photo` with file form-data
* Show `PhotoAnalyzerPreview`:

  * Scene (e.g., "family gathering")
  * Persons detected (matched to member or unknown)
  * Objects list
  * Year estimate
  * One-line summary (AI)
  * Buttons: `Use this context` / `Edit` / `Skip`

Step 3 — Raw Text Input

* Textarea placeholder: “Kể lại điều bạn nhớ về khoảnh khắc này…”
* Auto-suggest prompts from photoAnalysis (click to append)
* Choose style dropdown: `nostalgic | warm | formal | folk`
* Optional fields: Title (auto-suggested), Year, Tags (chips)

Step 4 — Generate Story

* Button `Tạo câu chuyện` (disabled until at least rawText or photoAnalysis exist)
* Call `/api/memory/generate`
* Show loading skeleton, then `StoryEditor` with:

  * Title (editable)
  * Story text (editable)
  * Tags (AI suggested)
  * Timeline entries (AI suggested)

Step 5 — Review & Save

* Buttons: `Edit` (inline), `Save`, `Export PDF` (calls server to create PDF)
* After Save, redirect to Memory Detail

## C. Memory Detail

* Full story, photos (before/after slider if restored), metadata, createdBy, createdAt
* Buttons: Edit, Delete, Export PDF, Share link (expiring)

---

# 4 — API Spec (ASP.NET Core controllers — full)

> Base: `/api/memories`

### 1) POST `/api/memories/analyze-photo`

* Auth required
* Input: form-data `file: image`, `memberId: string` (optional)
* Output 200:

```json
{
  "photoAnalysisId": "guid",
  "persons": [{"matchedPersonId":"p123","name":"Ông A","confidence":0.89}],
  "scene":"indoor",
  "event":"family_gathering",
  "emotion":"warm",
  "objects":["table","tea_cup"],
  "yearEstimate":"1990s",
  "summary":"Một buổi sum họp gia đình trong căn nhà cũ."
}
```

* Error: 400/413(file too large)/500

### 2) POST `/api/memories/generate`

* Auth required
* Input JSON:

```json
{
  "memberId":"string",
  "photoAnalysisId":"guid|null",
  "rawText":"string", // user input
  "style":"nostalgic|warm|formal|folk",
  "maxWords":500
}
```

* Output 200:

```json
{
  "title":"string",
  "draftStory":"string",
  "tags":["nostalgic","family"],
  "keywords":["ông","tuổi thơ"],
  "timeline":[{"year":1990,"event":"..."}]
}
```

### 3) POST `/api/memories`

* Save memory
* Input:

```json
{
  "memberId":"guild",
  "title":"string",
  "story":"string",
  "photoAnalysisId":"guid|null",
  "photoUrl":"string|null",
  "tags":["string"],
  "createdBy":"userId"
}
```

* Output: saved memory object (with id)

### 4) GET `/api/memories/{memberId}`

* Returns list of memories for member (paged)

### 5) GET `/api/memories/detail/{memoryId}`

* Returns full detail

### 6) DELETE `/api/memories/{memoryId}`

---

# 5 — Data Model (EF Core entities + DTOs)

## Entity: `Member` (exists)

* id (guid), name, birthYear, deathYear, avatarUrl, ...

## Entity: `Memory`

```csharp
public class Memory {
  public Guid Id { get; set; }
  public Guid MemberId { get; set; }
  public string Title { get; set; } // max 120
  public string Story { get; set; } // long text
  public Guid? PhotoAnalysisId { get; set; }
  public string PhotoUrl { get; set; } // optional (restored or original)
  public string[] Tags { get; set; }
  public string[] Keywords { get; set; }
  public DateTime CreatedAt { get; set; }
}
```

## Entity: `PhotoAnalysisResult`

```csharp
public class PhotoAnalysisResult {
  public Guid Id { get; set; }
  public string OriginalUrl { get; set; }
  public string Description { get; set; }
  public string Scene { get; set; } // indoor/outdoor/...
  public string Event { get; set; }
  public string Emotion { get; set; }
  public JsonDocument Faces { get; set; } // array of face objects
  public JsonDocument Objects { get; set; }
  public string YearEstimate { get; set; }
  public DateTime CreatedAt { get; set; }
}
```
---

# 6 — AI Prompts (giữ nguyên, lưu thành files)

> Lưu ở `/ai/prompts/photo_analysis.vi.md` và `/ai/prompts/story_generation.vi.md`

## Prompt: Photo Analysis (VI)

```
You are a family photo analyst. Input: image. Output: JSON only.
Fields:
- persons: [{matchedPersonId|null, name|null, age_est, emotion, bbox}]
- scene: indoor|outdoor|unknown
- event: wedding|funeral|family_gathering|birthday|unknown
- objects: [string]
- yearEstimate: "1970s"|"1990s"|...
- summary: one-sentence vietnamese summary.
Do not write extraneous text.
```

## Prompt: Story Generation (VI)

```
You are a Vietnamese family storyteller. Input:
- rawText: user memory (may be 1-5 sentences)
- photoContext: JSON from photo analysis (may be null)
- style: nostalgic|warm|formal|folk
Produce JSON:
{
  title: short string (max 80 chars),
  story: long vietnamese text (200-600 words), // use "tôi" unless user requested otherwise
  tags: [strings],
  keywords: [strings],
  timeline: [{year:int, event:string}]
}
Rules:
- Expand and enrich detail using sensory description (sight, sound, smell) when possible.
- Keep tone respectful, avoid fabricating major facts (names/dates) not hinted by user/photo.
- Do not hallucinate family relations.
Return JSON only.
```

---

# 7 — Frontend Component Contracts (props, events) — Vue

### `MemoryCreate.vue`

* Props: `memberId`
* Emits: `saved` (memoryId)
* Internals:

  * UploadImage → calls API analyze
  * rawText state
  * style select
  * onGenerate → call `/api/memories/generate`
  * show `StoryEditor` with generated draft

### `PhotoAnalyzerPreview.vue`

* Props: `analysisResult`
* Shows: persons list, scene, objects, summary
* Emits: `useContext`, `editContext`, `skip`

### `StoryEditor.vue`

* Props: `draft` (title, story, tags, timeline)
* Allows inline edit, autosave draft (localStorage)
* Emits: `save` with final payload

### `MemoryList.vue` & `MemoryDetail.vue`

* Typical list / detail components with thumbnail and before/after slider if restored image exists.

---

# 8 — Validation & Business Rules

* File upload max: 15 MB (reject > 15MB)
* Image types allowed: jpg/jpeg/png/heic
* Raw text min length: 10 chars for generate
* If generate called with neither photoAnalysis nor rawText → 400
* Save: title required, story required
* Rate-limit AI generate: 10 requests/min per user (server side)
* GDPR/Privacy:

  * User must check a consent checkbox before voice/clone (for Voice module later)
  * PhotoAnalysis results flagged as `sensitive` if faces detected and not matched to any known member → show explicit consent to store

---

# 9 — Error handling & edge-cases

* If Vision model fails: fallback → return `summary: null` and allow user to continue with raw text
* If LLM returns partial JSON / malformed: server validates JSON schema and retries once
* Large/low-quality images: return suggestion “upload higher-res” and allow user to accept lower-quality result
* Conflicting data (photo suggests wedding, user says funeral) → UI must show both: Photo suggestion + user override

---

# 10 — Tests / Acceptance Criteria

## Unit tests

* PhotoAnalysisService: returns valid JSON for sample images (mock Vision)
* StoryGenerationService: given sample rawText + photoContext returns JSON with required fields
* MemoriesController: create->get->delete lifecycle


# 11 — Security & Privacy notes (must implement)

* All endpoints require auth (JWT or cookie).
* Photo & analysis results flagged private by default and inherit `IsPrivate` rules.
* Audit log: who generated and saved each memory.
* Delete operation must soft-delete first (flag `isDeleted`) for 30 days.
* Access control: only family members with proper permission can view private memories.

---

# 12 — Deployment / Infra hints

* AI calls: keep async worker queue for heavy tasks (photo analysis, story generation) — return jobId and push websocket/event when done. But for initial MVP you may do synchronous calls with a 10–20s timeout and show spinner.
* Store images in object storage (S3 / MinIO) with signed URLs.
* Use Redis for rate-limiting + short-term draft caches.

---

# 13 — Acceptance / Demo checklist (for release)

* [ ] Upload image + analyze returns meaningful context (scene, summary) for 80% of tested photos
* [ ] Generate story from rawText + photoContext returns coherent VN text
* [ ] UI: user can edit story before save
* [ ] Saved story appears on member profile and timeline
* [ ] Export PDF works with basic template (cover, story, photo)
* [ ] Logs stored and soft-delete works
* [ ] Basic unit tests pass

---

# 14 — Example payloads (useful for Gemini to seed tests)

### Analyze request (form-data)

`file: family1.jpg`, `memberId: "p-001"`

### Analyze response

```json
{
  "photoAnalysisId":"3f8b...","persons":[{"matchedPersonId":"p-001","name":"Ông A","confidence":0.92}],
  "scene":"indoor","event":"family_gathering","emotion":"warm",
  "objects":["table","tea_cup"],"yearEstimate":"1990s","summary":"Buổi sum họp gia đình trong nhà gỗ."
}
```

### Generate request

```json
{
  "memberId":"p-001",
  "photoAnalysisId":"3f8b...",
  "rawText":"Tôi nhớ buổi chiều ông kể chuyện chiến trường...",
  "style":"nostalgic",
  "maxWords":400
}
```

### Generate response

```json
{
  "title":"Chiều Ông Kể Chuyện","draftStory":"Chiều hôm đó...","tags":["nostalgic","family"],"keywords":["chiến trường","ông"],"timeline":[{"year":1972,"event":"Đi bộ đội"}]
}
```

---

# 15 — Gợi ý file structure cho Gemini generate

```
/src
  /api
    memories.controller.cs
    services/
      PhotoAnalysisService.cs
      StoryGenerationService.cs
  /models
    Memory.cs
    PhotoAnalysisResult.cs
  /migrations
  /client
    /components
      MemoryCreate.vue
      PhotoAnalyzerPreview.vue
      StoryEditor.vue
      MemoryList.vue
      MemoryDetail.vue
    /pages
      MemberMemories.vue
  /ai
    prompts/
      photo_analysis.vi.md
      story_generation.vi.md
  /tests
    MemoryControllerTests.cs
    PhotoAnalysisServiceTests.cs
```

---
