# Audit Logging with Audit.EntityFramework

This project uses [Audit.NET](https://github.com/thepirat000/Audit.NET) with the EntityFramework Core provider to log all database changes.

## Configuration

### Opt-Out Mode

The audit system is configured in **opt-out mode**, which means:
- All entities are audited by default
- You must explicitly ignore entities or properties you don't want to audit

### Audit Log Storage

Audit logs are stored as JSON files in the `AuditLogs/` directory (excluded from version control via `.gitignore`).

File naming pattern: `audit_yyyyMMdd_HHmmss_<guid>.json`

### Custom Fields

Each audit log includes custom fields when available:
- `UserId`: The ID of the currently authenticated user
- `Username`: The username of the currently authenticated user (or `UserId_<id>` if name claim is not available)

## Implementation Details

### AppDbContext

The `AppDbContext` inherits from `AuditDbContext` instead of `DbContext`:

```csharp
[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = true)]
public class AppDbContext : AuditDbContext
```

### Sensitive Data

The `HashedPassword` property in the `User` entity is marked with `[AuditIgnore]` to exclude it from the `ColumnValues` section of the audit log:

```csharp
[AuditIgnore]
public string HashedPassword { get; private set; } = default!;
```

**Note:** The `[AuditIgnore]` attribute only excludes the property from the `ColumnValues` section. The full entity object graph (including ignored properties) may still appear in the `Entity` section of the audit log. This is a design decision in Audit.EntityFramework to maintain a complete picture of the entity state.

## Audit Log Format

Each audit log file contains a JSON object with the following structure:

```json
{
  "EntityFrameworkEvent": {
    "Database": "master",
    "ConnectionId": "...",
    "ContextId": "...",
    "TransactionId": "...",
    "Entries": [
      {
        "Table": "Users",
        "Name": "User",
        "PrimaryKey": { "Id": 1 },
        "Action": "Insert",
        "Entity": { /* Full entity object */ },
        "ColumnValues": { /* Database column values (excludes [AuditIgnore] properties) */ },
        "Valid": true
      }
    ],
    "Result": 1,
    "Success": true
  },
  "EventType": "AppDbContext",
  "Environment": {
    "UserName": "...",
    "MachineName": "...",
    "DomainName": "..."
  },
  "StartDate": "2025-10-25T11:19:40.6651751Z",
  "EndDate": "2025-10-25T11:19:40.6680679Z",
  "Duration": 3,
  "UserId": 1,
  "Username": "john.doe"
}
```

## Querying Audit Logs

### Find all changes by a specific user:

```bash
grep -l '"UserId":123' AuditLogs/*.json
```

### Find all inserts to the Users table:

```bash
grep -l '"Table":"Users".*"Action":"Insert"' AuditLogs/*.json
```

### View a specific audit log:

```bash
cat AuditLogs/audit_20251025_111940_<guid>.json | python3 -m json.tool
```

### Find changes to a specific entity:

```bash
grep -l '"PrimaryKey":{"Id":5}' AuditLogs/*.json
```

## Configuration Files

- **AuditConfiguration.cs**: Contains the Audit.NET setup and configuration
- **MiddlewareConfig.cs**: Initializes the audit system during application startup
- **AppDbContext.cs**: DbContext configured for auditing

## References

- [Audit.NET Documentation](https://github.com/thepirat000/Audit.NET)
- [Audit.EntityFramework Documentation](https://github.com/thepirat000/Audit.NET/blob/master/src/Audit.EntityFramework/README.md)
- [AuditIgnore Attribute](https://github.com/thepirat000/Audit.NET#ignore-properties)
