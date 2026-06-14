**1. Current Lifecycle Analysis**
The current model suggests `AttachFile(...)` was intended to create an upload reservation:

- [`AttachFile`](D:/Personal/github.com/KhangStillAlive/personal-cognition-ledger/be/Modules/Evidence/PCL.Modules.Evidence.Domain/EvidenceItems/EvidenceItem.cs:113) creates an `EvidenceFile` in `Pending`.
- [`EvidenceFile`](D:/Personal/github.com/KhangStillAlive/personal-cognition-ledger/be/Modules/Evidence/PCL.Modules.Evidence.Domain/EvidenceItems/EvidenceFile.cs:15) stores the expected metadata, expiry, checksum, version and failure information, but has no methods for transitioning status.
- [`EvidenceFileUploadStatus`](D:/Personal/github.com/KhangStillAlive/personal-cognition-ledger/be/Modules/Evidence/PCL.Modules.Evidence.Domain/EvidenceItems/EvidenceFileUploadStatus.cs:3) only contains `Pending`, `Ready`, and `Failed`.
- Persistence correctly models one file per item, with a unique object key and an expiry index in [`EvidenceFileConfiguration`](D:/Personal/github.com/KhangStillAlive/personal-cognition-ledger/be/Modules/Evidence/PCL.Modules.Evidence.Infrastructure/EvidenceItems/EvidenceFileConfiguration.cs:17).
- The add flow creates `FileReference` items without attaching a file, and the list flow does not return file metadata or status.

Therefore, `AttachFile` should be called when an upload is **initialized**, not after the bytes have reached S3. A clearer name would be `InitiateFileUpload` or `ReserveFileUpload`.

The current `Content` meaning for `FileReference` also needs definition. It should be a caption/description, not an S3 object key.

**2. Recommended Upload Workflow**
1. Client selects a file and sends its name, size, media type and checksum to the API.
2. API authenticates the caller and verifies session ownership, item type, allowed media type and maximum size.
3. API generates an opaque `uploadAttemptId` and server-controlled object key. Never accept the key from the client.
4. API calls `AttachFile`, creating `Pending`, and commits the database transaction.
5. Only after the reservation is persisted, API creates and returns the presigned upload request.
6. Client uploads directly to S3.
7. Client calls the confirmation endpoint after S3 returns success.
8. Backend performs `HeadObject`, verifies size, content type, checksum, object key and version, then transitions the file.
9. An S3 event consumer invokes the same verification process as a fallback.
10. A scheduled reconciliation job expires abandoned uploads and catches missed events.

Persisting before presigning prevents a database failure from leaving a usable upload URL with no corresponding aggregate. If presigning fails after persistence, the client can retry against the existing pending reservation.

This is necessarily a small saga: PostgreSQL and S3 cannot participate in one transaction.

**3. Presigned Endpoint Design**
Prefer a dedicated file-evidence use case rather than allowing the generic add endpoint to create an unattached `FileReference`:

`POST /lsessions/{sessionId}/evidence-items/file-uploads`

The application command would register the `FileReference`, attach the pending file and persist both atomically. The response should contain:

- `evidenceItemId`
- `uploadAttemptId`
- Upload URL and HTTP method
- Required headers or POST form fields
- Expiration time
- Current status

Use an idempotency key so a retry returns the existing reservation rather than attaching a second file.

