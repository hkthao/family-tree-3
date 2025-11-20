* Tạo thư mục `apps/mobile/` trong repo hiện tại
* Khởi tạo Flutter app
* Cấu trúc thư mục
* Tạo UI/UX các screen chính
* Tạo mock API
* Implement tree viewer
* Thêm test + README
* Commit theo chuẩn

# Task: Initialize & implement Flutter mobile app for Family Tree App (mobile-first)

## Goal
Create a **Flutter mobile-first application** inside the existing monorepo, located at `apps/mobile/`.  
The app provides:
- Landing page (home)
- Search watch/member
- Member profile
- Watch detail
- Interactive Family tree (pan/zoom)
- Modern UI/UX targeting younger users
- Mock API integration
- Responsive support for web (but mobile is priority)

Backend API is already completed — this task focuses **only** on Flutter app UI/UX, mock API, and project structure.

---

## Directory to create
Inside the existing repo, create:

```

apps/mobile/

```

Inside this folder, initialize the Flutter app:

```

flutter create --org com.familyapp --platforms=android,ios,web family_tree

````

All generated code must be placed inside `apps/mobile/family_tree/`.

---

## Required Dependencies (edit pubspec.yaml)

```yaml
dependencies:
  flutter:
    sdk: flutter
  flutter_riverpod: ^2.3.0
  http: ^1.1.0
  cached_network_image: ^3.2.0
  flutter_svg: ^1.1.0
  go_router: ^6.0.0
  lottie: ^2.3.0

dev_dependencies:
  flutter_test:
    sdk: flutter
  mockito: ^5.4.0
  flutter_lints: ^2.0.0
````

---

## Folder Structure (must create exactly)

```
lib/
 ├─ main.dart
 ├─ app.dart
 ├─ routes.dart
 ├─ screens/
 │   ├─ home/landing_screen.dart
 │   ├─ search/search_screen.dart
 │   ├─ watch/watch_detail_screen.dart
 │   ├─ member/member_profile_screen.dart
 │   ├─ family/tree_screen.dart
 ├─ widgets/
 │   ├─ search_bar.dart
 │   ├─ result_card.dart
 │   ├─ node_card.dart
 │   ├─ bottom_sheet_filter.dart
 ├─ services/
 │   ├─ api_service.dart
 │   ├─ mock_api.dart
 ├─ models/
 │   ├─ watch.dart
 │   ├─ member.dart
 ├─ providers/
 │   ├─ search_provider.dart
 ├─ themes/
 │   ├─ app_theme.dart
assets/
 ├─ fixtures/
 │  ├─ search_results.json
```

---

## Implement Features

### 1. **Landing Screen**

* Hero search bar
* Trending/featured list (from mock data)
* Modern UI for youth: rounded, soft-shadow, pastel colors

### 2. **Search Screen**

* Debounced search (300ms)
* Uses Riverpod provider
* Shows result list using `ResultCard`
* Supports watch + member in same screen

### 3. **Member Profile Screen**

* Avatar + name + relation
* Mini watch timeline or list (mock data)

### 4. **Watch Detail Screen**

* Image preview
* Brand/year/specification
* Members who own(ed) this watch

### 5. **Family Tree Screen**

* Create basic interactive Family Tree map:

  * Use `InteractiveViewer` for pan/zoom
  * Render nodes using `NodeCard`
  * Render connections using `CustomPainter`
* Placeholder/mock family tree data in `mock_api.dart`

---

## Mock API

Create file:

`assets/fixtures/search_results.json`

Content:

```json
{
  "items": [
    {"id":"w1","type":"watch","title":"Rolex Submariner","brand":"Rolex","year":1967,"thumbnail":"https://example.com/img1.jpg"},
    {"id":"w2","type":"watch","title":"Omega Speedmaster","brand":"Omega","year":1972,"thumbnail":"https://example.com/img2.jpg"},
    {"id":"w3","type":"watch","title":"Seiko 5","brand":"Seiko","year":1995,"thumbnail":"https://example.com/img3.jpg"},
    {"id":"m1","type":"member","name":"Huỳnh Kim Thảo","relation":"Chủ gia đình","avatar":"https://example.com/avatar1.jpg"},
    {"id":"m2","type":"member","name":"Huỳnh Văn A","relation":"Ông nội","avatar":"https://example.com/avatar2.jpg"},
    {"id":"m3","type":"member","name":"Nguyễn Thị B","relation":"Bà ngoại","avatar":"https://example.com/avatar3.jpg"}
  ]
}
```

Implement mock search + member/watch detail calls.

---

## State Management

`search_provider.dart` must:

* Use Riverpod
* Debounce 300ms
* Return `AsyncValue<List<Item>>`

---

## Animations

* `AnimatedSwitcher` for search results
* Lottie for empty state
* Smooth tween transitions between screens

---

## Tests (must be created)

```
test/widget/landing_test.dart
test/unit/search_provider_test.dart
```

Requirements:

* Landing screen loads search bar
* Debounce search triggers mock API
* Test passes with `flutter test`

---

## README (auto-generate)

README must include:

* How to run the mobile app
* How to build web version
* How to switch mock API → real API
* Folder structure explanation

---

## Git Commit Rules

Create commits:

* `feat: init flutter mobile app`
* `feat: add landing and search screens`
* `feat: add family tree viewer`
* `feat: add mock api`
* `test: add widget and unit tests`
* `docs: update README`

---

## Acceptance Criteria

This task is **complete only if all criteria below pass**:

* [ ] Folder `apps/mobile/family_tree` is created with full Flutter project
* [ ] App runs on emulator using mock API
* [ ] Landing + Search + Member + Watch + Tree screens are functional
* [ ] Family tree supports pan/zoom
* [ ] Search is debounced 300ms and loads mock results
* [ ] All tests pass: `flutter test`
* [ ] README auto-generated
* [ ] Code formatted: `dart format .`
* [ ] Lint clean: `dart analyze`

---

## Final Command For Validation (Gemini CLI must run)

```
flutter pub get &&
dart analyze &&
flutter test &&
flutter build web
```

If any step fails → fix automatically.

```