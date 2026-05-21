# Learning Session Platform - DDD Domain Design

## Overview

This document defines a Domain-Driven Design view for a backend-first platform that captures learning or work sessions, associated tasks, evidence, artifacts, annotations, and replayable history.

The core business idea is not simple time tracking. It is the structured capture of intention, activity, evidence, and reflection so a session can later be replayed, analyzed, and improved.

---

## Core Domain Perspective

The central business object is the **Session**.

A session represents a bounded period during which a user performs one or more intentional activities and generates evidence such as files, notes, annotations, links, screenshots, or coding-related outputs.

The system should answer these questions well:

- What was the user trying to do
- What actually happened during the session
- What evidence was produced or touched
- What did the user learn or conclude
- How can that session be replayed later

---

## Bounded Contexts

### 1. Session Context

#### Responsibility

The Session Context is the transactional core of the system.

It manages:

- starting a session
- stopping a session
- session lifecycle and state transitions
- session time boundaries
- active session tracking
- session timeline ownership at the business level

#### Key Concepts

- Session
- SessionStatus
- SessionTimeline
- SessionEvent

#### Notes

This context should own the authoritative truth of whether a session is active, paused, stopped, or completed.

---

### 2. Task Planning Context

#### Responsibility

The Task Planning Context manages the user's intended activities.

It handles:

- task declaration
- task categorization
- task templates
- planned objectives
- task-to-session association

#### Key Concepts

- Task
- TaskCategory
- TaskTemplate
- SessionTaskAssignment

#### Notes

A task is about intent.  
A session is about execution.  
This distinction is important and should remain explicit.

---

### 3. Evidence / Artifact Context

#### Responsibility

This context manages the materials and outputs related to a session.

It handles:

- uploaded files
- linked resources
- screenshots
- notes as evidence
- coding evidence in the future
- evidence metadata and storage references

#### Key Concepts

- EvidenceItem
- Artifact
- ArtifactType
- EvidenceSource
- StorageReference

#### Notes

Do not limit this context to uploaded files only.  
Use a broader concept such as **EvidenceItem** so the system can later support:

- PDF
- image
- note
- link
- code snapshot
- commit reference
- terminal summary
- browser capture
- manual checkpoint

This abstraction is critical for future flexibility.

---

### 4. Annotation / Workspace Context

#### Responsibility

This context manages user interactions with evidence.

It handles:

- text highlights
- comments
- markups
- extracted snippets
- attached notes
- positional annotations on artifacts

#### Key Concepts

- Annotation
- Highlight
- Comment
- Snippet
- WorkspaceEdit

#### Notes

Artifact storage and annotation behavior should not be merged into one model.  
The file and the user's interpretation of the file change at different rates and have different rules.

---

### 5. Replay / Timeline Context

#### Responsibility

This context reconstructs a historical view of what happened during a session.

It handles:

- chronological replay
- aggregated session timeline
- ordered events and evidence display
- read-optimized views for playback

#### Key Concepts

- ReplayTimeline
- TimelineProjection
- ReplayFrame
- SessionHistoryView

#### Notes

This context should be read-focused and derived from upstream data.  
It should not own transactional business rules.

---

### 6. Reflection / Learning Insight Context

#### Responsibility

This context captures the user's conclusions and later derived insights.

It handles:

- end-of-session reflection
- session summary
- perceived difficulty
- learning outcomes
- open questions
- future AI-derived insights

#### Key Concepts

- Reflection
- Insight
- LearningOutcome
- FrictionPoint
- ProgressSignal

#### Notes

This context should remain downstream from core transactional data.  
It should consume session facts, not redefine them.

---

### 7. Identity / User Context

#### Responsibility

This context manages the user and application-level preferences.

It handles:

- user profile
- settings
- timezone
- personalization
- privacy preferences
- authentication and authorization if needed

#### Key Concepts

- User
- UserProfile
- Preference
- TimezoneSetting

#### Notes

Even for a solo-user system, this context should remain conceptually separate from Session and Task logic.

---

## Context Relationships

### Session as Upstream Core

The Session Context is upstream to several other contexts because it defines the business container in which activity occurs.

Downstream consumers include:

- Evidence / Artifact
- Replay / Timeline
- Reflection / Learning Insight

### Task Planning and Session

Task Planning supplies intent into Session.  
Session records actual execution.

This is a customer-supplier relationship, not shared ownership.

### Artifact and Annotation

Artifact is upstream to Annotation.  
Annotations cannot exist meaningfully without evidence or artifacts.

