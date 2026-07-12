# HTTP API reference

This document describes the 32 endpoints mapped by the current PCL API. Executable examples live in [`requests/`](requests/).

## Runtime conventions

- Development HTTP URL: `http://localhost:5266`
- Development HTTPS URL: `https://localhost:7082`
- Swagger in Development: `/swagger`
- Routes are mounted at the root. Use `/tasks` and `/lsessions`, not `/api/tasks` or `/api/lsessions`.
- Request timestamps described as optional default to server UTC when omitted.
- JSON enum input may be numeric. The `.http` files use numeric values where appropriate.
- Failures use RFC 7807-style `ProblemDetails`.

Typical error:

```json
{
  "title": "Task.CannotActivate",
  "status": 409,
  "detail": "Only planned tasks can be activated."
}
```

| Error category | HTTP status |
| :--- | ---: |
| Validation/problem | 400 |
| Not found | 404 |
| Conflict | 409 |
| Unexpected failure | 500 |

## Security status

The host currently does not enforce authentication or authorization. `UseAuthorization()` is disabled, endpoints do not require authorization, and owner identity is supplied in bodies or query parameters.

`/admin` is currently only part of a route name; it does not establish an administrative security boundary. Do not deploy this API as though these routes were protected.

## Endpoint catalog

| Module | Endpoints | Executable requests |
| :--- | ---: | :--- |
| Task Planning | 12 | [task-planning.http](requests/task-planning.http) |
| Session | 6 | [session.http](requests/session.http) |
| Evidence | 9 | [evidence.http](requests/evidence.http) |

## Task Planning

Task Planning owns intended work. Mutations and single-resource queries use `ownerId`; an absent Task and an owner mismatch are both exposed as `404`.

### Enumerations

| Type | Values |
| :--- | :--- |
| `TaskStatus` | `0` Draft, `1` Planned, `2` Active, `3` Completed, `4` Cancelled |
| `TaskCategory` | `0` Reading, `1` Coding, `2` Debugging, `3` Practice, `4` Review, `5` Research, `6` Writing |
| `TaskPriority` | `0` Low, `1` Medium, `2` High |

### Endpoints

| Method | Route | Purpose | Success |
| :--- | :--- | :--- | :--- |
| POST | `/tasks/draft` | Create a Draft Task | `201`, Task ID |
| GET | `/tasks` | List owner Tasks with filters | `200`, array |
| GET | `/tasks/assignable` | List Planned Tasks eligible for assignment | `200`, array |
| GET | `/tasks/{taskId}` | Get one owner Task | `200`, detail |
| PUT | `/tasks/{taskId}/details` | Refine title and description | `204` |
| PUT | `/tasks/{taskId}/category` | Set category | `204` |
| PUT | `/tasks/{taskId}/priority` | Set priority | `204` |
| POST | `/tasks/{taskId}/plan` | Draft to Planned | `204` |
| POST | `/tasks/{taskId}/activate` | Planned to Active | `204` |
| POST | `/tasks/{taskId}/defer` | Active to Planned | `204` |
| POST | `/tasks/{taskId}/complete` | Planned/Active to Completed | `204` |
| POST | `/tasks/{taskId}/cancel` | Nonterminal to Cancelled | `204` |

### Create and query

`POST /tasks/draft`

```json
{
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "title": "Document the API",
  "description": "Keep contracts aligned with behavior",
  "createdAt": null
}
```

- `ownerId` and `title` are required.
- Title maximum is 200 characters; description maximum is 4,000.
- The server generates the Task ID.
- The current `Location` response header incorrectly includes `/api`; use the route listed above.

`GET /tasks?ownerId={ownerId}&status={status}&category={category}&priority={priority}&search={text}&createdFrom={timestamp}&createdTo={timestamp}`

`ownerId` is required; filters are optional and combined.

`GET /tasks/assignable?ownerId={ownerId}` returns Planned Tasks only.

`GET /tasks/{taskId}?ownerId={ownerId}` returns Task detail and lifecycle timestamps.

### Organize

`PUT /tasks/{taskId}/details`

```json
{
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "title": "Document and verify the API",
  "description": "Updated description",
  "updatedAt": null
}
```

`PUT /tasks/{taskId}/category`

```json
{ "ownerId": "11111111-1111-1111-1111-111111111111", "category": 6, "updatedAt": null }
```

`PUT /tasks/{taskId}/priority`

