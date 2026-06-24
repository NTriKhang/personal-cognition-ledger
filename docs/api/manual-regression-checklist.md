# Manual API Regression Checklist

Use a fresh owner UUID for a clean pass. Run the matching requests from `docs/api/requests`.

## Environment

- [ ] PostgreSQL is reachable using the configured development connection.
- [ ] API starts and migrations complete.
- [ ] Swagger loads at `/swagger`.
- [ ] `GET /lsessions` returns `200`.

## Task Planning

- [ ] Draft a task; capture `taskId`.
- [ ] Get the task with the correct owner.
- [ ] Get with another owner and expect `404`.
- [ ] Refine title/description.
- [ ] Set category and priority.
- [ ] Plan the task.
- [ ] Confirm it appears in `/tasks/assignable`.
- [ ] Activate, defer, and confirm it returns to Planned.
- [ ] Complete a Planned task.
- [ ] Attempt to mutate the completed task and expect `409`.
- [ ] Cancel a separate Draft task.
- [ ] Activate a separate task, cancel without reason, and expect `400`.
- [ ] Exercise list filters: status, category, priority, search, and creation range.

## Session and task assignment

- [ ] Start a Session; capture `sessionId`.
- [ ] Attempt a second Active Session for the same owner and expect `409`.
- [ ] Get and list Sessions.
- [ ] Assign a Planned task.
- [ ] Assign it again and expect `409`.
- [ ] Wait for outbox/inbox processing and confirm the task status becomes `"Active"`.
- [ ] Try assigning a task owned by another owner and expect `409`.
- [ ] Remove the task assignment.
- [ ] Remove it again and confirm idempotent `204`.
- [ ] End the Session.
- [ ] End it again and expect `400`.
- [ ] Attempt assignment after ending and expect `400`.

## Evidence items

- [ ] Start a fresh Active Session.
- [ ] Add Note evidence; capture `evidenceItemId`.
- [ ] Add a valid HTTPS Link.
- [ ] Add an invalid Link and expect `400`.
- [ ] Attempt generic FileReference creation and expect `409`.
- [ ] List evidence and confirm both valid items.
- [ ] Remove one item.
- [ ] List with `includeRemoved=false` and confirm it is absent.
- [ ] List with `includeRemoved=true` and confirm removal metadata.
- [ ] Attempt removal using another owner and expect `409`.
- [ ] End the Session.
- [ ] Attempt to add evidence and expect `409`.

## Evidence storage

- [ ] Create a Local profile under an allowed root.
- [ ] Reuse its name and expect `409`.
- [ ] Create a Local profile outside allowed roots and expect `400`.
- [ ] List profiles.
- [ ] Test a valid profile.
- [ ] Test a missing profile and expect `404`.
- [ ] Select a valid profile.
- [ ] Get settings and verify the active profile.
- [ ] If AWS is configured, repeat create/test/select for Amazon S3.

## Documentation maintenance

- [ ] Endpoint count still matches mapped `IEndpoint` implementations.
- [ ] Every mapped endpoint appears in one module document and one `.http` file.
- [ ] Changed business rules are reflected in affected flows.
- [ ] Known gaps and route/authentication notes still match `Program.cs`.
