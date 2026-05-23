## EF Core Configuration Rules

- Each module `DbContext` is the central place for model registration.
- Entity/table mapping must live in dedicated `IEntityTypeConfiguration<T>` classes.
- Entity-specific sequences may live beside the entity configuration and be registered from the DbContext.
- Shared infrastructure mappings such as Outbox/Inbox should be applied from the DbContext level.
- Do not hand-edit EF model snapshots except when intentionally resolving migration metadata issues.