A presigned `PUT` is valid and simple. For browser uploads where S3 must enforce a maximum size, consider presigned `POST`; its policy supports `content-length-range` and exact key/content constraints. ([docs.aws.amazon.com](https://docs.aws.amazon.com/AmazonS3/latest/API/sigv4-HTTPPOSTConstructPolicy.html))

Other protections:

- Private bucket with Block Public Access
- Short expiry, normally 5–15 minutes
- Server-generated, never-reused object keys
- Exact CORS origins, methods and headers ([docs.aws.amazon.com](https://docs.aws.amazon.com/AmazonS3/latest/userguide/cors.html))
- Signed checksum and content type
- Bucket-default encryption
- Bucket versioning, with `VersionId` persisted
- Quarantine/scanning before making untrusted files downloadable
- Never log presigned URLs; they are bearer credentials ([docs.aws.amazon.com](https://docs.aws.amazon.com/AmazonS3/latest/userguide/using-presigned-url.html))

For files around 100 MB or larger, introduce multipart upload rather than stretching this single-`PUT` model. AWS recommends multipart uploads at that size. ([docs.aws.amazon.com](https://docs.aws.amazon.com/AmazonS3/latest/userguide/mpuoverview.html))

**4. AWS Credential Strategy**
Your understanding is correct: storing long-lived access keys in `.env` should not be the deployment strategy.

- **Local:** Prefer an IAM Identity Center profile created with `aws configure sso`, then select it with `AWS_PROFILE` or development configuration. The SDK receives temporary credentials. ([docs.aws.amazon.com](https://docs.aws.amazon.com/sdkref/latest/guide/access-sso.html))
- **EC2:** Attach an IAM role through an instance profile. The SDK obtains and refreshes temporary credentials through EC2 instance metadata. No access keys in application configuration. ([docs.aws.amazon.com](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/iam-roles-for-amazon-ec2.html))
- **ECS:** Assign an IAM **task role** to the task definition. This is distinct from the task execution role used by ECS itself. The SDK uses the container credential provider. ([docs.aws.amazon.com](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-iam-roles.html))

Production configuration still needs non-secret settings such as bucket, region, prefix, URL lifetime, size limits and optional KMS key. It should not contain `AccessKey` or `SecretKey`.

The .NET SDK credential chain checks explicit credentials and environment variables before profiles, ECS container credentials and EC2 metadata. Consequently, stale production environment keys can accidentally override the role and should be absent. ([docs.aws.amazon.com](https://docs.aws.amazon.com/sdk-for-net/v4/developer-guide/creds-assign.html))

**5. Upload Status Lifecycle**
Recommended statuses:

`Pending → Verifying → Ready`

Terminal alternatives:

- `Expired`: no verified object before the deadline
- `Failed` or `Rejected`: object exists but validation/scanning failed
- `Cancelled`: explicitly abandoned by the user

Do not use `Uploading` as an authoritative backend state because the backend cannot observe browser upload progress.

Current `EvidenceFile` needs domain transitions for verification, readiness, failure, expiration and cancellation. `UploadedAt`, `VersionId`, and `FailureReason` already anticipate some of these operations.

Failure handling:

- URL generated but unused: remain `Pending`; reconciliation marks `Expired`.
- Single `PUT` interrupted: no usable partial object; client retries until expiry.
- Browser closes before upload finishes: eventually expire.
- Browser closes after S3 succeeds but before confirmation: S3 event or reconciliation detects the object.
- Multipart upload interrupted: preserve resumable parts, but configure S3 lifecycle to abort abandoned multipart uploads. ([docs.aws.amazon.com](https://docs.aws.amazon.com/AmazonS3/latest/userguide/mpu-abort-incomplete-mpu-lifecycle-config.html))

**6. Additional Endpoints**
- `POST .../file-uploads`: create item, reserve file and issue upload request.
- `POST .../{itemId}/file-upload/confirm`: verify S3 state; never blindly trust the client.
- `GET .../{itemId}/file-upload`: return status and metadata, but not a reusable upload URL.
- `POST .../{itemId}/file-upload/renew`: create a new attempt after expiry.
- `DELETE .../{itemId}/file-upload`: cancel or abort a pending attempt.
- `POST .../{itemId}/file/download-url`: authorize and presign downloads only for `Ready` files.

S3 events should go through SQS or EventBridge into an idempotent consumer. Notifications are at-least-once and may be duplicated or reordered. ([docs.aws.amazon.com](https://docs.aws.amazon.com/AmazonS3/latest/userguide/EventNotifications.html)) ([docs.aws.amazon.com](https://docs.aws.amazon.com/AmazonS3/latest/userguide/notification-how-to-event-types-and-destinations.html))

**7. Implementation Plan**
1. Finalize `FileReference.Content`, file limits, allowed types, checksum algorithm and replacement policy.
2. Extend the aggregate with explicit upload-state transitions and an upload-attempt identity.
3. Add a storage abstraction and AWS SDK registration using the default credential chain.
4. Implement the initialization, confirmation, status and download flows.
5. Add S3 event processing plus pending-upload reconciliation.
6. Extend evidence read models to include file metadata and status.
7. Define removal retention: soft-delete only, quarantine, or asynchronous S3 deletion through the outbox.
8. Before exposing uploads, derive `OwnerId` from authentication claims rather than request bodies/query strings; authorization is currently not active in [`Program.cs`](D:/Personal/github.com/KhangStillAlive/personal-cognition-ledger/be/PCL_API/Program.cs:55).

No files were changed.

/////

Implementation Plan
Storage abstraction
Add IEvidenceFileStorage in the Evidence Application layer.
Define operations for presigning upload, reading object metadata, presigning download, and deleting objects.
Keep AWS types out of Domain and Application contracts.

S3 infrastructure
Add AWS S3 SDK to Evidence Infrastructure.
Implement the storage abstraction using IAmazonS3.
Add non-secret configuration: bucket, region, key prefix, URL lifetime.
Use the AWS default credential chain instead of configured access keys.

Upload initialization flow
Add POST /lsessions/{sessionId}/evidence-items/file-uploads.
Accept optional caption, filename, MIME type, size, and SHA-256 checksum.
Validate session eligibility.
Generate the server-controlled object key.
Call RegisterFileReference(...).
Call InitializeFileUpload(...).
Save EvidenceItem and EvidenceFile in one database transaction.
Generate and return the presigned upload request.

Direct client upload
Client uploads directly to S3 using the returned URL.
API does not receive or proxy the file content.
Configure S3 CORS for the client origin.

Confirmation flow
Add POST /lsessions/{sessionId}/evidence-items/{itemId}/file-upload/confirm.
Load and authorize the aggregate.
Call HeadObject against S3.
Verify object key, size, content type, checksum and version.
Transition Pending → Verifying → Ready.
Mark mismatches as Failed.

Read/status flow
Extend evidence read models with filename, size, status and timestamps.
Add a dedicated status endpoint only if polling the list endpoint is insufficient.
Never expose the internal S3 object key unnecessarily.

Expiration and recovery
Add a background job that finds Pending uploads past UploadExpiresAt.
Perform one final S3 check.
Mark existing valid objects ready; otherwise mark the upload expired.
Decide whether expired uploads require a new evidence item or can be renewed.

S3 event fallback
Configure S3 ObjectCreated events through SQS/EventBridge.
Add an idempotent consumer that runs the same verification logic.
This handles cases where upload succeeds but the browser closes before confirmation.

Download flow
Add an authorized endpoint that generates a short-lived presigned download URL.
Only permit downloads when status is Ready.

Removal and cleanup
Decide the evidence retention policy.
When an item is removed, publish cleanup work through the outbox.
Delete or quarantine the S3 object asynchronously rather than inside the request.

Security
Derive OwnerId from authentication claims.
Restrict the IAM role to the evidence bucket/prefix.
Enable Block Public Access, encryption, versioning and lifecycle cleanup.
Never log presigned URLs or AWS credentials.

Testing
Domain tests for validation and every legal/illegal transition.
Application tests with a fake storage implementation.
Infrastructure integration tests against LocalStack or an isolated S3 bucket.
Endpoint tests covering authorization, retries and interrupted uploads.