# Manual API Regression Checklist

Use a fresh owner UUID for a clean pass. Run the matching requests from `docs/api/requests`.

## Environment

- [ ] PostgreSQL is reachable using the configured development connection.
- [ ] API starts and migrations complete.
- [ ] Swagger loads at `/swagger`.
- [ ] `GET /lsessions` returns `200`.

## Task Planning

Automated by `PCL_API.IntegrationTests/TaskPlanning` (Milestone 2):

- [x] Draft a task; capture `taskId`. (`TaskPlanningQueryTests`)
- [x] Get the task with the correct owner. (`TaskPlanningQueryTests`)
- [x] Get with another owner and expect `404`. (`TaskPlanningQueryTests`)
- [x] Refine title/description. (`TaskPlanningOrganizationTests`)
- [x] Set category and priority. (`TaskPlanningOrganizationTests`)
- [x] Plan the task. (`TaskPlanningLifecycleTests`)
- [x] Confirm it appears in `/tasks/assignable`. (`TaskPlanningQueryTests`)
- [x] Activate, defer, and confirm it returns to Planned. (`TaskPlanningLifecycleTests`)
- [x] Complete a Planned task. (`TaskPlanningLifecycleTests`)
- [x] Attempt to mutate the completed task and expect `409`. (`TaskPlanningLifecycleTests`)
- [x] Cancel a separate Draft task. (`TaskPlanningLifecycleTests`)
- [x] Activate a separate task, cancel without reason, and expect `400`. (`TaskPlanningLifecycleTests`)
- [x] Exercise list filters: status, category, priority, search, and creation range. (`TaskPlanningQueryTests`)

## Session and task assignment

Automated by `PCL_API.IntegrationTests/Session` (Milestone 3):

- [x] Start a Session; capture `sessionId`. (`SessionQueryTests`)
- [x] Attempt a second Active Session for the same owner and expect `409`. (`SessionLifecycleTests`)
- [x] Get and list Sessions. (`SessionQueryTests`)
- [x] Assign a Planned task. (`SessionTaskAssignmentTests`)
- [x] Assign it again and expect `409`. (`SessionTaskAssignmentTests`)
- [x] Process the outbox/inbox and confirm the task status becomes `"Active"`. (`CrossModuleFlowTests`)
- [x] Try assigning a task owned by another owner and expect `409`. (`SessionTaskAssignmentTests`)
- [x] Remove the task assignment. (`SessionTaskAssignmentTests`)
- [x] Remove it again and confirm idempotent `204`. (`SessionTaskAssignmentTests`)
- [x] End the Session. (`SessionLifecycleTests`)
- [x] End it again and expect `400`. (`SessionLifecycleTests`)
- [x] Attempt assignment after ending and expect `400`. (`SessionTaskAssignmentTests`)

## Evidence items

Automated by `PCL_API.IntegrationTests/Evidence` (Milestone 4):

- [x] Start a fresh Active Session. (`EvidenceItemTests`)
- [x] Add Note evidence; capture `evidenceItemId`. (`EvidenceItemTests`)
- [x] Add a valid HTTPS Link. (`EvidenceItemTests`)
- [x] Add an invalid Link and expect `400`. (`EvidenceItemTests`)
- [x] Attempt generic FileReference creation and expect `409`. (`EvidenceItemTests`)
- [x] List evidence and confirm both valid items. (`EvidenceItemTests`)
- [x] Remove one item. (`EvidenceItemTests`)
- [x] List with `includeRemoved=false` and confirm it is absent. (`EvidenceItemTests`)
- [x] List with `includeRemoved=true` and confirm removal metadata. (`EvidenceItemTests`)
- [x] Attempt removal using another owner and expect `409`. (`EvidenceItemTests`)
- [x] End the Session. (`EvidenceItemTests`)
- [x] Attempt to add evidence and expect `409`. (`EvidenceItemTests`)

## Evidence storage

Automated by `PCL_API.IntegrationTests/Evidence` (Milestone 5):

- [x] Create a Local profile under an allowed root. (`EvidenceStorageProfileTests`)
- [x] Reuse its name and expect `409`. (`EvidenceStorageProfileTests`)
- [x] Create a Local profile outside allowed roots and expect `400`. (`EvidenceStorageProfileTests`)
- [x] Reject an unavailable Local directory. (`EvidenceStorageProfileTests`)
- [x] List profiles. (`EvidenceStorageProfileTests`)
- [x] Test a valid profile. (`EvidenceStorageProfileTests`)
- [x] Test a missing profile and expect `404`. (`EvidenceStorageProfileTests`)
- [x] Select a valid profile. (`EvidenceStorageProfileTests`)
- [x] Get settings and verify the active profile. (`EvidenceStorageProfileTests`)
- [x] Create, test, and select Amazon S3 profiles through the configurable verifier. (`EvidenceStorageProfileTests`)
- [x] Confirm failed S3 verification prevents selection. (`EvidenceStorageProfileTests`)

## Cross-module flows

Automated by `PCL_API.IntegrationTests/Flows` (Milestone 6):

- [x] Assign a Planned task and verify eventual activation through explicit outbox/inbox processing. (`CrossModuleFlowTests`)
- [x] Remove the assignment and confirm the task remains Active. (`CrossModuleFlowTests`)
- [x] Add Evidence during an Active Session and reject it after the Session stops. (`CrossModuleFlowTests`)
- [x] Complete a Task and stop its Session independently. (`CrossModuleFlowTests`)

## Documentation maintenance

- [ ] Endpoint count still matches mapped `IEndpoint` implementations.
- [ ] Every mapped endpoint appears in one module document and one `.http` file.
- [ ] Changed business rules are reflected in affected flows.
- [ ] Known gaps and route/authentication notes still match `Program.cs`.