```json
{ "ownerId": "11111111-1111-1111-1111-111111111111", "priority": 2, "updatedAt": null }
```

Details, category, and priority may be changed in Draft, Planned, or Active. Terminal Tasks reject mutation.

### Lifecycle bodies

All bodies require `ownerId`; omitted action timestamps default to server UTC.

```json
{ "ownerId": "11111111-1111-1111-1111-111111111111", "plannedAt": null }
```

```json
{ "ownerId": "11111111-1111-1111-1111-111111111111", "activatedAt": null }
```

```json
{ "ownerId": "11111111-1111-1111-1111-111111111111", "deferredAt": null }
```

```json
{
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "completedAt": null,
  "completionNote": "The intended outcome is complete."
}
```

```json
{
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "cancelledAt": null,
  "cancellationReason": "No longer needed."
}
```

Completion and cancellation timestamps cannot predate creation. Notes/reasons have a 2,000-character maximum. Cancelling an Active Task requires a reason.

### Main Task errors

| Code | Status | Meaning |
| :--- | ---: | :--- |
| `Task.NotFound` | 404 | Missing Task or owner mismatch |
| `Task.AlreadyTerminal` | 409 | Completed/Cancelled Task was mutated |
| `Task.CannotPlan` | 409 | Task is not Draft |
| `Task.CannotActivate` | 409 | Task is not Planned |
| `Task.CannotDefer` | 409 | Task is not Active |
| `Task.CannotComplete` | 409 | Task is not Planned or Active |
| `Task.CannotCancel` | 409 | Task is terminal |
| `Task.InvalidCompletionTime` | 400 | Completion predates creation |
| `Task.InvalidCancellationTime` | 400 | Cancellation predates creation |
| `Task.CancellationReasonRequired` | 400 | Active cancellation has no reason |

## Session

Session owns bounded execution and assigned Task IDs.

### Endpoints

| Method | Route | Purpose | Success |
| :--- | :--- | :--- | :--- |
| POST | `/lsessions` | Start an Active Session | `201`, Session ID |
| GET | `/lsessions` | List all Sessions | `200`, array |
| GET | `/lsessions/{id}` | Get one Session | `200`, detail |
| PUT | `/lsessions/{id}/tasks/{taskId}` | Assign an eligible Task | `200` |
| DELETE | `/lsessions/{id}/tasks/{taskId}` | Remove an assignment | `204` |
| PUT | `/lsessions/{id}/end` | Stop a Session | `200` |

### Start and read

`POST /lsessions`

```json
{
  "id": null,
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "title": "API documentation session",
  "startedAt": "2026-06-24T14:00:00Z"
}
```

`id` is accepted but ignored; the server generates it. Owner, title, and start time are required. Only one Active Session is allowed per owner. The current `Location` header incorrectly includes `/api`.

`GET /lsessions` currently returns all Sessions without owner filtering. `GET /lsessions/{id}` returns one Session. Status is `0` Active or `1` Stopped, and the representation includes `assignedTaskIds`.

### Assign and remove Tasks

`PUT /lsessions/{id}/tasks/{taskId}`

```json
{ "assignedAt": null }
```

- Session must be Active.
- Task must exist, share the Session owner, and be Planned or Active.
- A Task cannot be duplicated in a Session or assigned to another Active Session.
- A successful assignment emits an integration event. Planned Task activation is eventual.

`DELETE /lsessions/{id}/tasks/{taskId}` is idempotent. It requires an Active Session. Removal emits a fact, but Task Planning currently does not defer the Task automatically.

### End

`PUT /lsessions/{id}/end`

```json
{ "endedAt": "2026-06-24T15:00:00Z" }
```

End time is required and cannot predate start. Ending twice fails. Ending does not complete assigned Tasks.

### Main Session errors

| Code | Status | Meaning |
| :--- | ---: | :--- |
| `LSession.NotFound` | 404 | Session does not exist |
| `LSession.ActiveSessionAlreadyExists` | 409 | Owner already has an Active Session |
| `LSession.InvalidOwnerId` | 400 | Owner ID is empty |
| `LSession.InvalidTitle` | 400 | Title is missing |
| `LSession.InvalidEndTime` | 400 | End predates start |
| `LSession.AlreadyEnded` | 400 | Session is already stopped |
| `LSession.NotActive` | 400 | Assignment mutation targets a Stopped Session |
| `LSession.TaskAlreadyAssigned` | 409 | Duplicate assignment |
| `LSession.TaskAlreadyAssignedToAnotherActiveSession` | 409 | Task is in another Active Session |
| `TaskAssignmentEligibility.TaskNotFound` | 404 | Task does not exist |
| `TaskAssignmentEligibility.OwnerMismatch` | 409 | Owners differ |
| `TaskAssignmentEligibility.NotAssignable` | 409 | Task status is ineligible |

