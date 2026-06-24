# Evidence Storage Configuration Implementation Plan

## 1. Purpose

This document defines the V1 implementation plan for configurable evidence file storage.

The API deployment administrator chooses where uploaded evidence files are stored:

- the API server's local filesystem
- an Amazon S3 bucket owned by the PCL deployment

The selected storage configuration is system-wide. It is not a regular user preference and does not belong to an individual PCL user.

Authentication and administrator authorization are outside the scope of this implementation. The configuration API should still be designed as an administrative boundary so authorization can be added later without redesigning the use cases.

---

## 2. Confirmed Decisions

### Deployment model

- PCL is a traditional API web server.
- Local storage means storage on the API server's filesystem.
- Amazon S3 resources belong to the PCL deployment, not to individual users.

### Configuration ownership

- Storage configuration is system-wide.
- Administrators create storage profiles.
- An administrator selects one profile as the active profile.
- The active profile is used for all new evidence file uploads.

### Supported providers

V1 supports:

- `Local`
- `AmazonS3`

Do not introduce a generic `Cloud` provider value. Cloud implementations have different configuration and behavior, so each provider should have an explicit type.

### Deferred behavior

The following are intentionally excluded from V1:

- per-user storage preferences
- user-owned AWS accounts or buckets
- storing AWS access keys or secret keys in PCL
- profile versioning
- profile migration
- moving existing files after changing the active profile
- complex profile deletion and replacement workflows
- physical file deletion when an evidence item is removed
- cleanup background jobs
- administrator authentication and authorization

Evidence removal remains a database soft delete. Physical cleanup will be implemented later as background processing.

---

## 3. Architectural Placement

### Evidence domain

The Evidence module owns:

- evidence file metadata
- upload lifecycle
- the storage location recorded for each file
- routing file operations to the correct storage provider

The Evidence domain must not own:

- AWS credentials
- S3 clients
- filesystem APIs
- physical root directory validation
- presigned URLs
- configuration binding

### Storage administration

Because configuration is system-wide and currently supports only the Evidence module, V1 should keep storage-profile management inside the Evidence module.

This avoids introducing an Identity/User module for a setting that is not user-specific. If storage later becomes a platform-wide shared capability for other modules, the configuration model can be extracted into a dedicated Storage module.

### Dependency direction

```text
Evidence.Presentation
    -> Evidence.Application
    -> Evidence.Domain

Evidence.Infrastructure
    -> Evidence.Application
    -> Evidence.Domain

Amazon S3 SDK and filesystem APIs
    -> Evidence.Infrastructure only
```

Provider-specific SDK types must not appear in Domain, Application commands, or API responses.

---

## 4. Domain Model

## 4.1 Storage provider type

Add a provider enum:

```text
EvidenceStorageProviderType
- Local
- AmazonS3
```

Persist it as a string to preserve database readability and follow existing enum mapping conventions.

## 4.2 Storage profile

Introduce a `StorageProfile` aggregate or entity owned by the Evidence module.

Suggested common fields:

```text
StorageProfile
- Id
- Name
- ProviderType
- IsActive
- CreatedAt
- UpdatedAt
```

V1 invariants:

- Name is required and length-limited.
- Provider type must be defined.
- Only one profile may be active system-wide.
- A profile must contain valid configuration for its provider.
- A profile should be tested successfully before it can become active.

Because activation affects more than one profile, enforcing a single active profile requires both:

- an application transaction that deactivates the current profile and activates the selected profile
- a database constraint or single-row selection model to prevent multiple active profiles

### Preferred active-profile model

Instead of an `IsActive` flag on every profile, use a singleton settings record:

```text
EvidenceStorageSettings
- Id
- ActiveStorageProfileId
- UpdatedAt
```

This naturally represents zero or one active profile and avoids a filtered unique-index dependency.

## 4.3 Provider-specific profile configuration

Use typed provider configuration rather than an unstructured JSON dictionary.

### Local profile

```text
LocalStorageProfileConfiguration
- StorageProfileId
- RootDirectory
```

Rules:

