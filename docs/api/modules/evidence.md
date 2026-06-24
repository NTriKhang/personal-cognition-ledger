# Evidence API

## Purpose

Evidence records proof attached to a Session. The same module also manages deployment-level storage profiles used by the future file upload workflow.

## Enumerations

| Type | Values |
| :--- | :--- |
| `EvidenceItemType` | `1` Note, `2` Link, `3` FileReference |
| Storage provider | `Local`, `AmazonS3` in responses |

## Endpoint catalog

| Method | Route | Purpose | Success |
| :--- | :--- | :--- | :--- |
| POST | `/lsessions/{sessionId}/evidence-items` | Add Note or Link evidence | `201`, evidence ID |
| GET | `/lsessions/{sessionId}/evidence-items` | List session evidence | `200`, array |
| DELETE | `/lsessions/{sessionId}/evidence-items/{evidenceItemId}` | Soft-remove evidence | `204` |
| POST | `/admin/evidence-storage/profiles/local` | Create and verify Local profile | `201`, profile ID |
| POST | `/admin/evidence-storage/profiles/amazon-s3` | Create and verify S3 profile | `201`, profile ID |
| GET | `/admin/evidence-storage/profiles` | List profiles | `200`, array |
| POST | `/admin/evidence-storage/profiles/{id}/test` | Verify profile connectivity | `204` |
| PUT | `/admin/evidence-storage/settings/active-profile` | Verify and select active profile | `200`, profile |
| GET | `/admin/evidence-storage/settings` | Get active storage setting | `200`, setting |

“Admin” is currently only a route name; no admin authorization is enforced.

## Evidence item contracts

### Add evidence

`POST /lsessions/{sessionId}/evidence-items`

```json
{
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "type": "Note",
  "content": "Documented the current API behavior.",
  "addedAt": null
}
```

Rules:

- Session must exist, belong to `ownerId`, and be Active.
- `Note` content is required and limited to 10,000 characters.
- `Link` content is required, limited to 2,048 characters, and must be an absolute HTTP/HTTPS URL.
- `FileReference` is rejected with `EvidenceItem.FileReferenceRequiresUploadInitialization`.
- `addedAt` defaults to server UTC.
- The `Location` header currently says `/api/lsessions/...`, while routes are mounted without `/api`.

### List evidence

`GET /lsessions/{sessionId}/evidence-items?ownerId={ownerId}&includeRemoved=false`

- `ownerId` is required.
- The query filters by both session and owner.
- `includeRemoved` defaults to `false`.
- An empty collection is returned for no matches; this query does not verify the Session exists.

Representative item:

```json
{
  "id": "33333333-3333-3333-3333-333333333333",
  "sessionId": "22222222-2222-2222-2222-222222222222",
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "type": 1,
  "content": "Documented the current API behavior.",
  "addedAt": "2026-06-24T14:15:00Z",
  "removedAt": null,
  "removedBy": null,
  "removalReason": null
}
```

### Remove evidence

`DELETE /lsessions/{sessionId}/evidence-items/{evidenceItemId}`

```json
{
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "removedAt": null,
  "removalReason": "Duplicate note."
}
```

- The body is required even though the method is DELETE.
- Evidence must belong to the route Session.
- Only the evidence owner can remove it.
- Removal is a soft delete and is idempotent.
- `removedAt` cannot precede `addedAt`.
- `removalReason` maximum length is 1000.
- Current code permits removal after the Session is Stopped.

## Storage profile contracts

### Create Local profile

`POST /admin/evidence-storage/profiles/local`

```json
{
  "name": "Development local storage",
  "rootDirectory": "D:\\PCL\\Evidence"
}
```

- Name is required, unique, and limited to 100 characters.
- Root directory is required and limited to 2048 characters.
- Creation verifies the path before saving.
- The path must be under one of `EvidenceStorage:Local:AllowedRootDirectories`.
- Verification creates the directory if needed and checks write access.

### Create Amazon S3 profile

`POST /admin/evidence-storage/profiles/amazon-s3`

```json
{
  "name": "Development S3",
  "bucketName": "my-evidence-bucket",
  "region": "ap-southeast-1",
  "keyPrefix": "pcl/evidence"
}
```

- Name must be unique.
- Bucket name maximum length is 63.
- Region maximum length is 64.
- Key prefix maximum length is 512; slashes are normalized and `..` path segments are rejected.
- Creation verifies bucket access using the deployment's AWS identity before saving.
- No AWS access keys are accepted by the API.

### List, test, and select

- `GET /admin/evidence-storage/profiles` returns configuration metadata without credentials.
- `POST /admin/evidence-storage/profiles/{id}/test` verifies the configured provider.
- `PUT /admin/evidence-storage/settings/active-profile` accepts:

  ```json
  { "storageProfileId": "44444444-4444-4444-4444-444444444444" }
  ```

  It verifies the profile before selecting it.

- `GET /admin/evidence-storage/settings` returns:

  ```json
  {
    "activeProfile": null,
    "updatedAt": null
  }
  ```

  or the selected profile and update time.

## Main error codes

| Code | Status | Meaning |
| :--- | ---: | :--- |
| `SessionEvidenceAttachmentEligibility.SessionNotFound` | 404 | Session missing when adding evidence |
| `SessionEvidenceAttachmentEligibility.OwnerMismatch` | 409 | Session belongs to another owner |
| `SessionEvidenceAttachmentEligibility.NotActive` | 409 | Session is Stopped |
| `EvidenceItem.InvalidContent` | 400 | Missing/oversized content |
| `EvidenceItem.InvalidLink` | 400 | Link is not absolute HTTP/HTTPS |
| `EvidenceItem.FileReferenceRequiresUploadInitialization` | 409 | FileReference used through generic add endpoint |
| `EvidenceItem.NotFound` | 404 | Missing item or wrong route Session |
| `EvidenceItem.OwnerMismatch` | 409 | Another owner attempted removal |
| `StorageProfile.NameAlreadyExists` | 409 | Duplicate profile name |
| `StorageProfile.NotFound` | 404 | Profile missing |
| `StorageProfile.LocalRootNotAllowed` | 400 | Local path outside allowed roots |
| `StorageProfile.LocalDirectoryUnavailable` | 400 | Directory cannot be created/written |
| `StorageProfile.AmazonS3Unavailable` | 400 | Bucket/identity verification failed |
| `StorageProfile.ProviderNotRegistered` | 400 | Provider verifier is unavailable |

## File workflow gap

The domain contains Pending → Verifying → Ready/Failed/Expired/Cancelled file upload states, checksum rules, a 25 MiB limit, and allowed content types. There are currently no mapped HTTP endpoints for that workflow. Manual API coverage can therefore test storage profile configuration, Note evidence, and Link evidence, but not real file upload.