## Evidence

Evidence owns Session Evidence and deployment-level storage profiles.

### Endpoints

| Method | Route | Purpose | Success |
| :--- | :--- | :--- | :--- |
| POST | `/lsessions/{sessionId}/evidence-items` | Add Note or Link Evidence | `201`, Evidence ID |
| GET | `/lsessions/{sessionId}/evidence-items` | List Session Evidence | `200`, array |
| DELETE | `/lsessions/{sessionId}/evidence-items/{evidenceItemId}` | Soft-remove Evidence | `204` |
| POST | `/lsessions/{sessionId}/evidence-items/file-uploads` | Initialize a file upload | `201` |
| PUT | `/lsessions/{sessionId}/evidence-items/{evidenceItemId}/file-upload/content` | Upload Local file bytes | `204` |
| POST | `/lsessions/{sessionId}/evidence-items/{evidenceItemId}/file-upload/confirm` | Verify an upload | `200` |
| GET | `/lsessions/{sessionId}/evidence-items/{evidenceItemId}/file-upload` | Get upload status | `200` |
| POST | `/lsessions/{sessionId}/evidence-items/{evidenceItemId}/file-upload/renew` | Renew an expired or failed upload | `200` |
| DELETE | `/lsessions/{sessionId}/evidence-items/{evidenceItemId}/file-upload` | Cancel a pending upload | `200` |
| GET | `/lsessions/{sessionId}/evidence-items/{evidenceItemId}/file` | Download a Ready file | `200` or `302` |
| POST | `/admin/evidence-storage/profiles/local` | Create and verify Local profile | `201`, profile ID |
| POST | `/admin/evidence-storage/profiles/amazon-s3` | Create and verify S3 profile | `201`, profile ID |
| GET | `/admin/evidence-storage/profiles` | List profiles | `200`, array |
| POST | `/admin/evidence-storage/profiles/{id}/test` | Verify connectivity | `204` |
| PUT | `/admin/evidence-storage/settings/active-profile` | Verify and select profile | `200`, profile |
| GET | `/admin/evidence-storage/settings` | Get active setting | `200`, setting |

### Evidence items

`POST /lsessions/{sessionId}/evidence-items`

```json
{
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "type": "Note",
  "content": "Documented the current API behavior.",
  "addedAt": null
}
```

- Session must exist, share `ownerId`, and be Active.
- Note content is required and limited to 10,000 characters.
- Link content is required, limited to 2,048 characters, and must be absolute HTTP/HTTPS.
- Generic FileReference creation returns `EvidenceItem.FileReferenceRequiresUploadInitialization`.
- Initialize FileReference Evidence with a required `Idempotency-Key` header plus `ownerId`, optional `caption`, `originalFileName`, allowed `contentType`, `fileSizeBytes` (maximum 25 MiB), and a Base64 SHA-256 checksum. Keys are scoped to the owner: an exact replay returns the existing reservation, while different input returns `EvidenceFile.IdempotencyKeyConflict`.
- Local profiles return an `ApiProxy` PUT URL; Amazon S3 returns a `Direct` presigned PUT URL and required headers. Confirmation verifies provider size, content type, and checksum before status becomes `Ready`.
- An `Expired` or `Failed` upload can be renewed with its current attempt ID. Renewal retains the EvidenceItem, archives the old storage identity for cleanup, and returns a fresh attempt ID, object identity, and 15-minute target. Pending uploads can be explicitly cancelled; cancellation is idempotent.
- A one-minute reconciliation job performs a final provider metadata check for expired Pending attempts and marks matching content Ready or the reservation Expired. Terminal and removed-file bytes are deleted asynchronously after seven days.
- The current `Location` header incorrectly includes `/api`.

`GET /lsessions/{sessionId}/evidence-items?ownerId={ownerId}&includeRemoved=false`

Owner is required. The query returns an empty collection when nothing matches and does not separately verify Session existence. Removed items are excluded unless requested.

`DELETE /lsessions/{sessionId}/evidence-items/{evidenceItemId}`