- Root directory is required.
- It must be an absolute path.
- It must resolve to an allowed server-side location.
- The application must verify that the directory exists or can be created.
- The application must verify read and write access before activation.

### Amazon S3 profile

```text
S3StorageProfileConfiguration
- StorageProfileId
- BucketName
- Region
- KeyPrefix
```

Rules:

- Bucket name and region are required.
- Key prefix is optional and normalized without leading traversal segments.
- No access key, secret key, session token, or credential document is stored.
- Connectivity and required permissions must be tested before activation.

Separate relational tables are preferred over a JSON column because the configuration is small, stable, typed, and provider-specific.

## 4.4 Evidence file storage location

Extend `EvidenceFile` so every initialized upload records where it belongs:

```text
EvidenceFile
- StorageProfileId
- StorageProviderType
- ObjectKey
```

`ObjectKey` remains provider-neutral:

- Local: a relative path beneath the configured root directory
- S3: an object key inside the configured bucket

The application generates the object key. Clients must never supply it.

`StorageProviderType` should be stored as a snapshot in addition to `StorageProfileId`. This makes routing explicit and protects file history if profile-management rules change later.

V1 does not support changing the storage location after upload initialization.

## 4.5 Existing upload lifecycle

Keep the current lifecycle:

```text
Pending -> Verifying -> Ready
```

Terminal alternatives remain:

- `Failed`
- `Expired`
- `Cancelled`

`EvidenceItem` should continue to control transitions through its existing methods. Storage providers report facts to the Application layer; the Application layer invokes domain transitions.

---

## 5. Persistence Design

Add tables in the `evidence` schema:

```text
storage_profile
evidence_storage_settings
local_storage_profile_configuration
s3_storage_profile_configuration
```

Add the following columns to `evidence_file`:

```text
StorageProfileId
StorageProviderType
```

Recommended constraints:

- unique storage-profile name
- one provider-configuration row per profile
- foreign key from provider configuration to storage profile
- foreign key from settings to the active profile
- foreign key from evidence file to storage profile
- required provider type on evidence file

Existing file rows need a migration strategy. Before generating the migration, determine whether development data can be reset.

If existing data must be preserved:

1. Create an initial profile representing the existing storage location.
2. Backfill `StorageProfileId` and `StorageProviderType`.
3. Make the new columns non-nullable.

If existing data can be discarded, recreate the Evidence schema and keep the migration simple.

Profiles referenced by evidence files should not be physically deleted in V1. A basic `IsEnabled` field may be used to prevent selection for new uploads while preserving historical references.

---

## 6. Application Abstractions

Add provider-neutral storage abstractions to Evidence Application.

## 6.1 Storage provider

```text
IEvidenceFileStorageProvider
- ProviderType
- CreateUploadRequestAsync(...)
- InspectAsync(...)
- OpenUploadStreamAsync(...) or StoreAsync(...)
- CreateDownloadRequestAsync(...)
```

Deletion can be omitted from the V1 interface until physical cleanup is implemented.

The abstraction must return application-owned DTOs and must not expose AWS SDK types.

## 6.2 Provider resolver

```text
IEvidenceFileStorageProviderResolver
- Resolve(EvidenceStorageProviderType)
```

Responsibilities:

- select the registered infrastructure implementation
- fail clearly when a configured provider is unavailable

## 6.3 Active profile reader

```text
IActiveEvidenceStorageProfileReader
- GetActiveAsync(...)
```

The upload initialization handler uses this abstraction to resolve the system-wide destination.

## 6.4 Profile verifier

```text
IEvidenceStorageProfileVerifier
- VerifyAsync(profile, ...)
```

Verification results should use provider-neutral error codes such as:

- directory unavailable
- directory not writable
- bucket unavailable
- region mismatch
- permission denied
- provider unavailable

Do not return raw provider exceptions or secret-bearing diagnostic data to API clients.

---

## 7. Administrative Use Cases

Add administrative endpoints under a clear route boundary:

```text
/admin/evidence-storage/profiles
```

Authorization is deferred, but route and use-case naming should make the boundary explicit.

