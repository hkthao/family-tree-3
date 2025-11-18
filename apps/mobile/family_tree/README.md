# Family Tree Mobile App

This is the Flutter mobile application for the Family Tree project. It provides a mobile-first experience for viewing and interacting with family tree data, member profiles, and watch details.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Running the App](#running-the-app)
  - [Building for Web](#building-for-web)
- [Project Structure](#project-structure)
- [API Integration](#api-integration)
- [Testing](#testing)

## Features

- Landing page with hero search bar and trending items.
- Search functionality for watches and members with debouncing.
- Member profile screen with personal information and watch timeline.
- Watch detail screen with image, specifications, and owners.
- Interactive Family Tree viewer with pan and zoom.
- Modern UI/UX with rounded elements, soft shadows, and pastel colors.

## Getting Started

### Prerequisites

- [Flutter SDK](https://flutter.dev/docs/get-started/install) (version 3.10.0 or higher)
- A code editor like VS Code with the Flutter extension.

### Running the App

To run the app on an emulator or a connected device:

1.  Navigate to the `apps/mobile/family_tree` directory:
    ```bash
    cd apps/mobile/family_tree
    ```
2.  Get the project dependencies:
    ```bash
    flutter pub get
    ```
3.  Run the app:
    ```bash
    flutter run
    ```

### Building for Web

To build the web version of the app:

1.  Navigate to the `apps/mobile/family_tree` directory:
    ```bash
    cd apps/mobile/family_tree
    ```
2.  Build the web app:
    ```bash
    flutter build web
    ```
    The output will be in the `build/web` directory.

## Project Structure

The core application code is located in the `lib/` directory, organized as follows:

```
lib/
 ├─ main.dart             # Main entry point of the application
 ├─ app.dart              # Root widget of the application
 ├─ routes.dart           # Defines the application's navigation routes using go_router
 ├─ screens/              # Contains all major screens/pages of the application
 │   ├─ home/landing_screen.dart
 │   ├─ search/search_screen.dart
 │   ├─ watch/watch_detail_screen.dart
 │   ├─ member/member_profile_screen.dart
 │   ├─ family/tree_screen.dart
 ├─ widgets/              # Reusable UI components
 │   ├─ search_bar.dart
 │   ├─ result_card.dart
 │   ├─ node_card.dart
 │   ├─ bottom_sheet_filter.dart
 ├─ services/             # API services and data fetching logic
 │   ├─ api_service.dart  # Abstract interface for API interactions
 │   ├─ mock_api.dart     # Mock implementation of ApiService
 ├─ models/               # Data models (e.g., Watch, Member)
 │   ├─ watch.dart
 │   ├─ member.dart
 ├─ providers/            # Riverpod providers for state management
 │   ├─ search_provider.dart
 ├─ themes/               # Application theme definitions
 │   ├─ app_theme.dart
assets/
 ├─ fixtures/             # Static data, e.g., mock API responses
 │  ├─ search_results.json
```

## API Integration

The application currently uses a `MockApiService` for all data fetching. This allows for independent development of the UI/UX without a live backend.

To switch to a real API, you would:

1.  Implement a new class that extends `ApiService` (e.g., `RealApiService`).
2.  In `lib/providers/search_provider.dart`, change the `apiServiceProvider` to return an instance of your `RealApiService`:

    ```dart
    final apiServiceProvider = Provider<ApiService>((ref) {
      return RealApiService(); // Replace MockApiService with your real implementation
    });
    ```
    You would also need to configure your `RealApiService` with the actual backend endpoint.

## Testing

To run the tests for the application:

1.  Navigate to the `apps/mobile/family_tree` directory:
    ```bash
    cd apps/mobile/family_tree
    ```
2.  Run all tests:
    ```bash
    flutter test
    ```
    This will execute both unit and widget tests.

## Code Formatting and Analysis

To ensure code quality and consistency:

1.  Format the code:
    ```bash
    dart format .
    ```
2.  Analyze the code for any issues:
    ```bash
    dart analyze
    ```