### Session and Replay

Replay is downstream of Session and Evidence.  
It consumes business events and builds read models.

### Session and Reflection

Reflection is downstream of Session.  
Reflections are attached to or derived from session completion and user interpretation.

### Shared Kernel

A very small Shared Kernel is acceptable for stable concepts such as:

- IDs
- timestamps
- user reference
- domain event interfaces

Avoid sharing rich business models across bounded contexts.

---

## Communication Patterns

### 1. Synchronous APIs

Use synchronous application APIs for core transactional operations such as:

- start session
- stop session
- attach task to session
- add evidence item
- create annotation
- submit reflection

These are command-oriented interactions.

### 2. Domain Events

Use domain events for cross-context reactions.

Examples:

- SessionStarted
- TaskAttachedToSession
- EvidenceItemAdded
- AnnotationCreated
- SessionStopped
- ReflectionSubmitted

These events can initially be handled in-process inside a modular monolith.

### 3. Asynchronous Background Processing

Use async processing for heavy operations such as:

- OCR
- thumbnail generation
- file indexing
- transcription
- AI summarization
- evidence enrichment

At first, this can be implemented with background jobs in the monolith.  
A broker is not necessary at the beginning.

### 4. Read Model Projection

Replay should consume upstream events and build a denormalized read model optimized for playback and querying.

This is a good candidate for internal CQRS-style separation later.

---

## Upstream / Downstream Summary

### Upstream Contexts

- Session
- Task Planning
- Evidence / Artifact

### Downstream Contexts

- Annotation
- Replay / Timeline
- Reflection / Learning Insight

### Supporting Context

- Identity / User

---

## Potential Challenges

### 1. Session vs Task vs Evidence Boundary

The biggest modeling risk is mixing these concepts.

Recommended separation:

- Session owns execution and lifecycle
- Task owns intention
- Evidence owns proof

If these are merged too early, the model will become unclear.

### 2. Modeling Non-Artifact Work

Coding, thinking, discussion, and problem solving may not naturally produce files.

The solution is to use a generalized **EvidenceItem** model that can represent many kinds of proof, not just uploaded documents.

### 3. Replay Becoming a Second Source of Truth

Replay must remain derived.  
If replay starts owning rules, business logic will be duplicated and inconsistencies will appear.

### 4. Over-Engineering for a Solo Product

These are bounded contexts, not microservices.  
Keep them in a modular monolith.

DDD here should improve language and model clarity, not create operational complexity.

### 5. Event Naming

Use business language for events.  
Prefer `SessionStopped` over technical names like `TimerRowUpdated`.

### 6. AI / Insight Coupling

Future AI features should remain downstream.  
The system's source of truth must be deterministic business data, not generated interpretation.

---

## Recommended Initial Aggregate Focus

A strong starting point is:

### Session Aggregate

Owns:

- session identity
- lifecycle state
- start and stop rules
- attached task references
- key session events

### Evidence Aggregate

Owns:

- evidence item metadata
- storage reference
- evidence type
- relation to session

### Annotation Aggregate

Owns:

- annotation creation
- annotation targeting
- annotation content and position

This is a practical initial decomposition for a backend-first modular monolith.

---

## Final Recommendation

The best domain center for this product is:

**Session as the transactional core, Evidence as the differentiating domain, and Replay / Reflection as downstream read and interpretation models.**

This structure gives you:

- strong business clarity
- room for future intelligence features
- a clean modular monolith foundation
- low operational complexity for a solo developer


 flowchart TD
    U[User Context] --> S[Session Context]
    U --> T[Task Planning Context]
    U --> E[Evidence Artifact Context]

    T -->|declared intention| S
    S -->|session lifecycle events| E
    E -->|artifact reference| A[Annotation Workspace Context]

    S -->|SessionStarted SessionStopped| R[Replay Timeline Context]
    E -->|EvidenceItemAdded| R
    A -->|AnnotationCreated| R

    S -->|session facts| I[Reflection Learning Insight Context]
    E -->|evidence facts| I
    A -->|annotation facts| I

    S -.->|sync API commands| APP[Application Layer]
    T -.->|sync API commands| APP
    E -.->|sync API commands| APP
    A -.->|sync API commands| APP
    I -.->|sync API commands| APP

    S -->|domain events| BUS[Internal Event Bus]
    T -->|domain events| BUS
    E -->|domain events| BUS
    A -->|domain events| BUS

    BUS --> R
    BUS --> I
 