## 7.1 Create local profile

```text
POST /admin/evidence-storage/profiles/local
```

Input:

- profile name
- absolute root directory

Flow:

1. Validate input.
2. Normalize and canonicalize the path.
3. Create the profile as disabled or inactive.
4. Verify directory access.
5. Persist the profile.

## 7.2 Create Amazon S3 profile

```text
POST /admin/evidence-storage/profiles/amazon-s3
```

Input:

- profile name
- bucket name
- region
- optional key prefix

Flow:

1. Validate input.
2. Create the profile as inactive.
3. Resolve AWS credentials through the SDK credential chain.
4. Verify bucket access and required permissions.
5. Persist the profile.

## 7.3 List profiles

```text
GET /admin/evidence-storage/profiles
```

Return:

- ID
- name
- provider
- enabled state
- whether it is active
- safe provider configuration
- last verification result and timestamp, if stored

Never return credentials because credentials are not part of the profile.

## 7.4 Test profile

```text
POST /admin/evidence-storage/profiles/{profileId}/test
```

The operation performs a non-destructive connectivity and permission test.

For Local, use a short-lived probe file inside a reserved health-check directory and remove it immediately.

For S3, prefer permission checks that prove the required operations. If a probe object is required, use a reserved prefix and remove it immediately.

## 7.5 Select active profile

```text
PUT /admin/evidence-storage/settings/active-profile
```

Input:

- profile ID

Flow:

1. Load the profile.
2. Verify it is enabled.
3. Run provider verification.
4. Update the singleton settings row in one transaction.
5. Return the active profile summary.

Changing the active profile affects new uploads only.

## 7.6 Get active configuration

```text
GET /admin/evidence-storage/settings
```

Return the active profile or an explicit unconfigured state.

If no profile is active, file-upload initialization must fail with a clear configuration error. Notes and links remain available.

---

## 8. File Upload Application Flow

The existing generic evidence endpoint should continue to handle notes and links.

Add a dedicated file-upload initialization use case:

```text
POST /lsessions/{sessionId}/evidence-items/file-uploads
```

Input:

- owner ID while authentication is deferred
- optional caption
- original file name
- content type
- file size
- SHA-256 checksum

Flow:

1. Validate session attachment eligibility.
2. Load the active storage profile.
3. Resolve the matching storage provider.
4. Generate an upload-attempt ID and server-controlled object key.
5. Call `EvidenceItem.RegisterFileReference`.
6. Call `EvidenceItem.InitializeFileUpload` with the profile and provider snapshot.
7. Save the aggregate.
8. Create the provider-specific upload mechanism.
9. Return a provider-neutral upload response.

If provider request generation fails after persistence, keep the upload `Pending` and allow the client to retry obtaining the upload mechanism for the same attempt.

## 8.1 Local upload

For Local storage, upload bytes through the API:

```text
PUT /lsessions/{sessionId}/evidence-items/{evidenceItemId}/file-upload
```

The endpoint should:

- validate the upload attempt
- stream the request body
- enforce the expected size
- calculate and verify SHA-256
- write to a temporary file
- atomically move the completed file to its final relative path
- start verification and mark the file ready

Do not buffer the complete file in memory.

## 8.2 S3 upload

For Amazon S3, return a short-lived presigned upload request.

The client uploads directly to S3 and then calls:

```text
POST /lsessions/{sessionId}/evidence-items/{evidenceItemId}/file-upload/confirm
```

The confirmation handler:

1. Loads the aggregate.
2. Calls S3 `HeadObject`.
3. Verifies object key, size, content type, checksum, and version information.
4. Calls `StartFileVerification`.
5. Calls `MarkFileReady` when valid.
6. Calls `MarkFileFailed` when the stored object does not match the reservation.

V1 does not require S3 event notifications or reconciliation jobs.

## 8.3 Download

Add an authorized-in-future download use case:

```text
POST /lsessions/{sessionId}/evidence-items/{evidenceItemId}/file/download
```

Behavior:

- only `Ready` files can be downloaded
- Local returns a controlled API streaming response
- S3 returns a short-lived presigned download URL or a provider-neutral redirect response
- internal root directories and object keys should not be exposed unnecessarily

---

## 9. Local Filesystem Security

The local provider must treat the configured root as a security boundary.

Required protections:

- require an absolute configured root
- canonicalize the root during profile verification
- store only generated relative object keys
- reject rooted object keys
- reject traversal segments
- resolve the final path and confirm it remains under the configured root
- do not use the original filename as the physical filename
- store original filename only as metadata
- write outside the application's public web root
- use restricted operating-system permissions
- stream uploads with explicit size limits
- write to a temporary extension or directory before atomic completion
- avoid following untrusted symbolic links or reparse points
- never return physical paths in API responses

The administrator is trusted to choose the root, but configuration mistakes must still fail safely.

---

## 10. Amazon S3 Credential and Security Model

PCL must not define these profile fields:

- access key ID
- secret access key
- session token

Use the AWS SDK default credential chain.

Recommended environments:

- local development: AWS IAM Identity Center or a named development profile
- EC2: instance role
- ECS: task role
- Lambda: execution role
- Kubernetes: workload identity/web identity
- non-AWS hosting: workload federation or IAM Roles Anywhere where practical

Required S3 protections:

- private bucket
- S3 Block Public Access
- least-privilege IAM policy
- permissions scoped to the configured bucket and prefix
- TLS-only access
- server-side encryption
- short-lived presigned requests
- server-generated non-reused object keys
- no presigned URLs in application logs
- checksum verification

The application's AWS identity needs only the actions required by implemented flows, initially expected to include:

```text
s3:PutObject
s3:GetObject
s3:HeadObject
s3:ListBucket only if verification requires it
```

Do not grant object deletion until the cleanup feature is implemented.

---

## 11. Soft Deletion and Retention

Removing an `EvidenceItem` continues to:

- set removal metadata in PostgreSQL
- leave the physical Local or S3 object unchanged
- prevent normal retrieval through active-evidence queries

V1 must not perform filesystem or S3 deletion inside the removal request.

Future cleanup should:

- run asynchronously
- identify removed evidence after the retention period
- route deletion using the stored provider and profile
- be retryable and idempotent
- record cleanup success or failure

No cleanup behavior is part of this implementation.

---

## 12. Configuration and Startup

Application configuration may define provider infrastructure behavior that is not part of an administrator profile, for example:

```text
EvidenceStorage:
  UploadUrlLifetimeMinutes
  DownloadUrlLifetimeMinutes
  Local:
    AllowedRootDirectories
  AmazonS3:
    ServiceUrl
    ForcePathStyle
```

`ServiceUrl` and `ForcePathStyle` support local S3-compatible testing and should not be stored in each production profile unless multiple S3-compatible services become a requirement.

Use typed options with startup validation for deployment-level settings.

Database-backed profiles should be validated when created, tested, or activated rather than loaded as static `IOptions`.

---

## 13. Error Handling

Add stable application errors for:

- storage not configured
- profile not found
- profile disabled
- profile verification failed
- provider not registered
- local path invalid
- local directory unavailable
- S3 bucket unavailable
- S3 permission denied
- upload mechanism unavailable
- stored object mismatch

Infrastructure exceptions should be logged internally and mapped to stable application errors.

Do not expose:

- physical paths
- AWS account details
- credential-chain diagnostics
- presigned URLs in errors
- raw SDK exception messages

---

## 14. Observability

Log structured, non-secret fields:

- storage provider
- storage profile ID
- evidence item ID
- upload attempt ID
- operation name
- result category
- duration

Do not log:

- presigned URLs
- request authorization headers
- local absolute paths
- checksums when unnecessary
- AWS credentials or tokens

Add health information for the active profile later if continuous provider health checks become necessary. V1 only requires explicit profile testing and operation-level failures.

---

## 15. Testing Strategy

## 15.1 Domain tests

Cover:

