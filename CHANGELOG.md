# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial scaffolding of ASP.NET Core backend with Clean Architecture.
- MongoDB integration for data persistence.
- ASP.NET Core Identity setup with MongoDB for authentication and authorization.
- CRUD APIs for Families, Members, and Relationships.
- Family Tree API for JSON and PDF generation (with TODO for full PDF implementation).
- Basic CI/CD pipeline for backend and frontend (build, test, lint, Docker build/push).
- Initial documentation files (system_design.md, api_design.md, user_guide.md, developer_guide.md, contribution.md).
- Sample product backlog (backlog_sample.yaml) with user stories and acceptance criteria.
- Initial Git branch strategy (main, develop, docs/init).

### Changed
- Updated project files to target .NET 8.0.
- Migrated database access from Entity Framework Core (PostgreSQL) to MongoDB.
- Refactored `IUser` and `IdentityService` to use `ObjectId` for user IDs.
- Removed example Todo features and their associated tests.
- Updated `ci.yml` to reflect correct solution name and added backend linting.

### Removed
- Default EF Core database initializers and related configurations.
- PostgreSQL related packages and test files.

### Fixed
- Various build errors encountered during setup and migration.
