Software Requirements & Architecture Document

Project: Pattern-Based Arabic Text Explorer with Multimedia Support

Version: 1.0
Date: 2025-08-17
Prepared by: System Architect

1. Introduction

1.1 Purpose

This document defines the requirements and architecture for the development of a cross-platform application that:

Identifies predefined Arabic text patterns with ḥarakāt.

Displays occurrences with surrounding context.

Allows linking of images and audio to each occurrence.

Provides playback/repetition functionality for learning and research.

1.2 Platforms

Phase 1 (Primary): Android, Ubuntu Linux Desktop.

Phase 2 (Secondary): iOS, Windows, macOS.

1.3 Technology Stack

Framework: .NET 8/9

UI Framework: Avalonia UI (preferred for Linux support) 
.NET MAUI (if prioritizing Android/iOS/Windows/macOS) may be used in later step.

Language: C#

Database: SQLite (cross-platform, lightweight)

Media: System APIs for playback (local & streaming)

2. Use Cases

UC-2: Search for Patterns

Actors: User, SystemFlow:

User selects predefined Arabic pattern.

System scans text and finds all matches.

System collects surrounding context and stores results in memory.

UC-3: Display Matches

Flow:

System lists all matches.

Each match displays: matched sequence + surrounding snippet.

UC-4: Link Media

Flow:

User selects a match.

User attaches image (file picker) or audio (local file or URL).

System stores reference in SQLite DB.

UC-5: Playback

Flow:

User selects a match.

System plays audio (local/stream).

User can replay, loop, or run full list sequentially.

3. System Architecture

3.1 High-Level Layers

Presentation Layer (Avalonia/.NET MAUI)

Cross-platform UI

RTL text support

Responsive design (mobile vs desktop)

Application Layer

Pattern Search Engine (regex/custom parser)

Media Controller (link, play, repeat)

Result Manager (list, filtering)

Data Layer

SQLite database

Tables:

Texts (id, title, content)

Patterns (id, pattern, description)

Occurrences (id, text_id, pattern_id, snippet, position)

MediaLinks (id, occurrence_id, type, path_or_url)

Media Layer

Local playback (system audio APIs)

Streaming (HttpClient with caching)

4. Non-Functional Requirements

Performance: Fast scanning even for large texts (Qur’anic corpus).

Localization: RTL support, Arabic fonts.

Cross-Platform: Codebase usable across Android/Linux initially.

Extensibility: Easy to add new character patterns.

Offline Support: Full functionality with local files when internet unavailable.

5. Development Plan

Phase 1 (MVP)

Core text loading & pattern search.

Listing of matches with snippets.

Manual media linking (local files).

Local audio playback.

Phase 2

Streaming audio support.

Improved UI (desktop split-view, mobile modals).

Batch playback with loop options.

Phase 3

Multi-platform expansion (iOS/Windows/macOS).

User-defined patterns.

Export/import of linked media and annotations.

6. Open Issues

Final decision: Avalonia (Linux priority) vs .NET MAUI (ecosystem priority).

Audio caching strategy for offline use.

UI design consistency across platforms.