- valid and invalid provider values
- storage profile invariants
- provider-specific configuration invariants
- file initialization requires a storage location
- storage location cannot change after initialization
- all existing upload lifecycle transitions
- removed evidence cannot initialize a new upload

## 15.2 Application tests

Cover:

- create Local profile
- create S3 profile without credential fields
- profile verification before activation
- selecting the active profile
- no-active-profile behavior
- new uploads use the active profile
- changing the active profile does not alter existing files
- provider resolution
- retry after upload-mechanism generation failure
- Local and S3 confirmation paths
- only ready files can be downloaded

Use fake storage-provider implementations.

## 15.3 Infrastructure tests

Local provider:

- valid upload and download
- path traversal attempts
- rooted object-key rejection
- symlink or reparse-point escape
- interrupted upload cleanup
- checksum mismatch
- size mismatch
- write-permission failure

S3 provider:

- presigned upload generation
- object metadata inspection
- checksum and size verification
- missing bucket
- missing permission
- expired upload request

Use LocalStack, MinIO, or an isolated test bucket for S3 integration tests. Provider differences must be considered if an S3-compatible emulator is used.

## 15.4 API tests

Cover:

- administrative profile endpoints
- explicit unconfigured response
- Local upload streaming
- S3 initialization and confirmation
- soft deletion without physical deletion
- safe error responses

Authorization tests are deferred until authentication is enabled.

---

## 16. Documentation Updates

During implementation, update:

- `docs/core/domain-glossary.md`
  - add Storage Profile, Storage Provider, and Storage Location
- `docs/core/architecture.md`
  - document provider-neutral storage routing
- `docs/core/module-rules.md`
  - only if storage is later extracted into another module
- Evidence lifecycle documentation
  - document Local and S3 upload differences
- deployment documentation
  - document filesystem permissions and AWS workload credentials

---

## 17. Implementation Sequence

### Phase 1: Domain and persistence

1. Add storage provider and profile domain types.
2. Add typed Local and S3 profile configuration.
3. Add singleton active-storage settings.
4. Extend `EvidenceFile` with profile and provider information.
5. Add EF Core configurations.
6. Generate and verify the database migration.
7. Add domain tests.

### Phase 2: Profile administration

1. Add profile repositories.
2. Add provider verification abstractions.
3. Implement create, list, test, and activate use cases.
4. Add administrative endpoints.
5. Add application tests.

### Phase 3: Local storage

1. Implement secure path resolution.
2. Implement streaming upload.
3. Implement file inspection and download.
4. Connect Local uploads to the existing lifecycle.
5. Add infrastructure and API tests.

### Phase 4: Amazon S3

1. Add the AWS S3 SDK to Evidence Infrastructure.
2. Register the S3 client using the default credential chain.
3. Implement profile verification.
4. Implement presigned upload and download requests.
5. Implement object inspection and confirmation.
6. Add S3 integration tests.

### Phase 5: Stabilization

1. Verify profile switching affects only new uploads.
2. Verify existing files route through their recorded provider/profile.
3. Verify soft deletion never deletes physical files.
4. Add structured logging and safe error mapping.
5. Update architecture and deployment documentation.

---

## 18. V1 Completion Criteria

The feature is complete when:

- an administrator can create Local and Amazon S3 profiles
- an administrator can test a profile
- one profile can be selected system-wide
- file upload fails clearly when storage is not configured
- new files record their provider and profile
- Local files are securely uploaded through and downloaded from the API
- S3 files are uploaded and downloaded using short-lived presigned requests
- S3 uses workload credentials rather than stored access keys
- changing the active profile affects only new files
- removing evidence performs only the existing database soft delete
- domain, application, infrastructure, and endpoint tests cover the critical paths

---

## 19. Explicitly Deferred Follow-up Work

- administrator authentication and permissions
- profile edit/version semantics
- profile deletion workflows
- migration of files between profiles
- S3 event notifications
- pending-upload reconciliation
- scheduled expiration processing
- physical deletion and retention jobs
- malware scanning and quarantine
- multipart uploads
- additional cloud providers
- per-user storage selection
- user-owned cloud accounts