```json
{
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "removedAt": null,
  "removalReason": "Duplicate note."
}
```

The body is required. Removal is soft, owner-checked, route-Session checked, and idempotent. The current code permits removal after the Session stops.

### Local storage profiles

`POST /admin/evidence-storage/profiles/local`

```json
{
  "name": "Development local storage",
  "rootDirectory": "D:\\PCL\\Evidence"
}
```

Name is unique and limited to 100 characters. The path is limited to 2,048 characters, must be below an allowed root, and is verified for creation/write access before the profile is saved.

### Amazon S3 profiles

`POST /admin/evidence-storage/profiles/amazon-s3`

```json
{
  "name": "Development S3",
  "bucketName": "my-evidence-bucket",
  "region": "ap-southeast-1",
  "keyPrefix": "pcl/evidence"
}
```

Bucket, region, and optional normalized prefix are stored. Access is verified with the API process AWS identity. No access keys are accepted or returned.

### List, test, and select profiles

- `GET /admin/evidence-storage/profiles` returns configuration metadata without credentials.
- `POST /admin/evidence-storage/profiles/{id}/test` verifies the configured provider.
- `PUT /admin/evidence-storage/settings/active-profile` accepts `{ "storageProfileId": "..." }`, verifies, and selects it.
- `GET /admin/evidence-storage/settings` returns the active profile and update time, or `null` values when none is selected.

### Main Evidence errors

| Code | Status | Meaning |
| :--- | ---: | :--- |
| `SessionEvidenceAttachmentEligibility.SessionNotFound` | 404 | Session missing during add |
| `SessionEvidenceAttachmentEligibility.OwnerMismatch` | 409 | Owners differ |
| `SessionEvidenceAttachmentEligibility.NotActive` | 409 | Session is stopped |
| `EvidenceItem.InvalidContent` | 400 | Content is missing or oversized |
| `EvidenceItem.InvalidLink` | 400 | Link is not absolute HTTP/HTTPS |
| `EvidenceItem.FileReferenceRequiresUploadInitialization` | 409 | FileReference used through generic add |
| `EvidenceItem.NotFound` | 404 | Item missing or in another route Session |
| `EvidenceItem.OwnerMismatch` | 409 | Another owner attempted removal |
| `StorageProfile.NameAlreadyExists` | 409 | Profile name is duplicated |
| `StorageProfile.NotFound` | 404 | Profile does not exist |
| `StorageProfile.LocalRootNotAllowed` | 400 | Local path is outside allowed roots |
| `StorageProfile.LocalDirectoryUnavailable` | 400 | Local directory cannot be created/written |
| `StorageProfile.AmazonS3Unavailable` | 400 | Bucket/identity verification failed |
| `StorageProfile.ProviderNotRegistered` | 400 | Provider verifier is unavailable |

## Cross-module workflows

### Plan and execute work

1. Create a Draft Task and plan it.
2. Start a Session for the same owner.
3. Assign the Task to the Session.
4. Session stores the assignment and publishes `TaskAssignedToSession` through its outbox.
5. Task Planning eventually consumes the event and activates a Planned Task.

The development processors run every 30 seconds. A successful assignment may be visible before Task activation.

### Capture Evidence

1. Start an Active Session.
2. Add Note or Link Evidence.
3. List current Evidence.
4. Soft-remove an item and use `includeRemoved=true` to inspect removal metadata.
5. Stop the Session.
6. New Evidence is rejected; removal of existing Evidence remains allowed.

### Complete work

Complete a Planned or Active Task and stop its Session separately. Neither command triggers the other:

```text
Task completion = the intended outcome was satisfied
Session stop    = the execution period ended
```

### Defer unfinished work

Remove the Task from an Active Session, then explicitly defer the Active Task. Assignment removal alone does not change Task status.

### Configure Evidence storage

Create a Local or Amazon S3 profile, test it, select it, and read settings. New file reservations capture the active profile, so later profile changes do not change where an existing EvidenceFile is read or verified.

## Known contract gaps

- Created-response `Location` headers currently include `/api`, while mapped routes do not.
- Session list/get routes are not owner-filtered.
- Authentication and administrative authorization are disabled.
- Interrupted-upload reconciliation, renewal, cancellation, and physical cleanup remain planned follow-up work.

These are documented current behaviors, not recommendations.

## Keeping the reference current

For every endpoint change, update this document, the relevant `.http` file, affected workflow text, integration tests, endpoint counts, and security notes in the same change.
